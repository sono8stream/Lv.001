import { clamp, createCanvas } from './binary'
import {
  buildPageConditionEntries,
  createDefaultContext,
  toCommonNumberVariableIndex,
  WolfDataRepository,
} from './data'
import type {
  CallEventCommand,
  ChangeStringDatabaseCommand,
  ChangeStringCommand,
  ChangeVariableCommand,
  ChoiceCommand,
  KeyInputCommand,
  LabelJumpCommand,
  CommandContext,
  CommonEventData,
  ConditionalForkCommand,
  Direction,
  EventPage,
  LoopStartCommand,
  MovePictureCommand,
  NumberRef,
  PictureEffectCommand,
  PicturePivot,
  ReadPicturePropertyCommand,
  RemovePictureCommand,
  ShowPictureStringCommand,
  ShowMessagePictureCommand,
  ShowPictureCommand,
  ShowWindowPictureCommand,
  StartLocation,
  Updater,
  WolfCommand,
  WolfMapData,
  WolfMapEvent,
} from './types'
import { TILE_SIZE } from './types'

interface RuntimeElements {
  canvas: HTMLCanvasElement
  statusPanel: HTMLPreElement
  debugPanel?: HTMLPreElement
  messageBox: HTMLDivElement
  messageText: HTMLDivElement
  choiceBox: HTMLDivElement
  choiceList: HTMLDivElement
  choiceTitle: HTMLDivElement
  pictureLayer: HTMLDivElement
  errorBox: HTMLDivElement
}

type PictureEntry = HTMLImageElement | HTMLDivElement
interface EventRuntimeState {
  x: number
  y: number
  direction: Direction
  canPass: boolean
  moveSpeed: number
  moveFrequency: number
  moveRouteIndex: number
  moveCooldownRemaining: number
}

type InterpolationToken =
  | { kind: 'self'; index: number }
  | { kind: 'cself'; index: number }
  | { kind: 'db'; database: 'system' | 'changeable' | 'user'; table: number; record: number; field: number }
type InterpolationSegment = string | InterpolationToken

export class WolfRuntime {
  private static readonly COMMAND_STEP_LIMIT = 20000
  private static readonly PLAYER_MOVE_REPEAT_FRAMES = 9
  private static readonly PLAYER_ANIMATION_SETTLE_FRAMES = 6
  private static readonly PICTURE_COORDINATE_SCALE = 10
  private static readonly UNSUPPORTED_BATTLE_EVENT_NAMES = new Set(['◆バトルの発生', 'X◆戦闘処理'])
  private static readonly INTERPOLATION_TOKEN_PATTERN = /\\(self\[(\d+)\]|cself\[(\d+)\]|([ucs])db\[(\d+):(\d+):(\d+)])/g

  private readonly elements: RuntimeElements
  private readonly context: CanvasRenderingContext2D
  private readonly pictureEntries = new Map<number, PictureEntry>()
  private readonly pressedKeys = new Set<string>()
  /** Keys pressed since last consume — not cleared by keyup, only by consumeKey */
  private readonly bufferedPresses = new Set<string>()
  private readonly virtualKeyTimers = new Map<string, number>()
  private readonly currentMapEventVariables = new Map<string, number[]>()
  private readonly triggeredAutoEvents = new Set<string>()
  private readonly stringTemplateCache = new Map<string, readonly InterpolationSegment[]>()
  private readonly eventStates = new Map<string, EventRuntimeState>()

  private repository: WolfDataRepository | null = null
  private currentMap: WolfMapData | null = null
  private startLocation: StartLocation | null = null
  private playerX = 0
  private playerY = 0
  private playerDirection: Direction = 'down'
  private playerAnimationFrame = 1
  private playerAnimationFlip = false
  private nextPlayerMoveTick = 0
  private playerAnimationResetTick = 0
  private eventBusy = false
  private tickCount = 0
  private currentMessageResolver: (() => void) | null = null
  private currentChoiceResolver: ((value: number) => void) | null = null
  private currentChoiceButtons: HTMLButtonElement[] = []
  private currentChoiceIndex = 0
  private playerSpriteSheet: HTMLImageElement | null = null
  private playerSpriteFallback: HTMLCanvasElement

  constructor(elements: RuntimeElements) {
    this.elements = elements
    const context = elements.canvas.getContext('2d')
    if (context === null) {
      throw new Error('2D context is not available.')
    }
    this.context = context
    this.context.imageSmoothingEnabled = false
    this.playerSpriteFallback = createCanvas(TILE_SIZE, TILE_SIZE)
    const fallbackContext = this.playerSpriteFallback.getContext('2d')
    if (fallbackContext !== null) {
      fallbackContext.fillStyle = '#5cc9ff'
      fallbackContext.fillRect(0, 0, TILE_SIZE, TILE_SIZE)
      fallbackContext.fillStyle = '#0b2540'
      fallbackContext.fillRect(2, 2, TILE_SIZE - 4, TILE_SIZE - 4)
    }
    this.setDebugInfo('debug: booting...')
  }

  async boot(): Promise<void> {
    try {
      this.setStatus('Loading databases and common events...')
      this.repository = await WolfDataRepository.create()
      this.initializeSystemUiDefaults()
      this.startLocation = this.repository.getStartLocation()
      this.playerX = this.startLocation.x
      this.playerY = this.startLocation.y

      try {
        this.playerSpriteSheet = await this.repository.loadImage('/Data/CharaChip/[Chara]Hero1_USM.png')
      } catch {
        this.playerSpriteSheet = null
      }

      this.installInput()
      await this.changeMap(this.startLocation.mapId, this.startLocation.x, this.startLocation.y)
      this.setDebugIdle()
      this.render()
      requestAnimationFrame(() => this.loop())
    } catch (error) {
      this.showError(error)
      throw error
    }
  }

  private installInput(): void {
    window.addEventListener('keydown', (event) => {
      this.pressedKeys.add(event.key)
      this.bufferedPresses.add(event.key)
      if (this.currentMessageResolver !== null && this.isConfirmKey(event.key)) {
        event.preventDefault()
        this.resolveCurrentMessage()
        return
      }
      if (this.handleChoiceKey(event.key)) {
        event.preventDefault()
      }
    })

    window.addEventListener('keyup', (event) => {
      this.pressedKeys.delete(event.key)
    })

    this.elements.messageBox.addEventListener('click', () => {
      if (this.currentMessageResolver !== null) {
        this.resolveCurrentMessage()
      }
    })

    document.querySelectorAll<HTMLButtonElement>('[data-virtual-key]').forEach((button) => {
      const key = button.dataset.virtualKey
      if (key === undefined || key.length === 0) {
        return
      }

      const release = () => {
        button.classList.remove('pressed')
        this.releaseVirtualKey(key)
      }

      button.addEventListener('pointerdown', (event) => {
        event.preventDefault()
        button.classList.add('pressed')
        this.pressVirtualKey(key)
      })
      button.addEventListener('pointerup', release)
      button.addEventListener('pointercancel', release)
      button.addEventListener('pointerleave', release)
    })
  }

  private pressVirtualKey(key: string): void {
    if (this.currentMessageResolver !== null && this.isConfirmKey(key)) {
      this.resolveCurrentMessage()
      return
    }

    if (this.handleChoiceKey(key)) {
      return
    }

    this.pressedKeys.add(key)
    this.bufferedPresses.add(key)
    const existingTimer = this.virtualKeyTimers.get(key)
    if (existingTimer !== undefined) {
      window.clearTimeout(existingTimer)
    }
    if (this.isMovementKey(key)) {
      this.virtualKeyTimers.delete(key)
      return
    }
    const timer = window.setTimeout(() => {
      this.releaseVirtualKey(key)
    }, 160)
    this.virtualKeyTimers.set(key, timer)
  }

  private releaseVirtualKey(key: string): void {
    const timer = this.virtualKeyTimers.get(key)
    if (timer !== undefined) {
      window.clearTimeout(timer)
      this.virtualKeyTimers.delete(key)
    }
    this.pressedKeys.delete(key)
  }

  private loop(): void {
    void this.tick()
    requestAnimationFrame(() => this.loop())
  }

  private async tick(): Promise<void> {
    if (this.repository === null || this.currentMap === null) {
      return
    }

    this.tickCount += 1

    if (!this.eventBusy) {
      await this.processAutoEvents()

      if (!this.eventBusy) {
        await this.processEventMovement()
      }

      if (!this.eventBusy) {
        if (this.isMovementKeyDown()) {
          if (this.tickCount >= this.nextPlayerMoveTick) {
            await this.stepPlayer()
            this.nextPlayerMoveTick = this.tickCount + WolfRuntime.PLAYER_MOVE_REPEAT_FRAMES
          }
        } else if (this.isAnyPressed(['z', 'Z', 'Enter', ' '])) {
          this.nextPlayerMoveTick = 0
          this.consumeKey('z')
          this.consumeKey('Z')
          this.consumeKey('Enter')
          this.consumeKey(' ')
          await this.tryInteract()
        } else if (this.isAnyPressed(['Escape', 'Backspace', 'x', 'X'])) {
          this.nextPlayerMoveTick = 0
          this.consumeKey('Escape')
          this.consumeKey('Backspace')
          this.consumeKey('x')
          this.consumeKey('X')
          await this.tryOpenMenu()
        } else {
          this.nextPlayerMoveTick = 0
        }
      }
    }

    if (!this.isMovementKeyDown() && this.tickCount >= this.playerAnimationResetTick) {
      this.playerAnimationFrame = 1
    }

    this.render()
  }

