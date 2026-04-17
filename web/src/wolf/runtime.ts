import { clamp, createCanvas } from './binary'
import {
  buildPageConditionEntries,
  createDefaultContext,
  toCommonNumberVariableIndex,
  WolfDataRepository,
} from './data'
import type {
  CallEventCommand,
  ChangeVariableCommand,
  ChoiceCommand,
  CommandContext,
  CommonEventData,
  ConditionalForkCommand,
  Direction,
  EventPage,
  LoopStartCommand,
  NumberRef,
  ShowMessagePictureCommand,
  ShowPictureCommand,
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
  messageBox: HTMLDivElement
  messageText: HTMLDivElement
  choiceBox: HTMLDivElement
  choiceList: HTMLDivElement
  choiceTitle: HTMLDivElement
  pictureLayer: HTMLDivElement
  errorBox: HTMLDivElement
}

type PictureEntry = HTMLImageElement | HTMLDivElement

export class WolfRuntime {
  private readonly elements: RuntimeElements
  private readonly context: CanvasRenderingContext2D
  private readonly pictureEntries = new Map<number, PictureEntry>()
  private readonly pressedKeys = new Set<string>()
  private readonly virtualKeyTimers = new Map<string, number>()
  private readonly currentMapEventVariables = new Map<string, number[]>()
  private readonly triggeredAutoEvents = new Set<string>()