  private isMovementKeyDown(): boolean {
    return this.isAnyPressed(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'w', 'W', 'a', 'A', 's', 'S', 'd', 'D'])
  }

  private isMovementKey(key: string): boolean {
    return ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'w', 'W', 'a', 'A', 's', 'S', 'd', 'D'].includes(key)
  }

  private isAnyPressed(keys: string[]): boolean {
    return keys.some((key) => this.pressedKeys.has(key))
  }

  private consumeKey(key: string): void {
    this.pressedKeys.delete(key)
    this.bufferedPresses.delete(key)
  }

  private async stepPlayer(): Promise<void> {
    if (this.currentMap === null) {
      return
    }

    let nextX = this.playerX
    let nextY = this.playerY
    if (this.isAnyPressed(['ArrowLeft', 'a', 'A'])) {
      this.playerDirection = 'left'
      nextX -= 1
    } else if (this.isAnyPressed(['ArrowRight', 'd', 'D'])) {
      this.playerDirection = 'right'
      nextX += 1
    } else if (this.isAnyPressed(['ArrowUp', 'w', 'W'])) {
      this.playerDirection = 'up'
      nextY -= 1
    } else if (this.isAnyPressed(['ArrowDown', 's', 'S'])) {
      this.playerDirection = 'down'
      nextY += 1
    }

    if (!this.isInsideMap(nextX, nextY)) {
      return
    }

    const contactEvent = this.findEventAt(nextX, nextY)
    const contactPage = contactEvent === null ? null : this.getActivePage(contactEvent)
    const rangeEvent = this.findTriggeredEventAt(nextX, nextY, ['playerContact', 'eventContact'])
    const tilePassable = this.currentMap.movableGrid[nextY][nextX]
    const eventPassable = contactEvent === null || contactPage === null
      ? true
      : this.getEventState(contactEvent, contactPage).canPass

    if (rangeEvent !== null && (!tilePassable || (contactEvent !== null && !eventPassable))) {
      if (contactEvent !== null && rangeEvent.id === contactEvent.id && this.isContactTrigger(contactPage?.triggerType)) {
        await this.runMapEvent(rangeEvent)
      }
      return
    }

    if (!tilePassable || (contactEvent !== null && !eventPassable)) {
      return
    }

    this.playerX = nextX
    this.playerY = nextY
    this.advancePlayerAnimation()

    if (rangeEvent !== null) {
      await this.runMapEvent(rangeEvent)
      return
    }
  }

  private isInsideMap(x: number, y: number): boolean {
    if (this.currentMap === null) {
      return false
    }

    return x >= 0 && y >= 0 && x < this.currentMap.width && y < this.currentMap.height
  }

  private isContactTrigger(triggerType: EventPage['triggerType'] | undefined): boolean {
    return triggerType === 'playerContact' || triggerType === 'eventContact'
  }

  private findTriggeredEventAt(
    x: number,
    y: number,
    triggerTypes: ReadonlyArray<EventPage['triggerType']>,
    excludeEventId?: number,
  ): WolfMapEvent | null {
    if (this.currentMap === null) {
      return null
    }

    return this.currentMap.events.find((event) => {
      if (excludeEventId !== undefined && event.id === excludeEventId) {
        return false
      }
      const page = this.getActivePage(event)
      return page !== null && triggerTypes.includes(page.triggerType) && this.isInsideTriggerRange(event, page, x, y)
    }) ?? null
  }

  private isInsideTriggerRange(event: WolfMapEvent, page: EventPage, x: number, y: number): boolean {
    const state = this.getEventState(event, page)
    return this.isInsideTriggerRangeAt(page, state.x, state.y, x, y)
  }

  private isInsideTriggerRangeAt(page: EventPage, originX: number, originY: number, x: number, y: number): boolean {
    const rangeX = page.rangeExtendX ?? 0
    const rangeY = page.rangeExtendY ?? 0
    return Math.abs(x - originX) <= rangeX && Math.abs(y - originY) <= rangeY
  }

  private async processEventMovement(): Promise<void> {
    if (this.currentMap === null) {
      return
    }

    const mapId = this.currentMap.id
    for (const event of this.currentMap.events) {
      const page = this.getActivePage(event)
      if (page === null || page.moveData.moveType === 0) {
        continue
      }

      const state = this.getEventState(event, page)
      if (state.moveCooldownRemaining > 0) {
        state.moveCooldownRemaining -= 1
        continue
      }

      await this.moveEventStep(event, page, state)
      if (this.currentMap === null || this.currentMap.id !== mapId || this.eventBusy) {
        return
      }
      if (state.moveCooldownRemaining <= 0) {
        state.moveCooldownRemaining = this.getMoveCooldownFrames(state.moveFrequency)
      }
    }
  }

  private async tryInteract(): Promise<void> {
    const facingPosition = this.getFacingPosition()
    const target = this.findTriggeredEventAt(facingPosition.x, facingPosition.y, ['check'])
    if (target === null) {
      return
    }

    const page = this.getActivePage(target)
    if (page?.triggerType === 'check') {
      await this.runMapEvent(target)
    }
  }

  private async tryOpenMenu(): Promise<void> {
    if (this.repository === null || this.currentMap === null) {
      return
    }
    if (this.currentMessageResolver !== null || this.currentChoiceResolver !== null) {
      return
    }

    const commonEvent = this.repository.getCommonEventByName('X[移]メニュー起動')
      ?? this.repository.getCommonEventById(127)
    if (commonEvent === null) {
      return
    }

    const command: CallEventCommand = {
      kind: 'callEvent',
      indent: 0,
      eventLookup: { type: 'name', name: commonEvent.name },
      numberArgs: [],
      hasReturnValue: false,
      returnDestination: null,
    }

    this.eventBusy = true
    const pictureSnapshot = new Map(this.pictureEntries)
    try {
      await this.runCommonEvent(commonEvent, command, {
        mapId: this.currentMap.id,
        eventId: null,
        commonEventId: null,
      })
    } finally {
      this.restorePictureEntries(pictureSnapshot)
      this.eventBusy = false
    }
  }

  private async processAutoEvents(): Promise<void> {
    if (this.currentMap === null) {
      return
    }

    for (const event of this.currentMap.events) {
      const page = this.getActivePage(event)
      if (page === null || page.triggerType !== 'auto') {
        continue
      }

      const signature = `${this.currentMap.id}:${event.id}:${page.pageIndex}`
      if (this.triggeredAutoEvents.has(signature)) {
        continue
      }

      this.triggeredAutoEvents.add(signature)
      await this.runMapEvent(event)
    }
  }

  private async runMapEvent(event: WolfMapEvent): Promise<void> {
    const page = this.getActivePage(event)
    if (page === null) {
      return
    }

    this.eventBusy = true
    try {
      await this.executeCommands(page.commands, {
        mapId: this.currentMap?.id ?? 0,
        eventId: event.id,
        commonEventId: null,
      })
    } finally {
      this.eventBusy = false
    }
  }

  private async executeCommands(commands: WolfCommand[], context: CommandContext): Promise<void> {
    const loops: Array<{ indent: number; startIndex: number; currentCount: number; maxCount: number; isInfinite: boolean }> = []
    let index = 0
    let steps = 0

    try {
      while (index < commands.length) {
        steps += 1
        if (steps > WolfRuntime.COMMAND_STEP_LIMIT) {
          throw new Error(`Command execution exceeded step limit (${WolfRuntime.COMMAND_STEP_LIMIT}) on map ${context.mapId} (event=${context.eventId ?? '-'} common=${context.commonEventId ?? '-'})`)
        }

        const command = commands[index]
        this.setDebugCommand(context, command, index, commands.length)

        switch (command.kind) {
          case 'blank':
          case 'checkpoint':
          case 'debugComment':
          case 'labelSet':
          case 'playSystemSe':
          case 'playSeFile':
          case 'saveSlot':
            index += 1
            break
          case 'message':
            await this.showMessage(this.interpolateString(command.text, context))
            index += 1
            break
          case 'choice': {
            const choiceIndex = await this.showChoices(command)
            const nextIndex = this.findLabelIndex(commands, index, `${command.indent}.${choiceIndex + 2}`)
            index = nextIndex >= 0 ? nextIndex + 1 : index + 1
            break
          }
          case 'conditionalFork': {
            const nextIndex = this.jumpForConditionalFork(commands, index, command, context)
            index = nextIndex
            break
          }
          case 'branchElse': {
            const nextIndex = this.findForkEndIndex(commands, index, command.indent)
            index = nextIndex >= 0 ? nextIndex + 1 : commands.length
            break
          }
          case 'forkBegin': {
            const nextIndex = this.findLabelIndex(commands, index, `${command.indent}.0`)
            index = nextIndex >= 0 ? nextIndex + 1 : commands.length
            break
          }
          case 'forkEnd':
            index += 1
            break
          case 'changeVariable':
            this.applyChangeVariable(command, context)
            index += 1
            break
          case 'changeString':
            this.applyChangeString(command, context)
            index += 1
            break
          case 'changeStringDatabase':
            this.applyChangeStringDatabase(command, context)
            index += 1
            break
          case 'movePosition':
            await this.changeMap(command.mapId, command.x, command.y)
            context.mapId = command.mapId
            index += 1
            break
          case 'callEvent':
            await this.executeCallEvent(command, context)
            index += 1
            break
          case 'keyInput':
            this.assignNumberRef({ kind: 'raw', value: command.targetRaw }, await this.resolveKeyInput(command), context)
            index += 1
            break
          case 'loopStart': {
            const nextIndex = this.enterLoop(command, commands, index, context, loops)
            index = nextIndex
            break
          }
          case 'loopEnd': {
            const nextIndex = this.handleLoopEnd(command, index, loops)
            index = nextIndex
            break
          }
          case 'loopBreak': {
            const nextIndex = this.handleLoopBreak(commands, loops)
            index = nextIndex
            break
          }
          case 'loopContinue': {
            const nextIndex = this.handleLoopContinue(commands, index, loops)
            index = nextIndex
            break
          }
          case 'showPicture':
            await this.showPicture(command)
            index += 1
            break
          case 'showMessagePicture':
            this.showMessagePicture(command, context)
            index += 1
            break
          case 'removePicture':
            if (!this.shouldSkipPictureRemoval(commands, index, command, context)) {
              this.removePicture(this.resolveNumberRef(command.pictureId, context))
            }
            index += 1
            break
          case 'showPictureString':
            await this.showPictureString(command, context, this.getShowPictureStringOverride(commands, index, command, context))
            index += 1
            break
          case 'showWindowPicture':
            this.showWindowPicture(command, context)
            index += 1
            break
          case 'movePicture':
            this.movePicture(command, context)
            index += 1
            break
          case 'readPictureProperty':
            this.readPictureProperty(command, context)
            index += 1
            break
          case 'pictureEffect':
            this.applyPictureEffect(command, context)
            index += 1
            break
          case 'wait':
            await this.waitFrames(command.frames)
            index += 1
            break
          case 'labelJump': {
            const nextIndex = this.findNamedLabelIndex(commands, command, context)
            if (nextIndex < 0) {
              throw new Error(`Label "${this.interpolateString(command.name, context)}" not found on map ${context.mapId} (event=${context.eventId ?? '-'} common=${context.commonEventId ?? '-'})`)
            }
            index = nextIndex + 1
            break
          }
          case 'abortEvent':
            return
          case 'unknown':
            index += 1
            break
        }
      }
    } finally {
      this.setDebugIdle()
    }
  }

  private jumpForConditionalFork(
    commands: WolfCommand[],
    index: number,
    command: ConditionalForkCommand,
    context: CommandContext,
  ): number {
    const matchIndex = command.conditions.findIndex((condition) => this.evaluateCondition(condition.operator, this.resolveNumberRef(condition.left, context), this.resolveNumberRef(condition.right, context)))
    if (matchIndex >= 0) {
      const nextIndex = this.findLabelIndex(commands, index, `${command.indent}.${matchIndex + 1}`)
      return nextIndex >= 0 ? nextIndex + 1 : index + 1
    }

    const elseIndex = this.findBranchElseIndex(commands, index, command.indent)
    if (elseIndex >= 0) {
      return elseIndex + 1
    }

    const endIndex = this.findForkEndIndex(commands, index, command.indent)
    return endIndex >= 0 ? endIndex + 1 : index + 1
  }

  private enterLoop(
    command: LoopStartCommand,
    commands: WolfCommand[],
    index: number,
    context: CommandContext,
    loops: Array<{ indent: number; startIndex: number; currentCount: number; maxCount: number; isInfinite: boolean }>,
  ): number {
    const maxCount = command.isInfinite
      ? Number.POSITIVE_INFINITY
      : this.resolveNumberRef(command.loopCount ?? { kind: 'raw', value: 0 }, context)

    if (!command.isInfinite && maxCount <= 0) {
      return this.findLoopExit(commands, command.indent, index)
    }

    loops.push({
      indent: command.indent,
      startIndex: index,
      currentCount: 1,
      maxCount,
      isInfinite: command.isInfinite,
    })
    return index + 1
  }

  private handleLoopEnd(
    command: { indent: number },
    index: number,
    loops: Array<{ indent: number; startIndex: number; currentCount: number; maxCount: number; isInfinite: boolean }>,
  ): number {
    const loop = loops.at(-1)
    if (loop === undefined || loop.indent !== command.indent) {
      return index + 1
    }

    if (loop.isInfinite || loop.currentCount < loop.maxCount) {
      loop.currentCount += 1
      return loop.startIndex + 1
    }

    loops.pop()
    return index + 1
  }

  private handleLoopBreak(
    commands: WolfCommand[],
    loops: Array<{ indent: number; startIndex: number; currentCount: number; maxCount: number; isInfinite: boolean }>,
  ): number {
    const loop = loops.pop()
    if (loop === undefined) {
      return commands.length
    }
    return this.findLoopExit(commands, loop.indent, loop.startIndex)
  }

  private handleLoopContinue(
    commands: WolfCommand[],
    index: number,
    loops: Array<{ indent: number; startIndex: number; currentCount: number; maxCount: number; isInfinite: boolean }>,
  ): number {
    const loop = loops.at(-1)
    if (loop === undefined) {
      return index + 1
    }

    if (loop.isInfinite || loop.currentCount < loop.maxCount) {
      loop.currentCount += 1
      return loop.startIndex + 1
    }

    loops.pop()
    return this.findLoopExit(commands, loop.indent, loop.startIndex)
  }

  private findLoopExit(commands: WolfCommand[], indent: number, startIndex: number): number {
    for (let cursor = startIndex + 1; cursor < commands.length; cursor += 1) {
      if (commands[cursor].indent === indent) {
        return cursor + 1
      }
    }
    return commands.length
  }

  private async executeCallEvent(command: CallEventCommand, context: CommandContext): Promise<void> {
    if (this.repository === null || this.currentMap === null) {
      return
    }

    if (command.eventLookup.type === 'name') {
      const commonEvent = this.repository.getCommonEventByName(command.eventLookup.name)
      if (commonEvent !== null) {
        await this.runCommonEvent(commonEvent, command, context)
      }
      return
    }

    const resolvedEventId = this.resolveNumberRef(command.eventLookup.rawEventId, context)

    if (resolvedEventId >= 500000) {
      const commonEvent = this.repository.getCommonEventById(resolvedEventId - 500000)
      if (commonEvent !== null) {
        await this.runCommonEvent(commonEvent, command, context)
      }
      return
    }

    const targetEventId = resolvedEventId
    const targetEvent = this.currentMap.events.find((event) => event.id === targetEventId)
    if (targetEvent !== undefined) {
      const page = this.getActivePage(targetEvent)
      if (page !== null) {
        await this.executeCommands(page.commands, {
          mapId: this.currentMap.id,
          eventId: targetEvent.id,
          commonEventId: null,
        })
      }
    }
  }

  private async runCommonEvent(
    commonEvent: CommonEventData,
    command: CallEventCommand,
    context: CommandContext,
  ): Promise<void> {
    const args = command.numberArgs.map((ref) => this.resolveNumberRef(ref, context))
    for (let index = 1; index <= 4; index += 1) {
      commonEvent.numberVariables[index] = 0
    }
    for (let index = 0; index < Math.min(4, args.length); index += 1) {
      commonEvent.numberVariables[index + 1] = args[index]
    }

    if (WolfRuntime.UNSUPPORTED_BATTLE_EVENT_NAMES.has(commonEvent.name)) {
      if (command.hasReturnValue && command.returnDestination !== null) {
        this.assignNumberRef(command.returnDestination, 0, context)
      }
      await this.showMessage('戦闘はまだ Web 版で未対応です。')
      return
    }

    await this.executeCommands(commonEvent.commands, {
      mapId: context.mapId,
      eventId: context.eventId,
      commonEventId: commonEvent.id,
    })

    if (command.hasReturnValue && command.returnDestination !== null && commonEvent.returnValueRaw !== null) {
      this.assignNumberRef(
        command.returnDestination,
        this.resolveNumberRef({ kind: 'raw', value: commonEvent.returnValueRaw }, {
          mapId: context.mapId,
          eventId: context.eventId,
          commonEventId: commonEvent.id,
        }),
        context,
      )
    }
  }

  private applyChangeVariable(command: ChangeVariableCommand, context: CommandContext): void {
    for (const updater of command.updaters) {
      this.applyUpdater(updater, context)
    }
  }

  private applyChangeString(command: ChangeStringCommand, context: CommandContext): void {
    let currentValue: string | null = null
    let sourceString: string | null = null
    let sourceNumber: string | null = null
    let literal: string | null = null
    let replacementText: string | null = null

    const getCurrentValue = (): string => {
      if (currentValue === null) {
        currentValue = this.resolveStringRef(command.targetRaw, context)
      }
      return currentValue
    }

    const getSourceString = (): string => {
      if (sourceString === null) {
        sourceString = command.sourceRaw === null ? '' : this.resolveStringRef(command.sourceRaw, context)
      }
      return sourceString
    }

    const getSourceNumber = (): string => {
      if (sourceNumber === null) {
        sourceNumber = command.sourceRaw === null
          ? ''
          : String(this.resolveNumberRef({ kind: 'raw', value: command.sourceRaw }, context))
      }
      return sourceNumber
    }

    const getLiteral = (): string => {
      if (literal === null) {
        literal = this.interpolateString(command.texts[0] ?? '', context)
      }
      return literal
    }

    const getReplacementText = (): string => {
      if (replacementText === null) {
        replacementText = this.interpolateString(command.texts[1] ?? '', context)
      }
      return replacementText
    }

    let nextValue = getCurrentValue()
    switch (command.opRaw) {
      case 0:
      case 2048:
        nextValue = getLiteral()
        break
      case 1:
        nextValue = getSourceString()
        break
      case 2:
        nextValue = getSourceNumber()
        break
      case 256:
        nextValue = `${getCurrentValue()}${getLiteral()}`
        break
      case 769:
        nextValue = getSourceString()
        break
      case 2304: {
        const from = command.texts[0] ?? ''
        nextValue = from.length === 0 ? getCurrentValue() : getCurrentValue().split(from).join(getReplacementText())
        break
      }
      default:
        if ((command.opRaw & 0x01) > 0 && command.sourceRaw !== null) {
          nextValue = (command.opRaw & 0x100) > 0 ? `${getCurrentValue()}${getSourceString()}` : getSourceString()
        } else if ((command.opRaw & 0x02) > 0 && command.sourceRaw !== null) {
          nextValue = (command.opRaw & 0x100) > 0 ? `${getCurrentValue()}${getSourceNumber()}` : getSourceNumber()
        } else {
          nextValue = (command.opRaw & 0x100) > 0 ? `${getCurrentValue()}${getLiteral()}` : getLiteral()
        }
        break
    }

    this.assignStringRef(command.targetRaw, nextValue, context)
  }

  private applyChangeStringDatabase(command: ChangeStringDatabaseCommand, context: CommandContext): void {
    if (this.repository === null) {
      return
    }

    const value = this.repository.getDatabase(command.database).getString(
      this.resolveNumberRef(command.table, context),
      this.resolveNumberRef(command.record, context),
      this.resolveNumberRef(command.field, context),
    )
    this.assignStringRef(command.targetRaw, value, context)
  }

  private applyUpdater(updater: Updater, context: CommandContext): void {
    if (updater.left.kind === 'db' && this.isStringDatabaseRef(updater.left, context)) {
      this.applyStringDatabaseUpdater(updater, context)
      return
    }

    const leftValue = this.resolveNumberRef(updater.left, context)
    const rightValue1 = this.resolveNumberRef(updater.right1, context)
    const rightValue2 = updater.right2 === null ? 0 : this.resolveNumberRef(updater.right2, context)
    const computedRightValue = this.applyRightOperator(updater.rightOperator, rightValue1, rightValue2)
    const nextValue = this.applyAssignOperator(updater.assignOperator, leftValue, computedRightValue)
    this.assignNumberRef(updater.left, nextValue, context)
  }

  private isStringDatabaseRef(ref: NumberRef, context: CommandContext): ref is Extract<NumberRef, { kind: 'db' }> {
    if (this.repository === null || ref.kind !== 'db') {
      return false
    }

    const table = this.resolveNumberRef(ref.table, context)
    const field = this.resolveNumberRef(ref.field, context)
    return this.repository.getDatabase(ref.database).schemas[table]?.columns[field]?.type === 'string'
  }

  private applyStringDatabaseUpdater(updater: Updater, context: CommandContext): void {
    if (this.repository === null || updater.left.kind !== 'db') {
      return
    }

    const store = this.repository.getDatabase(updater.left.database)
    const table = this.resolveNumberRef(updater.left.table, context)
    const record = this.resolveNumberRef(updater.left.record, context)
    const field = this.resolveNumberRef(updater.left.field, context)
    const currentValue = store.getString(table, record, field)
    const nextValue = this.applyStringAssignOperator(
      updater.assignOperator,
      currentValue,
      this.resolveUpdaterStringValue(updater.right1, context),
    )
    store.setString(table, record, field, nextValue)
  }

  private resolveUpdaterStringValue(ref: NumberRef, context: CommandContext): string {
    if (ref.kind === 'db') {
      if (this.isStringDatabaseRef(ref, context)) {
        return this.repository?.getDatabase(ref.database).getString(
          this.resolveNumberRef(ref.table, context),
          this.resolveNumberRef(ref.record, context),
          this.resolveNumberRef(ref.field, context),
        ) ?? ''
      }
      return String(this.resolveNumberRef(ref, context))
    }

    if (ref.value >= 15000000 || (ref.value >= 1600000 && ref.value % 100 >= 5 && ref.value % 100 <= 9)) {
      return this.resolveStringRef(ref.value, context)
    }

    return String(this.resolveNumberRef(ref, context))
  }

  private applyStringAssignOperator(operator: number, currentValue: string, nextValue: string): string {
    switch (operator) {
      case 1:
        return `${currentValue}${nextValue}`
      default:
        return nextValue
    }
  }

  private applyRightOperator(operator: number, left: number, right: number): number {
    switch (operator) {
      case 1:
        return left - right
      case 2:
        return left * right
      case 3:
        return right === 0 ? left : Math.trunc(left / right)
      case 4:
        return right === 0 ? left : left % right
      case 5:
        return left & right
      case 6:
        return Math.trunc(Math.random() * Math.max(1, right - left + 1)) + left
      case 15:
        return Math.round((Math.atan2(right, left) * 180) / Math.PI)
      default:
        return left + right
    }
  }

  private applyAssignOperator(operator: number, left: number, computedRightValue: number): number {
    switch (operator) {
      case 1:
        return left + computedRightValue
      case 2:
        return left - computedRightValue
      case 3:
        return left * computedRightValue
      case 4:
        return computedRightValue === 0 ? left : Math.trunc(left / computedRightValue)
      case 5:
        return computedRightValue === 0 ? left : left % computedRightValue
      case 6:
        return Math.max(left, computedRightValue)
      case 7:
        return Math.min(left, computedRightValue)
      case 8:
        return Math.abs(computedRightValue)
      case 9:
        return computedRightValue
      case 10:
        return Math.round(Math.sin((computedRightValue * Math.PI) / 180) * 1000)
      case 11:
        return Math.round(Math.cos((computedRightValue * Math.PI) / 180) * 1000)
      default:
        return computedRightValue
    }
  }

  private evaluateCondition(operator: number, left: number, right: number): boolean {
    switch (operator) {
      case 0:
        return left > right
      case 1:
        return left >= right
      case 2:
        return left === right
      case 3:
        return left <= right
      case 4:
        return left < right
      case 5:
        return left !== right
      case 6:
        return (left & right) === right
      default:
        return false
    }
  }

  private resolveNumberRef(ref: NumberRef, context: CommandContext): number {
    if (ref.kind === 'db') {
      if (this.repository === null) {
        return 0
      }
      const store = this.repository.getDatabase(ref.database)
      const table = this.resolveNumberRef(ref.table, context)
      const record = this.resolveNumberRef(ref.record, context)
      const field = this.resolveNumberRef(ref.field, context)
      return store.getInt(table, record, field)
    }

    const rawValue = ref.value
    if (rawValue >= 15000000) {
      const variableIndex = rawValue % 100
      const commonEvent = this.getCommonEvent(Math.trunc((rawValue - 15000000) / 100))
      if (commonEvent === null) {
        return 0
      }
      const resolvedIndex = toCommonNumberVariableIndex(variableIndex)
      return resolvedIndex >= 0 ? commonEvent.numberVariables[resolvedIndex] ?? 0 : 0
    }

    if (rawValue >= 1600000 && context.commonEventId !== null) {
      const commonEvent = this.getCommonEvent(context.commonEventId)
      if (commonEvent === null) {
        return 0
      }
      const resolvedIndex = toCommonNumberVariableIndex(rawValue % 100)
      return resolvedIndex >= 0 ? commonEvent.numberVariables[resolvedIndex] ?? 0 : 0
    }

    if (rawValue >= 1100000 && context.eventId !== null) {
      return this.getMapEventVariable(context.mapId, context.eventId, rawValue % 10)
    }

    return rawValue
  }

  private assignNumberRef(ref: NumberRef, value: number, context: CommandContext): void {
    if (this.repository === null) {
      return
    }

    if (ref.kind === 'db') {
      const store = this.repository.getDatabase(ref.database)
      const table = this.resolveNumberRef(ref.table, context)
      const record = this.resolveNumberRef(ref.record, context)
      const field = this.resolveNumberRef(ref.field, context)
      store.setInt(table, record, field, value)
      return
    }

    const rawValue = ref.value
    if (rawValue >= 15000000) {
      const variableIndex = rawValue % 100
      const commonEvent = this.getCommonEvent(Math.trunc((rawValue - 15000000) / 100))
      if (commonEvent === null) {
        return
      }
      const resolvedIndex = toCommonNumberVariableIndex(variableIndex)
      if (resolvedIndex >= 0) {
        commonEvent.numberVariables[resolvedIndex] = value
      }
      return
    }

    if (rawValue >= 1600000 && context.commonEventId !== null) {
      const commonEvent = this.getCommonEvent(context.commonEventId)
      if (commonEvent === null) {
        return
      }
      const resolvedIndex = toCommonNumberVariableIndex(rawValue % 100)
      if (resolvedIndex >= 0) {
        commonEvent.numberVariables[resolvedIndex] = value
      }
      return
    }

    if (rawValue >= 1100000 && context.eventId !== null) {
      this.setMapEventVariable(context.mapId, context.eventId, rawValue % 10, value)
    }
  }

  private resolveStringRef(rawValue: number, context: CommandContext): string {
    if (this.repository === null) {
      return ''
    }

    if (rawValue >= 15000000) {
      return this.resolveCommonEventVariableText(Math.trunc((rawValue - 15000000) / 100), rawValue % 100)
    }

    if (rawValue >= 1600000 && context.commonEventId !== null) {
      return this.resolveCommonEventVariableText(context.commonEventId, rawValue % 100)
    }

    if (rawValue >= 1100000 && context.eventId !== null) {
      return String(this.getMapEventVariable(context.mapId, context.eventId, rawValue % 10))
    }

    return ''
  }

  private assignStringRef(rawValue: number, value: string, context: CommandContext): void {
    if (this.repository === null) {
      return
    }

    if (rawValue >= 15000000) {
      this.assignCommonEventStringValue(Math.trunc((rawValue - 15000000) / 100), rawValue % 100, value)
      return
    }

    if (rawValue >= 1600000 && context.commonEventId !== null) {
      this.assignCommonEventStringValue(context.commonEventId, rawValue % 100, value)
    }
  }

  private resolveCommonEventVariableText(commonEventId: number, variableId: number): string {
    return this.readCommonEventVariableText(this.getCommonEvent(commonEventId), variableId)
  }

  private assignCommonEventStringValue(commonEventId: number, variableId: number, value: string): void {
    const commonEvent = this.getCommonEvent(commonEventId)
    if (commonEvent === null || variableId < 5 || variableId > 9) {
      return
    }

    commonEvent.stringVariables[variableId - 5] = value
  }

  private getMapEventVariable(mapId: number, eventId: number, fieldIndex: number): number {
    const key = `${mapId}:${eventId}`
    const values = this.currentMapEventVariables.get(key)
    if (values === undefined) {
      return 0
    }
    return values[fieldIndex] ?? 0
  }

  private setMapEventVariable(mapId: number, eventId: number, fieldIndex: number, value: number): void {
    const key = `${mapId}:${eventId}`
    const values = this.currentMapEventVariables.get(key) ?? Array.from({ length: 10 }, () => 0)
    values[fieldIndex] = value
    this.currentMapEventVariables.set(key, values)
  }

  private interpolateString(text: string, context: CommandContext): string {
    if (this.repository === null) {
      return text
    }

    if (!text.includes('\\')) {
      return text
    }

    const segments = this.getInterpolationSegments(text)
    if (segments.length === 1 && typeof segments[0] === 'string') {
      return segments[0]
    }

    const commonEvent = this.getCommonEvent(context.commonEventId)
    const mapEventVariables = context.eventId === null
      ? null
      : this.currentMapEventVariables.get(`${context.mapId}:${context.eventId}`) ?? null
    const databaseStores = {
      system: this.repository.systemDb,
      changeable: this.repository.changeableDb,
      user: this.repository.userDb,
    } as const

    let selfValues: string[] | null = null
    let commonValues: string[] | null = null
    let databaseValues: Map<string, string> | null = null
    const parts: string[] = []

    for (const segment of segments) {
      if (typeof segment === 'string') {
        parts.push(segment)
        continue
      }

      switch (segment.kind) {
        case 'self': {
          const cached = selfValues?.[segment.index]
          if (cached !== undefined) {
            parts.push(cached)
            continue
          }
          const value = mapEventVariables === null ? '0' : String(mapEventVariables[segment.index] ?? 0)
          ;(selfValues ??= [])[segment.index] = value
          parts.push(value)
          break
        }
        case 'cself': {
          const cached = commonValues?.[segment.index]
          if (cached !== undefined) {
            parts.push(cached)
            continue
          }
          const value = commonEvent === null ? '0' : this.readCommonEventVariableText(commonEvent, segment.index)
          ;(commonValues ??= [])[segment.index] = value
          parts.push(value)
          break
        }
        case 'db': {
          const key = `${segment.database}:${segment.table}:${segment.record}:${segment.field}`
          const cached = databaseValues?.get(key)
          if (cached !== undefined) {
            parts.push(cached)
            continue
          }
          const value = databaseStores[segment.database].getString(segment.table, segment.record, segment.field)
          ;(databaseValues ??= new Map()).set(key, value)
          parts.push(value)
          break
        }
      }
    }

    return parts.join('')
  }

  private getCommonEvent(commonEventId: number | null): CommonEventData | null {
    if (this.repository === null || commonEventId === null) {
      return null
    }

    return this.repository.getCommonEventById(commonEventId)
  }

  private readCommonEventVariableText(commonEvent: CommonEventData | null, variableId: number): string {
    if (commonEvent === null) {
      return ''
    }

    if (variableId >= 5 && variableId <= 9) {
      return commonEvent.stringVariables[variableId - 5] ?? ''
    }

    const resolvedIndex = toCommonNumberVariableIndex(variableId)
    return resolvedIndex >= 0 ? String(commonEvent.numberVariables[resolvedIndex] ?? 0) : ''
  }

  private getInterpolationSegments(text: string): readonly InterpolationSegment[] {
    const cached = this.stringTemplateCache.get(text)
    if (cached !== undefined) {
      return cached
    }

    const segments: InterpolationSegment[] = []
    const matcher = new RegExp(WolfRuntime.INTERPOLATION_TOKEN_PATTERN)
    let cursor = 0
    let match = matcher.exec(text)

    while (match !== null) {
      if (match.index > cursor) {
        segments.push(text.slice(cursor, match.index))
      }

      const [, , selfIndex, cselfIndex, dbKind, table, record, field] = match
      if (selfIndex !== undefined) {
        segments.push({ kind: 'self', index: Number(selfIndex) })
      } else if (cselfIndex !== undefined) {
        segments.push({ kind: 'cself', index: Number(cselfIndex) })
      } else {
        segments.push({
          kind: 'db',
          database: dbKind === 'u' ? 'user' : dbKind === 'c' ? 'changeable' : 'system',
          table: Number(table),
          record: Number(record),
          field: Number(field),
        })
      }

      cursor = match.index + match[0].length
      match = matcher.exec(text)
    }

    if (cursor < text.length) {
      segments.push(text.slice(cursor))
    }

    if (segments.length === 0) {
      segments.push(text)
    }

    this.stringTemplateCache.set(text, segments)
    return segments
  }

  private getActivePage(event: WolfMapEvent): EventPage | null {
    if (this.currentMap === null) {
      return null
    }

    const context = createDefaultContext(this.startLocation ?? { mapId: this.currentMap.id, x: this.playerX, y: this.playerY })
    context.mapId = this.currentMap.id
    context.eventId = event.id

    for (let index = event.pages.length - 1; index >= 0; index -= 1) {
      const page = event.pages[index]
      const conditions = buildPageConditionEntries(page)
      const isActive = conditions.every((condition) =>
        this.evaluateCondition(condition.operator, this.resolveNumberRef(condition.left, context), this.resolveNumberRef(condition.right, context)),
      )
      if (isActive) {
        return page
      }
    }

    return event.pages[0] ?? null
  }

  private findEventAt(x: number, y: number, excludeEventId?: number): WolfMapEvent | null {
    if (this.currentMap === null) {
      return null
    }
    return this.currentMap.events.find((event) => {
      if (excludeEventId !== undefined && event.id === excludeEventId) {
        return false
      }
      const page = this.getActivePage(event)
      if (page === null) {
        return false
      }
      const state = this.getEventState(event, page)
      return state.x === x && state.y === y
    }) ?? null
  }

  private getFacingPosition(): { x: number; y: number } {
    let x = this.playerX
    let y = this.playerY
    switch (this.playerDirection) {
      case 'left':
        x -= 1
        break
      case 'right':
        x += 1
        break
      case 'up':
        y -= 1
        break
      case 'down':
        y += 1
        break
    }
    return { x, y }
  }

  private async changeMap(mapId: number, x: number, y: number): Promise<void> {
    if (this.repository === null) {
      return
    }

    this.currentMap = await this.repository.loadMap(mapId)
    this.playerX = clamp(x, 0, this.currentMap.width - 1)
    this.playerY = clamp(y, 0, this.currentMap.height - 1)
    this.playerAnimationFrame = 1
    this.playerAnimationFlip = false
    this.nextPlayerMoveTick = 0
    this.playerAnimationResetTick = 0
    this.tickCount = 0
    this.eventStates.clear()
    this.triggeredAutoEvents.clear()
    this.pictureEntries.forEach((entry) => entry.remove())
    this.pictureEntries.clear()
    for (const event of this.currentMap.events) {
      const key = `${this.currentMap.id}:${event.id}`
      if (!this.currentMapEventVariables.has(key)) {
        this.currentMapEventVariables.set(key, Array.from({ length: 10 }, () => 0))
      }
      const page = this.getActivePage(event)
      if (page !== null) {
        this.getEventState(event, page)
      }
    }
    this.setStatus([
      `mapId: ${this.currentMap.id}`,
      `size: ${this.currentMap.width} x ${this.currentMap.height}`,
      `player: (${this.playerX}, ${this.playerY})`,
      `events: ${this.currentMap.events.length}`,
      `commonEvents: ${this.repository.commonEvents.length}`,
    ].join('\n'))
  }

  private async showPicture(command: ShowPictureCommand): Promise<void> {
    if (this.repository === null) {
      return
    }

    const image = await this.repository.loadImage(command.filePath)
    const entry = document.createElement('img')
    entry.src = image.src
    entry.className = 'picture-entry'
    entry.dataset.baseWidth = String(image.naturalWidth || image.width || 0)
    entry.dataset.baseHeight = String(image.naturalHeight || image.height || 0)
    this.placePictureEntry(entry, command.pictureId, command.x, command.y, command.scale, command.pivot)
  }

  private showMessagePicture(command: ShowMessagePictureCommand, context: CommandContext): void {
    const entry = document.createElement('div')
    entry.className = 'picture-entry'
    const renderedText = this.sanitizePictureText(this.interpolateString(command.message, context))
    const estimatedSize = this.estimatePictureTextSize(renderedText)
    entry.textContent = renderedText
    entry.style.whiteSpace = 'pre'
    entry.dataset.baseWidth = String(estimatedSize.width)
    entry.dataset.baseHeight = String(estimatedSize.height)
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    const resolvedX = this.resolveNumberRef(command.x, context)
    const resolvedY = this.resolveNumberRef(command.y, context)
    const existingEntry = this.pictureEntries.get(pictureId)
    const preserveLayout = this.shouldPreserveReplacementLayout(existingEntry, resolvedX, resolvedY, command.scale)
    this.placePictureEntry(
      entry,
      pictureId,
      preserveLayout ? this.readPictureMetric(existingEntry, 'x') : resolvedX,
      preserveLayout ? this.readPictureMetric(existingEntry, 'y') : resolvedY,
      preserveLayout ? this.readPictureMetric(existingEntry, 'scale') : command.scale,
      command.pivot,
    )
  }

  private async showPictureString(
    command: ShowPictureStringCommand,
    context: CommandContext,
    filePathRawOverride: number | null = null,
  ): Promise<void> {
    if (this.repository === null) {
      return
    }

    const filePath = this.resolveStringRef(filePathRawOverride ?? command.filePathRaw, context)
    if (filePath.length === 0) {
      return
    }

    const image = await this.repository.loadImage(filePath)
    const entry = document.createElement('img')
    entry.src = image.src
    entry.className = 'picture-entry'
    entry.dataset.baseWidth = String(image.naturalWidth || image.width || 0)
    entry.dataset.baseHeight = String(image.naturalHeight || image.height || 0)
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    const resolvedX = this.resolveNumberRef(command.x, context)
    const resolvedY = this.resolveNumberRef(command.y, context)
    const resolvedScale = this.resolveNumberRef(command.scale, context) * 0.01
    const existingEntry = this.pictureEntries.get(pictureId)
    const preserveLayout = this.shouldPreserveReplacementLayout(existingEntry, resolvedX, resolvedY, resolvedScale)
    this.placePictureEntry(
      entry,
      pictureId,
      preserveLayout ? this.readPictureMetric(existingEntry, 'x') : resolvedX,
      preserveLayout ? this.readPictureMetric(existingEntry, 'y') : resolvedY,
      preserveLayout ? this.readPictureMetric(existingEntry, 'scale') : resolvedScale,
      command.pivot,
    )
  }

  private showWindowPicture(command: ShowWindowPictureCommand, context: CommandContext): void {
    const entry = document.createElement('div')
    entry.className = 'picture-entry'
    const resolvedWidth = this.resolveNumberRef(command.width, context)
    const resolvedHeight = this.resolveNumberRef(command.height, context)
    const opacity = this.resolveNumberRef(command.opacity, context)
    const renderedText = this.sanitizePictureText(this.interpolateString(command.message, context))
    const fallbackSize = this.estimatePictureTextSize(renderedText)
    const width = resolvedWidth > 1 ? resolvedWidth : fallbackSize.width
    const height = resolvedHeight > 1 ? resolvedHeight : fallbackSize.height
    entry.textContent = renderedText
    entry.style.width = `${Math.max(0, width)}px`
    entry.style.height = `${Math.max(0, height)}px`
    entry.style.opacity = `${Math.max(0, Math.min(255, opacity)) / 255}`
    entry.style.boxSizing = 'border-box'
    entry.style.border = '1px solid rgba(255,255,255,0.7)'
    entry.style.background = renderedText.length === 0
      ? 'linear-gradient(180deg, rgba(50, 80, 120, 0.9), rgba(15, 25, 45, 0.9))'
      : 'rgba(15, 25, 45, 0.82)'
    entry.style.color = '#fff'
    entry.style.whiteSpace = 'pre'
    entry.style.padding = '2px 4px'
    entry.dataset.baseWidth = String(Math.max(0, width))
    entry.dataset.baseHeight = String(Math.max(0, height))
    this.placePictureEntry(
      entry,
      this.resolveNumberRef(command.pictureId, context),
      this.resolveNumberRef(command.x, context),
      this.resolveNumberRef(command.y, context),
      this.resolveNumberRef(command.scale, context) * 0.01,
      command.pivot,
    )
  }

  private movePicture(command: MovePictureCommand, context: CommandContext): void {
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    const entry = this.pictureEntries.get(pictureId)
    if (entry === undefined) {
      return
    }

    const currentLeft = this.readPictureMetric(entry, 'x')
    const currentTop = this.readPictureMetric(entry, 'y')
    const currentScale = this.readPictureMetric(entry, 'scale')
    const nextLeft = this.resolveNumberRef(command.x, context)
    const nextTop = this.resolveNumberRef(command.y, context)
    const nextScaleRaw = this.resolveNumberRef(command.scale, context)

    const anchorX = nextLeft <= -1000000 ? currentLeft : nextLeft
    const anchorY = nextTop <= -1000000 ? currentTop : nextTop
    const scale = nextScaleRaw <= -1000000 ? currentScale : nextScaleRaw * 0.01
    this.applyPictureLayout(
      entry,
      pictureId,
      anchorX,
      anchorY,
      scale,
      this.readPicturePivot(entry),
    )
  }

  private readPictureProperty(command: ReadPicturePropertyCommand, context: CommandContext): void {
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    const entry = this.pictureEntries.get(pictureId)
    const value = entry === undefined ? 0 : this.readPictureMetric(entry, command.propertyId)
    this.assignNumberRef({ kind: 'raw', value: command.targetRaw }, value, context)
  }

  private applyPictureEffect(command: PictureEffectCommand, context: CommandContext): void {
    if (command.effectType !== 16 && command.effectType !== 32) {
      return
    }

    const pictureId = this.resolveNumberRef(command.pictureId, context)
    const entry = this.pictureEntries.get(pictureId)
    if (entry === undefined) {
      return
    }

    const currentLeft = this.readPictureMetric(entry, 'x')
    const currentTop = this.readPictureMetric(entry, 'y')
    this.applyPictureLayout(
      entry,
      pictureId,
      currentLeft + this.resolveNumberRef(command.x, context),
      currentTop + this.resolveNumberRef(command.y, context),
      this.readPictureMetric(entry, 'scale'),
      this.readPicturePivot(entry),
    )
  }

  private shouldSkipPictureRemoval(
    commands: WolfCommand[],
    index: number,
    command: RemovePictureCommand,
    context: CommandContext,
  ): boolean {
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    for (let cursor = index + 1; cursor < commands.length; cursor += 1) {
      const candidate = commands[cursor]
      switch (candidate.kind) {
        case 'blank':
        case 'debugComment':
        case 'checkpoint':
          continue
        case 'movePicture':
          return this.resolveNumberRef(candidate.pictureId, context) === pictureId
        default:
          return false
      }
    }

    return false
  }

  private placePictureEntry(
    entry: PictureEntry,
    pictureId: number,
    x: number,
    y: number,
    scale: number,
    pivot: PicturePivot,
  ): void {
    this.applyPictureLayout(entry, pictureId, x, y, scale, pivot)
    this.pictureEntries.get(pictureId)?.remove()
    this.pictureEntries.set(pictureId, entry)
    this.insertPictureEntry(entry, pictureId)
  }

  private insertPictureEntry(entry: PictureEntry, pictureId: number): void {
    const nextSibling = [...this.elements.pictureLayer.children].find((child) => {
      const childPictureId = Number.parseInt((child as HTMLElement).dataset.pictureId || '', 10)
      return Number.isFinite(childPictureId) && childPictureId > pictureId
    })
    if (nextSibling === undefined) {
      this.elements.pictureLayer.append(entry)
      return
    }
    this.elements.pictureLayer.insertBefore(entry, nextSibling)
  }

  private applyPictureLayout(
    element: HTMLElement,
    pictureId: number,
    x: number,
    y: number,
    scale: number,
    pivot: PicturePivot,
  ): void {
    const layout = this.calculatePictureLayout(element, x, y, scale, pivot)
    element.dataset.pictureId = String(pictureId)
    element.dataset.anchorX = String(x)
    element.dataset.anchorY = String(y)
    element.dataset.anchorScale = String(scale)
    element.dataset.pivot = pivot
    element.style.left = `${layout.left}px`
    element.style.top = `${layout.top}px`
    element.style.transform = `scale(${scale})`
  }

  private readPictureMetric(entry: PictureEntry, property: number | 'x' | 'y' | 'scale'): number {
    const element = entry
    switch (property) {
      case 'x':
      case 0:
        return Number.parseFloat(element.dataset.anchorX || '0') || 0
      case 'y':
      case 1:
        return Number.parseFloat(element.dataset.anchorY || '0') || 0
      case 2: {
        const baseWidth = Number.parseFloat(element.dataset.baseWidth || '0') || 0
        if (baseWidth > 0) {
          const ownScale = Number.parseFloat(element.dataset.anchorScale || '1') || 1
          return Math.round(baseWidth * ownScale)
        }
        return element instanceof HTMLImageElement ? element.naturalWidth : element.clientWidth
      }
      case 3: {
        const baseHeight = Number.parseFloat(element.dataset.baseHeight || '0') || 0
        if (baseHeight > 0) {
          const ownScale = Number.parseFloat(element.dataset.anchorScale || '1') || 1
          return Math.round(baseHeight * ownScale)
        }
        return element instanceof HTMLImageElement ? element.naturalHeight : element.clientHeight
      }
      case 5: {
        const opacity = Number.parseFloat(element.style.opacity || '1')
        return Number.isFinite(opacity) ? Math.round(opacity * 255) : 255
      }
      case 9:
        return this.pictureEntries.has(Number.parseInt(element.dataset.pictureId || '-1', 10)) ? 1 : 0
      case 'scale': {
        return Number.parseFloat(element.dataset.anchorScale || '1') || 1
      }
      default:
        return 0
    }
  }

  private getShowPictureStringOverride(
    commands: WolfCommand[],
    index: number,
    command: ShowPictureStringCommand,
    context: CommandContext,
  ): number | null {
    if (command.filePathRaw !== 1600008 || context.commonEventId === null) {
      return null
    }

    const nextCommand = commands[index + 1]
    if (nextCommand?.kind !== 'pictureEffect' || nextCommand.effectType !== 16) {
      return null
    }

    const pictureId = this.resolveNumberRef(command.pictureId, context)
    return this.resolveNumberRef(nextCommand.pictureId, context) === pictureId ? 1600009 : null
  }

  private sanitizePictureText(text: string): string {
    return text
      .replace(/<[^>]+>/g, '')
      .replace(/\\f\[[^\]]*\]/g, '')
      .replace(/\\ax\[[^\]]*\]/g, '')
      .replace(/\\ay\[[^\]]*\]/g, '')
      .replace(/\\space\[[^\]]*\]/g, ' ')
      .replace(/\\A/g, '')
      .replace(/\\E/g, '')
      .replace(/<R>/g, '')
      .trim()
  }

  private estimatePictureTextSize(text: string): { width: number; height: number } {
    if (text.length === 0) {
      return { width: 160, height: 24 }
    }

    const lines = text.split('\n')
    const maxLength = lines.reduce((longest, line) => Math.max(longest, line.length), 0)
    return {
      width: Math.max(48, maxLength * 8 + 8),
      height: Math.max(24, lines.length * 16 + 8),
    }
  }

  private shouldPreserveReplacementLayout(
    existingEntry: PictureEntry | undefined,
    resolvedX: number,
    resolvedY: number,
    resolvedScale: number,
  ): existingEntry is PictureEntry {
    if (existingEntry === undefined || resolvedX !== 0 || resolvedY !== 0) {
      return false
    }

    if (resolvedScale <= 0) {
      return true
    }

    if (resolvedScale !== 1) {
      return false
    }

    return this.readPictureMetric(existingEntry, 'x') !== 0
      || this.readPictureMetric(existingEntry, 'y') !== 0
      || this.readPictureMetric(existingEntry, 'scale') !== 1
      || this.readPicturePivot(existingEntry) !== 'leftTop'
  }

  private calculatePictureLayout(
    element: HTMLElement,
    x: number,
    y: number,
    scale: number,
    pivot: PicturePivot,
  ): { left: number; top: number } {
    const baseWidth = Number.parseFloat(element.dataset.baseWidth || '0') || 0
    const baseHeight = Number.parseFloat(element.dataset.baseHeight || '0') || 0
    const scaledWidth = baseWidth * scale
    const scaledHeight = baseHeight * scale
    const pivotOffset = this.getPivotOffset(pivot, scaledWidth, scaledHeight)
    const anchorX = x / WolfRuntime.PICTURE_COORDINATE_SCALE
    const anchorY = y / WolfRuntime.PICTURE_COORDINATE_SCALE
    return {
      left: Math.round(anchorX - pivotOffset.x),
      top: Math.round(anchorY - pivotOffset.y),
    }
  }

  private getPivotOffset(pivot: PicturePivot, width: number, height: number): { x: number; y: number } {
    switch (pivot) {
      case 'centerTop':
        return { x: width / 2, y: 0 }
      case 'rightTop':
        return { x: width, y: 0 }
      case 'leftMiddle':
        return { x: 0, y: height / 2 }
      case 'center':
        return { x: width / 2, y: height / 2 }
      case 'rightMiddle':
        return { x: width, y: height / 2 }
      case 'leftBottom':
        return { x: 0, y: height }
      case 'centerBottom':
        return { x: width / 2, y: height }
      case 'rightBottom':
        return { x: width, y: height }
      default:
        return { x: 0, y: 0 }
    }
  }

  private readPicturePivot(entry: PictureEntry): PicturePivot {
    const pivot = entry.dataset.pivot
    switch (pivot) {
      case 'centerTop':
      case 'rightTop':
      case 'leftMiddle':
      case 'center':
      case 'rightMiddle':
      case 'leftBottom':
      case 'centerBottom':
      case 'rightBottom':
        return pivot
      default:
        return 'leftTop'
    }
  }

  private initializeSystemUiDefaults(): void {
    if (this.repository === null) {
      return
    }

    const systemUiTable = 18
    const changeableDb = this.repository.changeableDb
    const defaults = [
      { record: 3, value: 10 },
      { record: 5, value: 10 },
      { record: 6, value: 8 },
      { record: 7, value: 6 },
      { record: 97, value: 10 },
      { record: 98, value: 10 },
    ] as const

    for (const entry of defaults) {
      if (changeableDb.getInt(systemUiTable, entry.record, 0) === 0) {
        changeableDb.setInt(systemUiTable, entry.record, 0, entry.value)
      }
    }
  }

  private removePicture(pictureId: number): void {
    this.pictureEntries.get(pictureId)?.remove()
    this.pictureEntries.delete(pictureId)
  }

  private restorePictureEntries(snapshot: Map<number, PictureEntry>): void {
    for (const [pictureId, entry] of this.pictureEntries) {
      if (!snapshot.has(pictureId)) {
        entry.remove()
        this.pictureEntries.delete(pictureId)
      }
    }

    for (const [pictureId, entry] of snapshot) {
      const currentEntry = this.pictureEntries.get(pictureId)
      if (currentEntry === entry) {
        continue
      }

      currentEntry?.remove()
      this.pictureEntries.set(pictureId, entry)
      if (!entry.isConnected) {
        this.elements.pictureLayer.append(entry)
      }
    }
  }

  private async showMessage(message: string): Promise<void> {
    this.elements.messageText.textContent = message
    this.elements.messageBox.classList.remove('hidden')
    await new Promise<void>((resolve) => {
      this.currentMessageResolver = resolve
    })
  }

  private hideMessage(): void {
    this.elements.messageBox.classList.add('hidden')
    this.elements.messageText.textContent = ''
  }

  private resolveCurrentMessage(): void {
    if (this.currentMessageResolver === null) {
      return
    }

    const resolve = this.currentMessageResolver
    this.currentMessageResolver = null
    this.hideMessage()
    resolve()
  }

  private async showChoices(command: ChoiceCommand): Promise<number> {
    this.elements.choiceList.innerHTML = ''
    this.elements.choiceBox.classList.remove('hidden')
    this.currentChoiceButtons = []
    this.currentChoiceIndex = 0
    for (let index = 0; index < command.options.length; index += 1) {
      const button = document.createElement('button')
      button.type = 'button'
      button.className = 'choice-button'
      button.dataset.choiceIndex = String(index)
      button.textContent = this.interpolateString(command.options[index], {
        mapId: this.currentMap?.id ?? 0,
        eventId: null,
        commonEventId: null,
      })
      button.addEventListener('click', () => {
        this.resolveCurrentChoice(index)
      })
      this.elements.choiceList.append(button)
      this.currentChoiceButtons.push(button)
    }
    this.updateChoiceSelection()

    return new Promise<number>((resolve) => {
      this.currentChoiceResolver = resolve
    })
  }

  private handleChoiceKey(key: string): boolean {
    if (this.currentChoiceResolver === null || this.currentChoiceButtons.length === 0) {
      return false
    }

    if (key === 'ArrowUp' || key === 'w' || key === 'W') {
      this.moveChoiceSelection(-1)
      return true
    }

    if (key === 'ArrowDown' || key === 's' || key === 'S') {
      this.moveChoiceSelection(1)
      return true
    }

    if (this.isConfirmKey(key)) {
      this.resolveCurrentChoice(this.currentChoiceIndex)
      return true
    }

    return false
  }

  private moveChoiceSelection(direction: number): void {
    if (this.currentChoiceButtons.length === 0) {
      return
    }

    const count = this.currentChoiceButtons.length
    this.currentChoiceIndex = (this.currentChoiceIndex + direction + count) % count
    this.updateChoiceSelection()
  }

  private updateChoiceSelection(): void {
    for (let index = 0; index < this.currentChoiceButtons.length; index += 1) {
      const button = this.currentChoiceButtons[index]
      const selected = index === this.currentChoiceIndex
      button.classList.toggle('selected', selected)
      button.setAttribute('aria-selected', selected ? 'true' : 'false')
      button.tabIndex = selected ? 0 : -1
    }
  }

  private resolveCurrentChoice(index: number): void {
    if (this.currentChoiceResolver === null) {
      return
    }

    const resolve = this.currentChoiceResolver
    this.currentChoiceResolver = null
    this.currentChoiceButtons = []
    this.currentChoiceIndex = 0
    this.elements.choiceBox.classList.add('hidden')
    resolve(index)
  }

  private async resolveKeyInput(command: KeyInputCommand): Promise<number> {
    if (command.device !== 'basic') {
      return 0
    }

    if (command.mode === 'wait') {
      while (true) {
        const value = this.readBasicKeyInput(command)
        if (value !== 0) {
          return value
        }
        await this.waitFrames(1)
      }
    }

    return this.readBasicKeyInput(command)
  }

  private readBasicKeyInput(command: KeyInputCommand): number {
    const directions = this.readBasicDirectionInput(command.acceptDirections)
    if (directions !== 0) {
      return directions
    }

    if (command.acceptConfirm) {
      const confirm = this.consumeFirstPressed(['Enter', ' ', 'z', 'Z'])
      if (confirm !== null) {
        return 10
      }
    }

    if (command.acceptCancel) {
      const cancel = this.consumeFirstPressed(['Escape', 'Backspace', 'x', 'X'])
      if (cancel !== null) {
        return 11
      }
    }

    if (command.acceptSub) {
      const sub = this.consumeFirstPressed(['c', 'C', 'Shift'])
      if (sub !== null) {
        return 12
      }
    }

    return 0
  }

  private readBasicDirectionInput(mode: KeyInputCommand['acceptDirections']): number {
    if (mode === 0) {
      return 0
    }

    if ((mode === 1 || mode === 2 || mode === 5) && this.consumeFirstPressed(['ArrowLeft', 'a', 'A']) !== null) {
      return 4
    }

    if ((mode === 1 || mode === 2 || mode === 6) && this.consumeFirstPressed(['ArrowRight', 'd', 'D']) !== null) {
      return 6
    }

    if ((mode === 1 || mode === 2 || mode === 3 || mode === 7) && this.consumeFirstPressed(['ArrowUp', 'w', 'W']) !== null) {
      return 8
    }

    if ((mode === 1 || mode === 2 || mode === 4 || mode === 7) && this.consumeFirstPressed(['ArrowDown', 's', 'S']) !== null) {
      return 2
    }

    return 0
  }

  private consumeFirstPressed(keys: readonly string[]): string | null {
    for (const key of keys) {
      if (this.bufferedPresses.has(key) || this.pressedKeys.has(key)) {
        this.consumeKey(key)
        return key
      }
    }
    return null
  }

  private async waitFrames(frames: number): Promise<void> {
    const frameCount = Math.max(0, Math.trunc(frames))
    for (let index = 0; index < frameCount; index += 1) {
      await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()))
    }
  }

  private findLabelIndex(commands: WolfCommand[], startIndex: number, label: string): number {
    for (let cursor = startIndex + 1; cursor < commands.length; cursor += 1) {
      const command = commands[cursor]
      if (
        (command.kind === 'forkBegin' && command.label === label)
        || (command.kind === 'forkEnd' && command.label === label)
      ) {
        return cursor
      }
    }
    return -1
  }

  private findForkEndIndex(commands: WolfCommand[], startIndex: number, indent: number): number {
    for (let cursor = startIndex + 1; cursor < commands.length; cursor += 1) {
      const command = commands[cursor]
      if (command.kind === 'forkEnd' && command.indent === indent) {
        return cursor
      }
    }
    return -1
  }

  private findBranchElseIndex(commands: WolfCommand[], startIndex: number, indent: number): number {
    for (let cursor = startIndex + 1; cursor < commands.length; cursor += 1) {
      const command = commands[cursor]
      if (command.kind === 'branchElse' && command.indent === indent) {
        return cursor
      }
      if (command.kind === 'forkEnd' && command.indent === indent) {
        break
      }
    }
    return -1
  }

  private findNamedLabelIndex(commands: WolfCommand[], command: LabelJumpCommand, context: CommandContext): number {
    const targetName = this.interpolateString(command.name, context)
    for (let cursor = 0; cursor < commands.length; cursor += 1) {
      const candidate = commands[cursor]
      if (candidate.kind === 'labelSet' && this.interpolateString(candidate.name, context) === targetName) {
        return cursor
      }
    }
    return -1
  }

  private async moveEventStep(event: WolfMapEvent, page: EventPage, state: EventRuntimeState): Promise<void> {
    switch (page.moveData.moveType) {
      case 1:
        await this.executeCustomMoveRoute(event, page, state)
        return
      case 2:
        await this.tryMoveEvent(event, page, state, this.getRandomDirectionDelta())
        return
      case 3:
        await this.tryMoveEvent(event, page, state, this.getApproachDirectionDelta(state))
        return
      default:
        return
    }
  }

  private async executeCustomMoveRoute(event: WolfMapEvent, page: EventPage, state: EventRuntimeState): Promise<void> {
    const moveCommands = page.moveData.moveCommands
    if (moveCommands.length === 0) {
      return
    }

    const routeIndex = Math.min(state.moveRouteIndex, moveCommands.length - 1)
    const command = moveCommands[routeIndex]
    let shouldAdvance = true

    switch (command.commandType) {
      case 0x00:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: 0, dy: 1, direction: 'down' })
        break
      case 0x01:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: -1, dy: 0, direction: 'left' })
        break
      case 0x02:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: 1, dy: 0, direction: 'right' })
        break
      case 0x03:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: 0, dy: -1, direction: 'up' })
        break
      case 0x04:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: -1, dy: 1, direction: 'left' })
        break
      case 0x05:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: 1, dy: 1, direction: 'right' })
        break
      case 0x06:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: -1, dy: -1, direction: 'left' })
        break
      case 0x07:
        shouldAdvance = await this.tryMoveEvent(event, page, state, { dx: 1, dy: -1, direction: 'right' })
        break
      case 0x08:
      case 0x09:
      case 0x0A:
      case 0x0B:
        state.direction = this.directionFromFacingCommand(command.commandType)
        break
      case 0x10:
        shouldAdvance = await this.tryMoveEvent(event, page, state, this.getRandomDirectionDelta())
        break
      case 0x11:
        shouldAdvance = await this.tryMoveEvent(event, page, state, this.getApproachDirectionDelta(state))
        break
      case 0x12:
        shouldAdvance = await this.tryMoveEvent(event, page, state, this.getRetreatDirectionDelta(state))
        break
      case 0x13:
        shouldAdvance = await this.tryMoveEvent(event, page, state, this.getDirectionDelta(state.direction))
        break
      case 0x14:
        shouldAdvance = await this.tryMoveEvent(event, page, state, this.getDirectionDelta(this.reverseDirection(state.direction)))
        break
      case 0x16:
        state.direction = this.rotateDirection(state.direction, 1)
        break
      case 0x17:
        state.direction = this.rotateDirection(state.direction, -1)
        break
      case 0x18:
        state.direction = this.rotateDirection(state.direction, Math.random() < 0.5 ? -1 : 1)
        break
      case 0x19:
        state.direction = this.getRandomDirectionDelta().direction
        break
      case 0x1A:
        state.direction = this.getApproachDirectionDelta(state).direction
        break
      case 0x1B:
        state.direction = this.getRetreatDirectionDelta(state).direction
        break
      case 0x1D:
        state.moveSpeed = Math.max(1, Math.trunc(command.args[0] ?? state.moveSpeed))
        break
      case 0x1E:
        state.moveFrequency = Math.max(1, Math.trunc(command.args[0] ?? state.moveFrequency))
        break
      case 0x26:
        state.canPass = true
        break
      case 0x27:
        state.canPass = false
        break
      case 0x2F:
        state.moveCooldownRemaining = Math.max(1, Math.trunc(command.args[0] ?? 1))
        break
      default:
        break
    }

    if (shouldAdvance) {
      const repeats = (page.moveData.moveFlags & 0x01) !== 0
      if (repeats) {
        state.moveRouteIndex = (routeIndex + 1) % moveCommands.length
      } else if (routeIndex < moveCommands.length - 1) {
        state.moveRouteIndex = routeIndex + 1
      }
    }
  }

  private async tryMoveEvent(
    event: WolfMapEvent,
    page: EventPage,
    state: EventRuntimeState,
    step: { dx: number; dy: number; direction: Direction },
  ): Promise<boolean> {
    if (step.dx === 0 && step.dy === 0) {
      return true
    }

    state.direction = step.direction
    if (this.currentMap === null) {
      return false
    }

    const nextX = state.x + step.dx
    const nextY = state.y + step.dy
    const skipBlockedMove = (page.moveData.moveFlags & 0x02) !== 0
    if (!this.isInsideMap(nextX, nextY)) {
      return skipBlockedMove
    }

    const tilePassable = this.currentMap.movableGrid[nextY][nextX]
    const targetIsPlayer = this.playerX === nextX && this.playerY === nextY
    const targetEvent = this.findEventAt(nextX, nextY, event.id)
    const targetPage = targetEvent === null ? null : this.getActivePage(targetEvent)
    const targetEventPassable = targetEvent === null || targetPage === null
      ? true
      : this.getEventState(targetEvent, targetPage).canPass

    if (!state.canPass && !tilePassable) {
      if (targetIsPlayer && page.triggerType === 'eventContact') {
        await this.runMapEvent(event)
        return true
      }
      return skipBlockedMove
    }

    if (targetEvent !== null && !state.canPass && !targetEventPassable) {
      return skipBlockedMove
    }

    if (targetIsPlayer && !state.canPass) {
      if (page.triggerType === 'eventContact') {
        await this.runMapEvent(event)
        return true
      }
      return skipBlockedMove
    }

    state.x = nextX
    state.y = nextY
    if (page.triggerType === 'eventContact' && this.isInsideTriggerRangeAt(page, nextX, nextY, this.playerX, this.playerY)) {
      await this.runMapEvent(event)
    }
    return true
  }

  private getMoveCooldownFrames(moveFrequency: number): number {
    switch (moveFrequency) {
      case 5:
        return 6
      case 4:
        return 12
      case 3:
        return 20
      case 2:
        return 32
      case 1:
        return 48
      default:
        return 24
    }
  }

  private getRandomDirectionDelta(): { dx: number; dy: number; direction: Direction } {
    const directions: Direction[] = ['down', 'left', 'right', 'up']
    return this.getDirectionDelta(directions[Math.floor(Math.random() * directions.length)] ?? 'down')
  }

  private getApproachDirectionDelta(state: EventRuntimeState): { dx: number; dy: number; direction: Direction } {
    const dx = this.playerX - state.x
    const dy = this.playerY - state.y
    if (Math.abs(dx) >= Math.abs(dy) && dx !== 0) {
      return this.getDirectionDelta(dx < 0 ? 'left' : 'right')
    }
    if (dy !== 0) {
      return this.getDirectionDelta(dy < 0 ? 'up' : 'down')
    }
    return this.getDirectionDelta(state.direction)
  }

  private getRetreatDirectionDelta(state: EventRuntimeState): { dx: number; dy: number; direction: Direction } {
    return this.getDirectionDelta(this.reverseDirection(this.getApproachDirectionDelta(state).direction))
  }

  private getDirectionDelta(direction: Direction): { dx: number; dy: number; direction: Direction } {
    switch (direction) {
      case 'left':
        return { dx: -1, dy: 0, direction }
      case 'right':
        return { dx: 1, dy: 0, direction }
      case 'up':
        return { dx: 0, dy: -1, direction }
      case 'down':
      default:
        return { dx: 0, dy: 1, direction: 'down' }
    }
  }

  private reverseDirection(direction: Direction): Direction {
    switch (direction) {
      case 'left':
        return 'right'
      case 'right':
        return 'left'
      case 'up':
        return 'down'
      case 'down':
      default:
        return 'up'
    }
  }

  private rotateDirection(direction: Direction, delta: -1 | 1): Direction {
    const order: Direction[] = ['up', 'right', 'down', 'left']
    const index = order.indexOf(direction)
    return order[(index + delta + order.length) % order.length] ?? direction
  }

  private directionFromFacingCommand(commandType: number): Direction {
    switch (commandType) {
      case 0x09:
        return 'left'
      case 0x0A:
        return 'right'
      case 0x0B:
        return 'up'
      case 0x08:
      default:
        return 'down'
    }
  }

  private getEventStateKey(event: WolfMapEvent): string {
    return `${this.currentMap?.id ?? 0}:${event.id}`
  }

  private getEventState(event: WolfMapEvent, page: EventPage): EventRuntimeState {
    const key = this.getEventStateKey(event)
    const existing = this.eventStates.get(key)
    if (existing !== undefined) {
      return existing
    }

    const created: EventRuntimeState = {
      x: event.x,
      y: event.y,
      direction: page.direction,
      canPass: page.moveData.canPass,
      moveSpeed: page.moveData.moveSpeed,
      moveFrequency: page.moveData.moveFrequency,
      moveRouteIndex: 0,
      moveCooldownRemaining: 0,
    }
    this.eventStates.set(key, created)
    return created
  }

  private render(): void {
    if (this.currentMap === null) {
      return
    }

    const viewportWidth = this.elements.canvas.width
    const viewportHeight = this.elements.canvas.height
    const worldWidth = this.currentMap.width * TILE_SIZE
    const worldHeight = this.currentMap.height * TILE_SIZE
    const cameraX = clamp(this.playerX * TILE_SIZE - viewportWidth / 6, 0, Math.max(0, worldWidth - viewportWidth / 3))
    const cameraY = clamp(this.playerY * TILE_SIZE - viewportHeight / 6, 0, Math.max(0, worldHeight - viewportHeight / 3))
    const viewWidth = viewportWidth / 3
    const viewHeight = viewportHeight / 3

    this.context.clearRect(0, 0, viewportWidth, viewportHeight)
    this.context.drawImage(this.currentMap.lowerCanvas, cameraX, cameraY, viewWidth, viewHeight, 0, 0, viewportWidth, viewportHeight)
    for (const event of this.currentMap.events) {
      const page = this.getActivePage(event)
      if (page !== null) {
        this.drawEvent(event, page, cameraX, cameraY)
      }
    }
    this.drawPlayer(cameraX, cameraY)
    this.context.drawImage(this.currentMap.upperCanvas, cameraX, cameraY, viewWidth, viewHeight, 0, 0, viewportWidth, viewportHeight)
  }

  private drawPlayer(cameraX: number, cameraY: number): void {
    const screenX = (this.playerX * TILE_SIZE - cameraX) * 3
    const screenY = (this.playerY * TILE_SIZE - cameraY) * 3
    if (this.playerSpriteSheet === null) {
      this.context.drawImage(this.playerSpriteFallback, screenX, screenY, TILE_SIZE * 3, TILE_SIZE * 3)
      return
    }

    const frame = this.getCharacterFrame(this.playerDirection, this.playerSpriteSheet.width, this.playerSpriteSheet.height, this.playerAnimationFrame)
    this.context.drawImage(
      this.playerSpriteSheet,
      frame.sx,
      frame.sy,
      frame.sw,
      frame.sh,
      screenX,
      screenY,
      frame.sw * 3,
      frame.sh * 3,
    )
  }

  private drawEvent(event: WolfMapEvent, page: EventPage, cameraX: number, cameraY: number): void {
    if (this.repository === null) {
      return
    }

    const state = this.getEventState(event, page)
    const screenX = (state.x * TILE_SIZE - cameraX) * 3
    const screenY = (state.y * TILE_SIZE - cameraY) * 3
    if (page.hasDirection && page.chipImgName.length > 0) {
      const image = this.repository.getLoadedImage(page.chipImgName)
      if (image === null) {
        void this.repository.loadImage(page.chipImgName)
        return
      }
      const frame = this.getCharacterFrame(state.direction, image.width, image.height)
      this.context.drawImage(
        image,
        frame.sx,
        frame.sy,
        frame.sw,
        frame.sh,
        screenX,
        screenY,
        frame.sw * 3,
        frame.sh * 3,
      )
      return
    }

    if (page.tileNo >= 0 && this.currentMap !== null) {
      const tileLayer = createCanvas(TILE_SIZE, TILE_SIZE)
      const tileContext = tileLayer.getContext('2d')
      if (tileContext !== null) {
        tileContext.drawImage(this.currentMap.upperCanvas, event.x * TILE_SIZE, event.y * TILE_SIZE, TILE_SIZE, TILE_SIZE, 0, 0, TILE_SIZE, TILE_SIZE)
        this.context.drawImage(tileLayer, screenX, screenY, TILE_SIZE * 3, TILE_SIZE * 3)
      }
    }
  }

  private getCharacterFrame(direction: Direction, width: number, height: number, column = 1): { sx: number; sy: number; sw: number; sh: number } {
    const frameWidth = Math.trunc(width / 6)
    const frameHeight = Math.trunc(height / 4)
    const row = direction === 'down' ? 0 : direction === 'left' ? 1 : direction === 'right' ? 2 : 3
    return {
      sx: column * frameWidth,
      sy: row * frameHeight,
      sw: frameWidth,
      sh: frameHeight,
    }
  }

  private advancePlayerAnimation(): void {
    this.playerAnimationFlip = !this.playerAnimationFlip
    this.playerAnimationFrame = this.playerAnimationFlip ? 0 : 2
    this.playerAnimationResetTick = this.tickCount + WolfRuntime.PLAYER_ANIMATION_SETTLE_FRAMES
  }

  private setStatus(text: string): void {
    this.elements.statusPanel.textContent = text
  }

  private setDebugInfo(text: string): void {
    if (this.elements.debugPanel !== undefined) {
      this.elements.debugPanel.textContent = text
    }
  }

  private setDebugIdle(): void {
    const lines = ['debug: idle']
    if (this.currentMap !== null) {
      lines.push(`map: ${this.currentMap.id}`)
      lines.push(`player: (${this.playerX}, ${this.playerY})`)
    }
    this.setDebugInfo(lines.join('\n'))
  }

  private setDebugCommand(context: CommandContext, command: WolfCommand, index: number, total: number): void {
    const lines = [
      'debug: executing',
      `map: ${context.mapId}`,
      `mapEvent: ${this.describeMapEvent(context)}`,
      `commonEvent: ${this.describeCommonEvent(context.commonEventId)}`,
      `command: ${index + 1}/${total}`,
      `kind: ${command.kind}`,
      '',
      this.formatCommandDebug(command),
    ]
    this.setDebugInfo(lines.join('\n'))
  }

  private describeMapEvent(context: CommandContext): string {
    if (context.eventId === null) {
      return '-'
    }
    const event = this.currentMap?.id === context.mapId
      ? this.currentMap.events.find((entry) => entry.id === context.eventId)
      : null
    return event === undefined || event === null ? String(context.eventId) : `${event.id} ${event.name}`
  }

  private describeCommonEvent(commonEventId: number | null): string {
    if (commonEventId === null) {
      return '-'
    }
    const commonEvent = this.getCommonEvent(commonEventId)
    return commonEvent === null ? String(commonEventId) : `${commonEvent.id} ${commonEvent.name}`
  }

  private formatCommandDebug(command: WolfCommand): string {
    return JSON.stringify(command, null, 2)
  }

  private showError(error: unknown): void {
    const message = error instanceof Error ? error.stack ?? error.message : String(error)
    this.elements.errorBox.innerHTML = `<div>${message.replace(/\n/g, '<br>')}</div>`
    this.elements.errorBox.classList.remove('hidden')
    this.setStatus(message)
  }

  private isConfirmKey(key: string): boolean {
    return key === 'Enter' || key === ' ' || key === 'z' || key === 'Z'
  }
}