  private repository: WolfDataRepository | null = null
  private currentMap: WolfMapData | null = null
  private startLocation: StartLocation | null = null
  private playerX = 0
  private playerY = 0
  private playerDirection: Direction = 'down'
  private eventBusy = false
  private currentMessageResolver: (() => void) | null = null
  private currentChoiceResolver: ((value: number) => void) | null = null
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
  }

  async boot(): Promise<void> {
    try {
      this.setStatus('Loading databases and common events...')
      this.repository = await WolfDataRepository.create()
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
      if (this.currentMessageResolver !== null && this.isConfirmKey(event.key)) {
        event.preventDefault()
        this.resolveCurrentMessage()
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

    this.pressedKeys.add(key)
    const existingTimer = this.virtualKeyTimers.get(key)
    if (existingTimer !== undefined) {
      window.clearTimeout(existingTimer)
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

    if (!this.eventBusy) {
      await this.processAutoEvents()

      if (!this.eventBusy) {
        if (this.isMovementKeyDown()) {
          await this.stepPlayer()
        } else if (this.isAnyPressed(['z', 'Z', 'Enter', ' '])) {
          this.consumeKey('z')
          this.consumeKey('Z')
          this.consumeKey('Enter')
          this.consumeKey(' ')
          await this.tryInteract()
        }
      }
    }

    this.render()
  }

  private isMovementKeyDown(): boolean {
    return this.isAnyPressed(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'w', 'W', 'a', 'A', 's', 'S', 'd', 'D'])
  }

  private isAnyPressed(keys: string[]): boolean {
    return keys.some((key) => this.pressedKeys.has(key))
  }

  private consumeKey(key: string): void {
    this.pressedKeys.delete(key)
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
      this.consumeKey('ArrowLeft')
      this.consumeKey('a')
      this.consumeKey('A')
    } else if (this.isAnyPressed(['ArrowRight', 'd', 'D'])) {
      this.playerDirection = 'right'
      nextX += 1
      this.consumeKey('ArrowRight')
      this.consumeKey('d')
      this.consumeKey('D')
    } else if (this.isAnyPressed(['ArrowUp', 'w', 'W'])) {
      this.playerDirection = 'up'
      nextY -= 1
      this.consumeKey('ArrowUp')
      this.consumeKey('w')
      this.consumeKey('W')
    } else if (this.isAnyPressed(['ArrowDown', 's', 'S'])) {
      this.playerDirection = 'down'
      nextY += 1
      this.consumeKey('ArrowDown')
      this.consumeKey('s')
      this.consumeKey('S')
    }

    if (!this.canMoveTo(nextX, nextY)) {
      return
    }

    this.playerX = nextX
    this.playerY = nextY

    const contactEvent = this.findEventAt(nextX, nextY)
    if (contactEvent !== null) {
      const page = this.getActivePage(contactEvent)
      if (page?.triggerType === 'playerContact') {
        await this.runMapEvent(contactEvent)
      }
    }
  }

  private canMoveTo(x: number, y: number): boolean {
    if (this.currentMap === null) {
      return false
    }

    if (x < 0 || y < 0 || x >= this.currentMap.width || y >= this.currentMap.height) {
      return false
    }

    if (!this.currentMap.movableGrid[y][x]) {
      return false
    }

    const event = this.findEventAt(x, y)
    if (event === null) {
      return true
    }

    const page = this.getActivePage(event)
    return page?.moveData.canPass ?? true
  }

  private async tryInteract(): Promise<void> {
    const target = this.findFacingEvent()
    if (target === null) {
      return
    }

    const page = this.getActivePage(target)
    if (page?.triggerType === 'check') {
      await this.runMapEvent(target)
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

    while (index < commands.length) {
      const command = commands[index]
      switch (command.kind) {
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
        case 'movePosition':
          await this.changeMap(command.mapId, command.x, command.y)
          context.mapId = command.mapId
          index += 1
          break
        case 'callEvent':
          await this.executeCallEvent(command, context)
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
        case 'showPicture':
          await this.showPicture(command, context)
          index += 1
          break
        case 'showMessagePicture':
          this.showMessagePicture(command, context)
          index += 1
          break
        case 'removePicture':
          this.removePicture(command.pictureId)
          index += 1
          break
        case 'unknown':
          index += 1
          break
      }
    }
  }

  private jumpForConditionalFork(
    commands: WolfCommand[],
    index: number,
    command: ConditionalForkCommand,
    context: CommandContext,
  ): number {
    const matchIndex = command.conditions.findIndex((condition) => this.evaluateCondition(condition.operator, this.resolveNumberRef(condition.left, context), this.resolveNumberRef(condition.right, context)))
    const label = `${command.indent}.${matchIndex >= 0 ? matchIndex + 1 : 0}`
    const nextIndex = this.findLabelIndex(commands, index, label)
    return nextIndex >= 0 ? nextIndex + 1 : index + 1
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

    if (command.eventLookup.rawEventId >= 500000) {
      const commonEvent = this.repository.getCommonEventById(command.eventLookup.rawEventId - 500000)
      if (commonEvent !== null) {
        await this.runCommonEvent(commonEvent, command, context)
      }
      return
    }

    const targetEventId = command.eventLookup.rawEventId
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
    for (let index = 0; index < Math.min(4, args.length); index += 1) {
      commonEvent.numberVariables[index] = args[index]
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

  private applyUpdater(updater: Updater, context: CommandContext): void {
    const leftValue = this.resolveNumberRef(updater.left, context)
    const rightValue1 = this.resolveNumberRef(updater.right1, context)
    const rightValue2 = updater.right2 === null ? 0 : this.resolveNumberRef(updater.right2, context)
    const computedRightValue = this.applyRightOperator(updater.rightOperator, rightValue1, rightValue2)
    const nextValue = this.applyAssignOperator(updater.assignOperator, leftValue, computedRightValue)
    this.assignNumberRef(updater.left, nextValue, context)
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
    if (this.repository === null) {
      return 0
    }

    if (ref.kind === 'db') {
      const store = this.repository.getDatabase(ref.database)
      const table = this.resolveNumberRef(ref.table, context)
      const record = this.resolveNumberRef(ref.record, context)
      const field = this.resolveNumberRef(ref.field, context)
      return store.getInt(table, record, field)
    }

    const rawValue = ref.value
    if (rawValue >= 15000000) {
      const commonEventId = Math.trunc((rawValue - 15000000) / 100)
      const variableIndex = rawValue % 100
      const commonEvent = this.repository.getCommonEventById(commonEventId)
      if (commonEvent === null) {
        return 0
      }
      const resolvedIndex = toCommonNumberVariableIndex(variableIndex)
      return resolvedIndex >= 0 ? commonEvent.numberVariables[resolvedIndex] ?? 0 : 0
    }

    if (rawValue >= 1600000 && context.commonEventId !== null) {
      const commonEvent = this.repository.getCommonEventById(context.commonEventId)
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
      const commonEventId = Math.trunc((rawValue - 15000000) / 100)
      const variableIndex = rawValue % 100
      const commonEvent = this.repository.getCommonEventById(commonEventId)
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
      const commonEvent = this.repository.getCommonEventById(context.commonEventId)
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

    return text.replace(/\\(self\[(\d+)\]|cself\[(\d+)\]|([ucs])db\[(\d+):(\d+):(\d+)])/g, (_, _whole, selfIndex, cselfIndex, dbKind, table, record, field) => {
      if (selfIndex !== undefined) {
        if (context.eventId === null) {
          return '0'
        }
        return String(this.getMapEventVariable(context.mapId, context.eventId, Number(selfIndex)))
      }

      if (cselfIndex !== undefined) {
        if (context.commonEventId === null) {
          return '0'
        }
        const commonEvent = this.repository?.getCommonEventById(context.commonEventId) ?? null
        if (commonEvent === null) {
          return '0'
        }
        const index = toCommonNumberVariableIndex(Number(cselfIndex))
        return index >= 0 ? String(commonEvent.numberVariables[index] ?? 0) : ''
      }

      const dbType = dbKind === 'u' ? 'user' : dbKind === 'c' ? 'changeable' : 'system'
      return this.repository!.getDatabase(dbType).getString(Number(table), Number(record), Number(field))
    })
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

  private findEventAt(x: number, y: number): WolfMapEvent | null {
    if (this.currentMap === null) {
      return null
    }
    return this.currentMap.events.find((event) => {
      const page = this.getActivePage(event)
      return page !== null && event.x === x && event.y === y
    }) ?? null
  }

  private findFacingEvent(): WolfMapEvent | null {
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
    return this.findEventAt(x, y)
  }

  private async changeMap(mapId: number, x: number, y: number): Promise<void> {
    if (this.repository === null) {
      return
    }

    this.currentMap = await this.repository.loadMap(mapId)
    this.playerX = clamp(x, 0, this.currentMap.width - 1)
    this.playerY = clamp(y, 0, this.currentMap.height - 1)
    this.triggeredAutoEvents.clear()
    this.pictureEntries.forEach((entry) => entry.remove())
    this.pictureEntries.clear()
    for (const event of this.currentMap.events) {
      const key = `${this.currentMap.id}:${event.id}`
      if (!this.currentMapEventVariables.has(key)) {
        this.currentMapEventVariables.set(key, Array.from({ length: 10 }, () => 0))
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

  private async showPicture(command: ShowPictureCommand, context: CommandContext): Promise<void> {
    if (this.repository === null) {
      return
    }

    const image = await this.repository.loadImage(command.filePath)
    const entry = document.createElement('img')
    entry.src = image.src
    entry.className = 'picture-entry'
    this.applyPictureLayout(entry, command.pictureId, command.x, command.y, command.scale, command.pivot, context)
    this.pictureEntries.get(command.pictureId)?.remove()
    this.pictureEntries.set(command.pictureId, entry)
    this.elements.pictureLayer.append(entry)
  }

  private showMessagePicture(command: ShowMessagePictureCommand, context: CommandContext): void {
    const entry = document.createElement('div')
    entry.className = 'picture-entry'
    entry.textContent = this.interpolateString(command.message, context)
    this.applyPictureLayout(
      entry,
      this.resolveNumberRef(command.pictureId, context),
      this.resolveNumberRef(command.x, context),
      this.resolveNumberRef(command.y, context),
      command.scale,
      command.pivot,
      context,
    )
    const pictureId = this.resolveNumberRef(command.pictureId, context)
    this.pictureEntries.get(pictureId)?.remove()
    this.pictureEntries.set(pictureId, entry)
    this.elements.pictureLayer.append(entry)
  }

  private applyPictureLayout(
    element: HTMLElement,
    pictureId: number,
    x: number,
    y: number,
    scale: number,
    pivot: string,
    context: CommandContext,
  ): void {
    void pictureId
    void pivot
    void context
    element.style.left = `${x}px`
    element.style.top = `${y}px`
    element.style.transform = `scale(${scale})`
  }

  private removePicture(pictureId: number): void {
    this.pictureEntries.get(pictureId)?.remove()
    this.pictureEntries.delete(pictureId)
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
    for (let index = 0; index < command.options.length; index += 1) {
      const button = document.createElement('button')
      button.type = 'button'
      button.className = 'choice-button'
      button.textContent = this.interpolateString(command.options[index], {
        mapId: this.currentMap?.id ?? 0,
        eventId: null,
        commonEventId: null,
      })
      button.addEventListener('click', () => {
        if (this.currentChoiceResolver !== null) {
          const resolve = this.currentChoiceResolver
          this.currentChoiceResolver = null
          this.elements.choiceBox.classList.add('hidden')
          resolve(index)
        }
      })
      this.elements.choiceList.append(button)
    }

    return new Promise<number>((resolve) => {
      this.currentChoiceResolver = resolve
    })
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

    const frame = this.getCharacterFrame(this.playerDirection, this.playerSpriteSheet.width, this.playerSpriteSheet.height)
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

    const screenX = (event.x * TILE_SIZE - cameraX) * 3
    const screenY = (event.y * TILE_SIZE - cameraY) * 3
    if (page.hasDirection && page.chipImgName.length > 0) {
      const image = this.repository.getLoadedImage(page.chipImgName)
      if (image === null) {
        void this.repository.loadImage(page.chipImgName)
        return
      }
      const frame = this.getCharacterFrame(page.direction, image.width, image.height)
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

  private getCharacterFrame(direction: Direction, width: number, height: number): { sx: number; sy: number; sw: number; sh: number } {
    const frameWidth = Math.trunc(width / 6)
    const frameHeight = Math.trunc(height / 4)
    const column = 1
    const row = direction === 'down' ? 0 : direction === 'left' ? 1 : direction === 'right' ? 2 : 3
    return {
      sx: column * frameWidth,
      sy: row * frameHeight,
      sw: frameWidth,
      sh: frameHeight,
    }
  }

  private setStatus(text: string): void {
    this.elements.statusPanel.textContent = text
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
