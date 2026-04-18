import { WolfBinaryReader, createCanvas, loadBinary, loadImage } from './binary'
import type {
  AbortEventCommand,
  BlankCommand,
  BranchElseCommand,
  CallEventCommand,
  ChangeStringDatabaseCommand,
  ChangeStringCommand,
  ChangeVariableCommand,
  CommonEventData,
  CommandContext,
  ConditionEntry,
  DatabaseRecord,
  DatabaseSchema,
  DatabaseType,
  CheckpointCommand,
  DebugCommentCommand,
  Direction,
  EventMoveData,
  EventPage,
  EventTriggerType,
  ForkBeginCommand,
  ForkEndCommand,
  KeyInputCommand,
  LabelJumpCommand,
  LabelSetCommand,
  LoopBreakCommand,
  LoopContinueCommand,
  LoopEndCommand,
  LoopStartCommand,
  MessageCommand,
  MovePositionCommand,
  MovePictureCommand,
  NumberRef,
  PageCondition,
  PictureEffectCommand,
  PicturePivot,
  ReadPicturePropertyCommand,
  RemovePictureCommand,
  ShowMessagePictureCommand,
  ShowPictureCommand,
  ShowPictureStringCommand,
  ShowWindowPictureCommand,
  StartLocation,
  WaitCommand,
  TileSetData,
  UnitTile,
  UnknownCommand,
  Updater,
  WolfCommand,
  WolfMapData,
  WolfMapEvent,
  PlaySystemSeCommand,
  PlaySeFileCommand,
  SaveSlotCommand,
} from './types'
import { TILE_SIZE } from './types'

function makeDbKey(table: number, record: number, field: number): string {
  return `${table}:${record}:${field}`
}

class WolfDatabaseStore {
  readonly intMap = new Map<string, number>()
  readonly stringMap = new Map<string, string>()
  readonly schemas: DatabaseSchema[]
  readonly records: DatabaseRecord[][]

  constructor(schemas: DatabaseSchema[], records: DatabaseRecord[][]) {
    this.schemas = schemas
    this.records = records
    for (let tableIndex = 0; tableIndex < schemas.length; tableIndex += 1) {
      const schema = schemas[tableIndex]
      const tableRecords = records[tableIndex] ?? []
      for (let recordIndex = 0; recordIndex < tableRecords.length; recordIndex += 1) {
        const record = tableRecords[recordIndex]
        let intIndex = 0
        let stringIndex = 0
        for (let fieldIndex = 0; fieldIndex < schema.columns.length; fieldIndex += 1) {
          const field = schema.columns[fieldIndex]
          const key = makeDbKey(tableIndex, recordIndex, fieldIndex)
          if (field.type === 'int') {
            this.intMap.set(key, record.intData[intIndex] ?? 0)
            intIndex += 1
          } else {
            this.stringMap.set(key, record.stringData[stringIndex] ?? '')
            stringIndex += 1
          }
        }
      }
    }
  }

  getInt(table: number, record: number, field: number): number {
    return this.intMap.get(makeDbKey(table, record, field)) ?? 0
  }

  setInt(table: number, record: number, field: number, value: number): void {
    const key = makeDbKey(table, record, field)
    if (this.intMap.has(key)) {
      this.intMap.set(key, value)
      return
    }

    if (this.stringMap.has(key)) {
      this.stringMap.set(key, String(value))
    }
  }

  setString(table: number, record: number, field: number, value: string): void {
    const key = makeDbKey(table, record, field)
    if (this.stringMap.has(key)) {
      this.stringMap.set(key, value)
      return
    }

    if (this.intMap.has(key)) {
      const numericValue = Number.parseInt(value, 10)
      this.intMap.set(key, Number.isFinite(numericValue) ? numericValue : 0)
    }
  }

  getString(table: number, record: number, field: number): string {
    const key = makeDbKey(table, record, field)
    if (this.stringMap.has(key)) {
      return this.stringMap.get(key) ?? ''
    }

    if (this.intMap.has(key)) {
      return String(this.intMap.get(key) ?? 0)
    }

    return ''
  }

  findTableIndexByName(name: string): number {
    return this.schemas.findIndex((schema) => schema.name === name)
  }

  findRecordIndexByName(table: number, name: string): number {
    return (this.records[table] ?? []).findIndex((record) => record.name === name)
  }

  findFieldIndexByName(table: number, name: string): number {
    return (this.schemas[table]?.columns ?? []).findIndex((column) => column.name === name)
  }
}

interface MetaCommand {
  numberArgs: number[]
  stringArgs: string[]
  indentDepth: number
  footerValue: number
}

function rawRef(value: number): NumberRef {
  return { kind: 'raw', value }
}

function convertDirection(directionValue: number): Direction {
  switch (directionValue) {
    case 4:
      return 'left'
    case 6:
      return 'right'
    case 8:
      return 'up'
    default:
      return 'down'
  }
}

function convertTrigger(triggerValue: number): EventTriggerType {
  switch (triggerValue) {
    case 1:
      return 'auto'
    case 2:
      return 'parallel'
    case 3:
      return 'playerContact'
    case 4:
      return 'eventContact'
    default:
      return 'check'
  }
}

function convertPivot(value: number): PicturePivot {
  switch (value) {
    case 0x10:
      return 'centerTop'
    case 0x20:
      return 'rightTop'
    case 0x01:
      return 'leftMiddle'
    case 0x11:
      return 'center'
    case 0x21:
      return 'rightMiddle'
    case 0x02:
      return 'leftBottom'
    case 0x12:
      return 'centerBottom'
    case 0x22:
      return 'rightBottom'
    default:
      return 'leftTop'
  }
}

export class WolfDataRepository {
  readonly systemDb: WolfDatabaseStore
  readonly changeableDb: WolfDatabaseStore
  readonly userDb: WolfDatabaseStore
  readonly tileSets: TileSetData[]
  readonly commonEvents: CommonEventData[]

  private readonly imageCache = new Map<string, Promise<HTMLImageElement>>()
  private readonly loadedImages = new Map<string, HTMLImageElement>()
  private readonly mapCache = new Map<number, Promise<WolfMapData>>()
  private readonly commonEventByName = new Map<string, CommonEventData>()

  private constructor(
    systemDb: WolfDatabaseStore,
    changeableDb: WolfDatabaseStore,
    userDb: WolfDatabaseStore,
    tileSets: TileSetData[],
    commonEvents: CommonEventData[],
  ) {
    this.systemDb = systemDb
    this.changeableDb = changeableDb
    this.userDb = userDb
    this.tileSets = tileSets
    this.commonEvents = commonEvents

    for (const commonEvent of commonEvents) {
      this.commonEventByName.set(commonEvent.name, commonEvent)
    }
  }

  static async create(): Promise<WolfDataRepository> {
    const [systemDb, changeableDb, userDb] = await Promise.all([
      this.loadDatabase('SysDatabase'),
      this.loadDatabase('CDataBase'),
      this.loadDatabase('DataBase'),
    ])

    const tileSetsBytes = await loadBinary('/Data/BasicData/TileSetData.dat')
    const tileSets = this.readTileSets(tileSetsBytes)
    const commonEvents = this.readCommonEvents(
      await loadBinary('/Data/BasicData/CommonEvent.dat'),
      { systemDb, changeableDb, userDb },
    )

    return new WolfDataRepository(systemDb, changeableDb, userDb, tileSets, commonEvents)
  }

  getStartLocation(): StartLocation {
    return {
      mapId: this.systemDb.getInt(7, 0, 0),
      x: this.systemDb.getInt(7, 0, 1),
      y: this.systemDb.getInt(7, 0, 2),
    }
  }

  getMapFilePath(mapId: number): string {
    const filePath = this.systemDb.getString(0, mapId, 0)
    if (filePath.length === 0) {
      throw new Error(`Map file path was not found for mapId=${mapId}.`)
    }
    return `/Data/${filePath}`
  }

  getCommonEventById(id: number): CommonEventData | null {
    return this.commonEvents[id] ?? null
  }

  getCommonEventByName(name: string): CommonEventData | null {
    return this.commonEventByName.get(name) ?? null
  }

  getDatabase(database: DatabaseType): WolfDatabaseStore {
    switch (database) {
      case 'changeable':
        return this.changeableDb
      case 'user':
        return this.userDb
      default:
        return this.systemDb
    }
  }

  loadImage(path: string): Promise<HTMLImageElement> {
    const normalized = path.startsWith('/Data/') ? path : `/Data/${path}`
    const cached = this.imageCache.get(normalized)
    if (cached !== undefined) {
      return cached
    }

    const promise = loadImage(normalized).then((image) => {
      this.loadedImages.set(normalized, image)
      return image
    })
    this.imageCache.set(normalized, promise)
    return promise
  }

  getLoadedImage(path: string): HTMLImageElement | null {
    const normalized = path.startsWith('/Data/') ? path : `/Data/${path}`
    return this.loadedImages.get(normalized) ?? null
  }

  async loadMap(mapId: number): Promise<WolfMapData> {
    const cached = this.mapCache.get(mapId)
    if (cached !== undefined) {
      return cached
    }

    const promise = this.loadMapInternal(mapId)
    this.mapCache.set(mapId, promise)
    return promise
  }

  private async loadMapInternal(mapId: number): Promise<WolfMapData> {
    const reader = new WolfBinaryReader(await loadBinary(this.getMapFilePath(mapId)))
    const tileSetId = reader.readInt(0x22).value
    const width = reader.readInt(0x26).value
    const height = reader.readInt(0x2a).value
    const layers = [
      this.readLayer(reader, width, height, 0x32 + width * height * 4 * 0),
      this.readLayer(reader, width, height, 0x32 + width * height * 4 * 1),
      this.readLayer(reader, width, height, 0x32 + width * height * 4 * 2),
    ]

    const tileSet = this.tileSets[tileSetId]
    if (tileSet === undefined) {
      throw new Error(`TileSet ${tileSetId} was not found.`)
    }

    const [baseImage, ...autoImages] = await Promise.all([
      this.loadImage(tileSet.baseTileFilePath),
      ...tileSet.autoTileFilePaths.map((filePath) => this.loadImage(filePath)),
    ])

    const lowerCanvas = createCanvas(width * TILE_SIZE, height * TILE_SIZE)
    const upperCanvas = createCanvas(width * TILE_SIZE, height * TILE_SIZE)
    const lowerContext = lowerCanvas.getContext('2d')
    const upperContext = upperCanvas.getContext('2d')
    if (lowerContext === null || upperContext === null) {
      throw new Error('Failed to acquire map rendering context.')
    }

    const movableGrid = Array.from({ length: height }, () => Array.from({ length: width }, () => true))
    for (let layerIndex = 0; layerIndex < layers.length; layerIndex += 1) {
      const context = layerIndex < 2 ? lowerContext : upperContext
      for (let y = 0; y < height; y += 1) {
        for (let x = 0; x < width; x += 1) {
          const tileValue = layers[layerIndex][y][x]
          const tileInfo = this.resolveTileInfo(tileSet, tileValue)
          movableGrid[y][x] = movableGrid[y][x] && tileInfo.passable
          this.drawTile(context, baseImage, autoImages, tileValue, x, y)
        }
      }
    }

    const events = await this.readMapEvents(
      reader,
      mapId,
      0x32 + width * height * 4 * 3,
    )

    return {
      id: mapId,
      width,
      height,
      tileSetId,
      layers,
      events,
      movableGrid,
      lowerCanvas,
      upperCanvas,
    }
  }

  private readLayer(reader: WolfBinaryReader, width: number, height: number, offset: number): number[][] {
    const layer = Array.from({ length: height }, () => Array.from({ length: width }, () => 0))
    let currentOffset = offset
    for (let x = 0; x < width; x += 1) {
      for (let y = 0; y < height; y += 1) {
        const value = reader.readInt(currentOffset)
        layer[y][x] = value.value
        currentOffset = value.nextOffset
      }
    }
    return layer
  }

  private resolveTileInfo(tileSet: TileSetData, tileValue: number): { passable: boolean } {
    if (tileValue >= 100000) {
      const autoTileId = Math.trunc(tileValue / 100000) - 1
      if (autoTileId <= 0) {
        return { passable: true }
      }
      const tile = tileSet.unitTiles[autoTileId]
      return { passable: tile?.movableType !== 'immovable' }
    }

    const tile = tileSet.unitTiles[tileValue + 16]
    return { passable: tile?.movableType !== 'immovable' }
  }

  private drawTile(
    context: CanvasRenderingContext2D,
    baseImage: HTMLImageElement,
    autoImages: HTMLImageElement[],
    tileValue: number,
    x: number,
    y: number,
  ): void {
    const dx = x * TILE_SIZE
    const dy = y * TILE_SIZE

    if (tileValue >= 100000) {
      const autoTileId = Math.trunc(tileValue / 100000) - 1
      if (autoTileId <= 0) {
        return
      }

      const image = autoImages[autoTileId - 1]
      if (image === undefined) {
        return
      }

      const leftUp = Math.trunc(tileValue / 1000) % 10
      const rightUp = Math.trunc(tileValue / 100) % 10
      const leftDown = Math.trunc(tileValue / 10) % 10
      const rightDown = tileValue % 10

      context.drawImage(image, 0, leftUp * TILE_SIZE, 8, 8, dx, dy, 8, 8)
      context.drawImage(image, 8, rightUp * TILE_SIZE, 8, 8, dx + 8, dy, 8, 8)
      context.drawImage(image, 0, leftDown * TILE_SIZE + 8, 8, 8, dx, dy + 8, 8, 8)
      context.drawImage(image, 8, rightDown * TILE_SIZE + 8, 8, 8, dx + 8, dy + 8, 8, 8)
      return
    }

    const sx = (tileValue % 8) * TILE_SIZE
    const sy = Math.trunc(tileValue / 8) * TILE_SIZE
    context.drawImage(baseImage, sx, sy, TILE_SIZE, TILE_SIZE, dx, dy, TILE_SIZE, TILE_SIZE)
  }

  private async readMapEvents(
    reader: WolfBinaryReader,
    mapId: number,
    offset: number,
  ): Promise<WolfMapEvent[]> {
    const events: WolfMapEvent[] = []
    let currentOffset = offset
    let header = reader.readByte(currentOffset)
    currentOffset = header.nextOffset

    while (header.value !== 0x66) {
      currentOffset += 4

      const eventId = reader.readInt(currentOffset)
      currentOffset = eventId.nextOffset

      const eventName = reader.readString(currentOffset)
      currentOffset = eventName.nextOffset

      const posX = reader.readInt(currentOffset)
      currentOffset = posX.nextOffset

      const posY = reader.readInt(currentOffset)
      currentOffset = posY.nextOffset

      const pageCount = reader.readInt(currentOffset)
      currentOffset = pageCount.nextOffset

      currentOffset = reader.readInt(currentOffset).nextOffset

      const pages: EventPage[] = []
      for (let pageIndex = 0; pageIndex < pageCount.value; pageIndex += 1) {
        const page = this.readEventPage(reader, currentOffset, pageIndex)
        pages.push(page.page)
        currentOffset = page.nextOffset
      }

      currentOffset = reader.readByte(currentOffset).nextOffset
      events.push({
        id: eventId.value,
        name: eventName.value,
        x: posX.value,
        y: posY.value,
        pages,
      })

      header = reader.readByte(currentOffset)
      currentOffset = header.nextOffset
    }

    void mapId
    return events
  }

  private readEventPage(
    reader: WolfBinaryReader,
    offset: number,
    pageIndex: number,
  ): { page: EventPage; nextOffset: number } {
    let currentOffset = reader.readByte(offset).nextOffset

    const tileNo = reader.readInt(currentOffset)
    currentOffset = tileNo.nextOffset

    const chipImgName = reader.readString(currentOffset)
    currentOffset = chipImgName.nextOffset

    const direction = reader.readByte(currentOffset)
    currentOffset = direction.nextOffset

    currentOffset = reader.readByte(currentOffset).nextOffset
    currentOffset = reader.readByte(currentOffset).nextOffset
    currentOffset = reader.readByte(currentOffset).nextOffset

    const triggerType = reader.readByte(currentOffset)
    currentOffset = triggerType.nextOffset

    const conditionOps: number[] = []
    for (let index = 0; index < 4; index += 1) {
      const conditionOp = reader.readByte(currentOffset)
      conditionOps.push(conditionOp.value)
      currentOffset = conditionOp.nextOffset
    }

    const leftValues: number[] = []
    for (let index = 0; index < 4; index += 1) {
      const left = reader.readInt(currentOffset)
      leftValues.push(left.value)
      currentOffset = left.nextOffset
    }

    const rightValues: number[] = []
    for (let index = 0; index < 4; index += 1) {
      const right = reader.readInt(currentOffset)
      rightValues.push(right.value)
      currentOffset = right.nextOffset
    }

    const moveData = this.readEventMoveData(reader, currentOffset)
    currentOffset = moveData.nextOffset

    const eventCommandCount = reader.readInt(currentOffset)
    currentOffset = eventCommandCount.nextOffset

    const commands: WolfCommand[] = []
    for (let index = 0; index < eventCommandCount.value; index += 1) {
      const command = this.readCommand(reader, currentOffset)
      commands.push(command.command)
      currentOffset = command.nextOffset
    }

    currentOffset = reader.readInt(currentOffset).nextOffset
    currentOffset = reader.readByte(currentOffset).nextOffset
    const rangeExtendX = reader.readByte(currentOffset)
    currentOffset = rangeExtendX.nextOffset
    const rangeExtendY = reader.readByte(currentOffset)
    currentOffset = rangeExtendY.nextOffset
    currentOffset = reader.readByte(currentOffset).nextOffset

    const conditions = conditionOps.map<PageCondition>((operatorRaw, index) => ({
      operatorRaw,
      leftRaw: leftValues[index] ?? 0,
      rightRaw: rightValues[index] ?? 0,
    }))

    return {
      page: {
        pageIndex,
        tileNo: tileNo.value,
        chipImgName: chipImgName.value,
        direction: convertDirection(direction.value),
        hasDirection: tileNo.value === -1 && chipImgName.value.length > 0,
        triggerType: convertTrigger(triggerType.value),
        conditions,
        rangeExtendX: rangeExtendX.value,
        rangeExtendY: rangeExtendY.value,
        moveData: moveData.moveData,
        commands,
      },
      nextOffset: currentOffset,
    }
  }

  private readEventMoveData(
    reader: WolfBinaryReader,
    offset: number,
  ): { moveData: EventMoveData; nextOffset: number } {
    let currentOffset = offset
    const animationSpeed = reader.readByte(currentOffset)
    currentOffset = animationSpeed.nextOffset
    const moveSpeed = reader.readByte(currentOffset)
    currentOffset = moveSpeed.nextOffset
    const moveFrequency = reader.readByte(currentOffset)
    currentOffset = moveFrequency.nextOffset
    const moveType = reader.readByte(currentOffset)
    currentOffset = moveType.nextOffset
    const optionType = reader.readByte(currentOffset)
    currentOffset = optionType.nextOffset
    const moveFlags = reader.readByte(currentOffset)
    currentOffset = moveFlags.nextOffset
    const commandCount = reader.readInt(currentOffset)
    currentOffset = commandCount.nextOffset
    const moveCommands = []

    for (let index = 0; index < commandCount.value; index += 1) {
      const moveCommand = this.readMoveCommand(reader, currentOffset)
      moveCommands.push(moveCommand.command)
      currentOffset = moveCommand.nextOffset
    }

    return {
      moveData: {
        animationSpeed: animationSpeed.value,
        moveSpeed: moveSpeed.value,
        moveFrequency: moveFrequency.value,
        moveType: moveType.value,
        optionFlags: optionType.value,
        moveFlags: moveFlags.value,
        canPass: (optionType.value & 8) > 0,
        moveCommands,
      },
      nextOffset: currentOffset,
    }
  }

  private readMoveCommand(
    reader: WolfBinaryReader,
    offset: number,
  ): { command: { commandType: number; args: number[] }; nextOffset: number } {
    let currentOffset = offset
    const commandType = reader.readByte(currentOffset)
    currentOffset = commandType.nextOffset
    const variableCount = reader.readByte(currentOffset)
    currentOffset = variableCount.nextOffset
    const args: number[] = []
    for (let index = 0; index < variableCount.value; index += 1) {
      const value = reader.readInt(currentOffset)
      args.push(value.value)
      currentOffset = value.nextOffset
    }
    currentOffset = reader.readByte(currentOffset).nextOffset
    currentOffset = reader.readByte(currentOffset).nextOffset
    return {
      command: {
        commandType: commandType.value,
        args,
      },
      nextOffset: currentOffset,
    }
  }

  private readCommand(
    reader: WolfBinaryReader,
    offset: number,
  ): { command: WolfCommand; nextOffset: number } {
    let currentOffset = offset
    const numberArgCount = reader.readByte(currentOffset)
    currentOffset = numberArgCount.nextOffset

    const numberArgs: number[] = []
    for (let index = 0; index < numberArgCount.value; index += 1) {
      const value = reader.readInt(currentOffset)
      numberArgs.push(value.value)
      currentOffset = value.nextOffset
    }

    const indentDepth = reader.readByte(currentOffset)
    currentOffset = indentDepth.nextOffset

    const stringArgCount = reader.readByte(currentOffset)
    currentOffset = stringArgCount.nextOffset

    const stringArgs: string[] = []
    for (let index = 0; index < stringArgCount.value; index += 1) {
      const value = reader.readString(currentOffset)
      stringArgs.push(value.value)
      currentOffset = value.nextOffset
    }

    const footerValue = reader.readByte(currentOffset)
    currentOffset = footerValue.nextOffset

    const meta: MetaCommand = {
      numberArgs,
      stringArgs,
      indentDepth: indentDepth.value,
      footerValue: footerValue.value,
    }

    if (meta.footerValue === 1) {
      currentOffset = this.readEventMoveData(reader, currentOffset).nextOffset
    }

    return {
      command: this.createCommand(meta),
      nextOffset: currentOffset,
    }
  }

  private createCommand(meta: MetaCommand): WolfCommand {
    const key = meta.numberArgs[0] ?? -1
    switch (key) {
      case 0x00:
        return { kind: 'blank', indent: meta.indentDepth } satisfies BlankCommand
      case 0x65:
        return { kind: 'message', indent: meta.indentDepth, text: meta.stringArgs[0] ?? '' } satisfies MessageCommand
      case 0x67:
      case 0x6a:
        return { kind: 'debugComment', indent: meta.indentDepth, text: meta.stringArgs[0] ?? '' } satisfies DebugCommentCommand
      case 0x63:
        return { kind: 'checkpoint', indent: meta.indentDepth } satisfies CheckpointCommand
      case 0x66:
        return { kind: 'choice', indent: meta.indentDepth, options: meta.stringArgs } satisfies WolfCommand
      case 0x6f:
        return {
          kind: 'conditionalFork',
          indent: meta.indentDepth,
          conditions: this.createConditions(meta),
        }
      case 0x79:
        return {
          kind: 'changeVariable',
          indent: meta.indentDepth,
          updaters: [this.createStandardUpdater(meta)],
        } satisfies ChangeVariableCommand
      case 0x7a:
        return this.createChangeString(meta)
      case 0x82:
        return {
          kind: 'movePosition',
          indent: meta.indentDepth,
          targetEventId: meta.numberArgs[1] ?? 0,
          x: meta.numberArgs[2] ?? 0,
          y: meta.numberArgs[3] ?? 0,
          mapId: meta.numberArgs[4] ?? 0,
        } satisfies MovePositionCommand
      case 0x7b:
        return this.createKeyInputCommand(meta)
      case 0x7c:
        return this.createVariablePlusCommand(meta)
      case 0x96:
        return this.createPictureCommand(meta)
      case 0x122:
        return this.createPictureEffectCommand(meta)
      case 0xaa:
        return { kind: 'loopStart', indent: meta.indentDepth, isInfinite: true, loopCount: null } satisfies LoopStartCommand
      case 0xab:
        return { kind: 'loopBreak', indent: meta.indentDepth } satisfies LoopBreakCommand
      case 0xac:
        return { kind: 'abortEvent', indent: meta.indentDepth } satisfies AbortEventCommand
      case 0xb0:
        return { kind: 'loopContinue', indent: meta.indentDepth } satisfies LoopContinueCommand
      case 0xb3:
        return { kind: 'loopStart', indent: meta.indentDepth, isInfinite: false, loopCount: rawRef(meta.numberArgs[1] ?? 0) } satisfies LoopStartCommand
      case 0xb4:
        return { kind: 'wait', indent: meta.indentDepth, frames: meta.numberArgs[1] ?? 0 } satisfies WaitCommand
      case 0xd2:
        return this.createCallEventById(meta)
      case 0xd4:
        return { kind: 'labelSet', indent: meta.indentDepth, name: meta.stringArgs[0] ?? '' } satisfies LabelSetCommand
      case 0xd5:
        return { kind: 'labelJump', indent: meta.indentDepth, name: meta.stringArgs[0] ?? '' } satisfies LabelJumpCommand
      case 0xfa:
        return this.createDatabaseCommand(meta)
      case 0x12c:
        return this.createCallEventByName(meta)
      case 0x1a4:
        return { kind: 'branchElse', indent: meta.indentDepth } satisfies BranchElseCommand
      case 0x191:
        return { kind: 'forkBegin', indent: meta.indentDepth, label: `${meta.indentDepth}.${meta.numberArgs[1] ?? 0}` } satisfies ForkBeginCommand
      case 0x1f2:
        return { kind: 'loopEnd', indent: meta.indentDepth } satisfies LoopEndCommand
      case 0x1f3:
        return { kind: 'forkEnd', indent: meta.indentDepth, label: `${meta.indentDepth}.0` } satisfies ForkEndCommand
      case 0x70:
        return { kind: 'playSystemSe', indent: meta.indentDepth } satisfies PlaySystemSeCommand
      case 0x8c:
        return { kind: 'playSeFile', indent: meta.indentDepth } satisfies PlaySeFileCommand
      case 0xdc:
      case 0xdd:
      case 0xde:
        return { kind: 'saveSlot', indent: meta.indentDepth } satisfies SaveSlotCommand
      default:
        return { kind: 'unknown', indent: meta.indentDepth, key } satisfies UnknownCommand
    }
  }

  private createConditions(meta: MetaCommand): ConditionEntry[] {
    const conditions: ConditionEntry[] = []
    const conditionCount = Math.max(0, Math.trunc((meta.numberArgs.length - 2) / 3))
    for (let index = 0; index < conditionCount; index += 1) {
      const left = meta.numberArgs[2 + 3 * index] ?? 0
      const right = meta.numberArgs[3 + 3 * index] ?? 0
      const params = meta.numberArgs[4 + 3 * index] ?? 0
      conditions.push({
        left: rawRef(left),
        right: rawRef((params >> 4) > 0 ? right : right),
        operator: params & 0x0f,
      })
    }
    return conditions
  }

  private createKeyInputCommand(meta: MetaCommand): KeyInputCommand {
    const flagsRaw = meta.numberArgs[2] ?? 0
    const deviceRaw = (flagsRaw >> 8) & 0xff
    const deviceCode = deviceRaw & 0x0f
    const device =
      deviceCode === 0x01
        ? 'keyboard'
        : deviceCode === 0x02
          ? 'pad'
          : deviceCode === 0x03
            ? 'mouse'
            : deviceCode === 0x04
              ? 'padStick'
              : deviceCode === 0x05
                ? 'padPov'
                : deviceCode === 0x06
                  ? 'multiTouch'
                  : 'basic'

    return {
      kind: 'keyInput',
      indent: meta.indentDepth,
      targetRaw: meta.numberArgs[1] ?? 0,
      device,
      mode: (flagsRaw & 0x80) !== 0 ? 'wait' : 'pressed',
      flagsRaw,
      specificKeyCode: meta.numberArgs[3] ?? null,
      acceptDirections: (flagsRaw & 0x0f) as 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7,
      acceptConfirm: (flagsRaw & 0x10) !== 0,
      acceptCancel: (flagsRaw & 0x20) !== 0,
      acceptSub: (flagsRaw & 0x40) !== 0,
    } satisfies KeyInputCommand
  }

  private createStandardUpdater(meta: MetaCommand): Updater {
    const operatorType = Math.trunc((meta.numberArgs[4] ?? 0) / 0x100) % 0x100
    return {
      left: rawRef(meta.numberArgs[1] ?? 0),
      right1: rawRef(meta.numberArgs[2] ?? 0),
      right2: rawRef(meta.numberArgs[3] ?? 0),
      assignOperator: operatorType % 0x10,
      rightOperator: Math.trunc(operatorType / 0x10),
    }
  }

  private createChangeString(meta: MetaCommand): ChangeStringCommand {
    return {
      kind: 'changeString',
      indent: meta.indentDepth,
      targetRaw: meta.numberArgs[1] ?? 0,
      opRaw: meta.numberArgs[2] ?? 0,
      sourceRaw: meta.numberArgs.length > 3 ? (meta.numberArgs[3] ?? 0) : null,
      texts: meta.stringArgs,
    }
  }

  private createDatabaseUpdater(meta: MetaCommand): Updater {
    const typeNoRaw = meta.numberArgs[1] ?? 0
    const dataNoRaw = meta.numberArgs[2] ?? 0
    const fieldNoRaw = meta.numberArgs[3] ?? 0
    const configRaw = meta.numberArgs[4] ?? 0
    const targetValueRaw = meta.numberArgs[5] ?? 0

    const operatorRaw = configRaw & 0xf0
    const targetDatabase = (configRaw >> 8) & 0x0f
    const modeType = (configRaw >> 12) & 0x0f
    const nameFlags = (configRaw >> 16) & 0x0f

    const database = this.toDatabaseType(targetDatabase)
    const store = this.getDatabase(database)

    let typeNo = typeNoRaw
    let dataNo = dataNoRaw
    let fieldNo = fieldNoRaw

    if ((nameFlags & 0x01) > 0) {
      typeNo = store.findTableIndexByName(meta.stringArgs[1] ?? '')
    }
    if ((nameFlags & 0x02) > 0) {
      dataNo = store.findRecordIndexByName(typeNo, meta.stringArgs[2] ?? '')
    }
    if ((nameFlags & 0x04) > 0) {
      fieldNo = store.findFieldIndexByName(typeNo, meta.stringArgs[3] ?? '')
    }

    const databaseRef: NumberRef = {
      kind: 'db',
      database,
      table: rawRef(typeNo),
      record: rawRef(dataNo),
      field: rawRef(fieldNo),
    }
    const valueRef = rawRef(targetValueRaw)

    return {
      left: modeType === 0 ? databaseRef : valueRef,
      right1: modeType === 0 ? valueRef : databaseRef,
      right2: rawRef(0),
      assignOperator: this.toDbAssignOperator(operatorRaw),
      rightOperator: 0,
    }
  }

  private createDatabaseCommand(meta: MetaCommand): WolfCommand {
    const typeNoRaw = meta.numberArgs[1] ?? 0
    const dataNoRaw = meta.numberArgs[2] ?? 0
    const fieldNoRaw = meta.numberArgs[3] ?? 0
    const configRaw = meta.numberArgs[4] ?? 0
    const targetValueRaw = meta.numberArgs[5] ?? 0

    const targetDatabase = (configRaw >> 8) & 0x0f
    const modeType = (configRaw >> 12) & 0x0f
    const nameFlags = (configRaw >> 16) & 0x0f

    const database = this.toDatabaseType(targetDatabase)
    const store = this.getDatabase(database)

    let typeNo = typeNoRaw
    let dataNo = dataNoRaw
    let fieldNo = fieldNoRaw

    if ((nameFlags & 0x01) > 0) {
      typeNo = store.findTableIndexByName(meta.stringArgs[1] ?? '')
    }
    if ((nameFlags & 0x02) > 0) {
      dataNo = store.findRecordIndexByName(typeNo, meta.stringArgs[2] ?? '')
    }
    if ((nameFlags & 0x04) > 0) {
      fieldNo = store.findFieldIndexByName(typeNo, meta.stringArgs[3] ?? '')
    }

    const fieldType = store.schemas[typeNo]?.columns[fieldNo]?.type ?? 'int'
    if (modeType !== 0 && fieldType === 'string' && this.isStringVariableRaw(targetValueRaw)) {
      return {
        kind: 'changeStringDatabase',
        indent: meta.indentDepth,
        targetRaw: targetValueRaw,
        database,
        table: rawRef(typeNo),
        record: rawRef(dataNo),
        field: rawRef(fieldNo),
      } satisfies ChangeStringDatabaseCommand
    }

    return {
      kind: 'changeVariable',
      indent: meta.indentDepth,
      updaters: [this.createDatabaseUpdater(meta)],
    } satisfies ChangeVariableCommand
  }

  private isStringVariableRaw(rawValue: number): boolean {
    if (rawValue < 1600000) {
      return false
    }

    const variableId = rawValue % 100
    return variableId >= 5 && variableId <= 9
  }

  private createPictureCommand(meta: MetaCommand): WolfCommand {
    const operationType = (meta.numberArgs[1] ?? 0) & 0x0f
    if (operationType === 0x00) {
      const sourceType = ((meta.numberArgs[1] ?? 0) >> 4) & 0x0f
      const pivot = ((meta.numberArgs[1] ?? 0) >> 8) & 0xff
      if (sourceType === 0x00) {
        return {
          kind: 'showPicture',
          indent: meta.indentDepth,
          pictureId: meta.numberArgs[2] ?? 0,
          filePath: meta.stringArgs[0] ?? '',
          pivot: convertPivot(pivot),
          x: meta.numberArgs[8] ?? 0,
          y: meta.numberArgs[9] ?? 0,
          scale: (meta.numberArgs[10] ?? 100) * 0.01,
        } satisfies ShowPictureCommand
      }

      if (sourceType === 0x02) {
        return {
          kind: 'showMessagePicture',
          indent: meta.indentDepth,
          pictureId: rawRef(meta.numberArgs[2] ?? 0),
          message: meta.stringArgs[0] ?? '',
          pivot: convertPivot(pivot),
          x: rawRef(meta.numberArgs[8] ?? 0),
          y: rawRef(meta.numberArgs[9] ?? 0),
          scale: (meta.numberArgs[10] ?? 100) * 0.01,
        } satisfies ShowMessagePictureCommand
      }

      if (sourceType === 0x01 || sourceType === 0x04) {
        return {
          kind: 'showPictureString',
          indent: meta.indentDepth,
          pictureId: rawRef(meta.numberArgs[2] ?? 0),
          filePathRaw: meta.numberArgs.at(-1) ?? 0,
          pivot: convertPivot(pivot),
          x: rawRef(meta.numberArgs[8] ?? 0),
          y: rawRef(meta.numberArgs[9] ?? 0),
          scale: rawRef(meta.numberArgs[10] ?? 100),
        } satisfies ShowPictureStringCommand
      }

      if (sourceType === 0x03) {
        return {
          kind: 'showWindowPicture',
          indent: meta.indentDepth,
          pictureId: rawRef(meta.numberArgs[2] ?? 0),
          message: meta.stringArgs[0] ?? '',
          pivot: convertPivot(((meta.numberArgs[1] ?? 0) >> 8) & 0xff),
          width: rawRef(meta.numberArgs[4] ?? 0),
          height: rawRef(meta.numberArgs[5] ?? 0),
          x: rawRef(meta.numberArgs[8] ?? 0),
          y: rawRef(meta.numberArgs[9] ?? 0),
          scale: rawRef(meta.numberArgs[10] ?? 100),
          opacity: rawRef(meta.numberArgs[7] ?? 255),
        } satisfies ShowWindowPictureCommand
      }
    }

    if (operationType === 0x01) {
      return {
        kind: 'movePicture',
        indent: meta.indentDepth,
        pictureId: rawRef(meta.numberArgs[2] ?? 0),
        x: rawRef(meta.numberArgs[8] ?? -1000000),
        y: rawRef(meta.numberArgs[9] ?? -1000000),
        scale: rawRef(meta.numberArgs[10] ?? -1000000),
      } satisfies MovePictureCommand
    }

    if (operationType === 0x02) {
      return { kind: 'removePicture', indent: meta.indentDepth, pictureId: rawRef(meta.numberArgs[2] ?? 0) } satisfies RemovePictureCommand
    }

    if (operationType === 0x03 && meta.numberArgs.length <= 4) {
      return { kind: 'removePicture', indent: meta.indentDepth, pictureId: rawRef(meta.numberArgs[2] ?? 0) } satisfies RemovePictureCommand
    }

    return { kind: 'unknown', indent: meta.indentDepth, key: meta.numberArgs[0] ?? -1 } satisfies UnknownCommand
  }

  private createVariablePlusCommand(meta: MetaCommand): WolfCommand {
    const mode = meta.numberArgs[2] ?? 0
    if (mode === 0x4000 || mode === 0x4400) {
      return {
        kind: 'readPictureProperty',
        indent: meta.indentDepth,
        targetRaw: meta.numberArgs[1] ?? 0,
        pictureId: rawRef(meta.numberArgs[3] ?? 0),
        propertyId: meta.numberArgs[4] ?? 0,
      } satisfies ReadPicturePropertyCommand
    }

    return { kind: 'unknown', indent: meta.indentDepth, key: meta.numberArgs[0] ?? -1 } satisfies UnknownCommand
  }

  private createPictureEffectCommand(meta: MetaCommand): WolfCommand {
    return {
      kind: 'pictureEffect',
      indent: meta.indentDepth,
      effectType: meta.numberArgs[1] ?? 0,
      pictureId: rawRef(meta.numberArgs[3] ?? meta.numberArgs[2] ?? 0),
      x: rawRef(meta.numberArgs[5] ?? 0),
      y: rawRef(meta.numberArgs[6] ?? 0),
    } satisfies PictureEffectCommand
  }

  private createCallEventById(meta: MetaCommand): CallEventCommand {
    const numberArgInfo = meta.numberArgs[2] ?? 0
    const numberArgCount = numberArgInfo & 0x0f
    const stringArgCount = (numberArgInfo >> 4) & 0x0f
    const hasReturnValue = (numberArgInfo >> 24) > 0
    const numberArgs: NumberRef[] = []
    for (let index = 0; index < numberArgCount; index += 1) {
      numberArgs.push(rawRef(meta.numberArgs[3 + index] ?? 0))
    }
    const returnDestinationIndex = 3 + numberArgCount + stringArgCount
    return {
      kind: 'callEvent',
      indent: meta.indentDepth,
      eventLookup: { type: 'id', rawEventId: rawRef(meta.numberArgs[1] ?? 0) },
      numberArgs,
      hasReturnValue,
      returnDestination: hasReturnValue ? rawRef(meta.numberArgs[returnDestinationIndex] ?? 0) : null,
    }
  }

  private createCallEventByName(meta: MetaCommand): CallEventCommand {
    const numberArgInfo = meta.numberArgs[2] ?? 0
    const numberArgCount = numberArgInfo & 0x0f
    const stringArgCount = (numberArgInfo >> 4) & 0x0f
    const hasReturnValue = (numberArgInfo >> 24) > 0
    const numberArgs: NumberRef[] = []
    for (let index = 0; index < numberArgCount; index += 1) {
      numberArgs.push(rawRef(meta.numberArgs[3 + index] ?? 0))
    }
    const returnDestinationIndex = 3 + numberArgCount + stringArgCount
    return {
      kind: 'callEvent',
      indent: meta.indentDepth,
      eventLookup: { type: 'name', name: meta.stringArgs[0] ?? '' },
      numberArgs,
      hasReturnValue,
      returnDestination: hasReturnValue ? rawRef(meta.numberArgs[returnDestinationIndex] ?? 0) : null,
    }
  }

  private toDatabaseType(value: number): DatabaseType {
    switch (value) {
      case 0:
        return 'changeable'
      case 2:
        return 'user'
      default:
        return 'system'
    }
  }

  private toDbAssignOperator(value: number): number {
    switch (value) {
      case 0x10:
        return 1
      case 0x20:
        return 2
      case 0x30:
        return 3
      case 0x40:
        return 4
      case 0x50:
        return 5
      case 0x60:
        return 6
      case 0x70:
        return 7
      default:
        return 0
    }
  }

  private static async loadDatabase(baseName: string): Promise<WolfDatabaseStore> {
    const [projectBytes, dataBytes] = await Promise.all([
      loadBinary(`/Data/BasicData/${baseName}.project`),
      loadBinary(`/Data/BasicData/${baseName}.dat`),
    ])

    const projectReader = new WolfBinaryReader(projectBytes)
    const dataReader = new WolfBinaryReader(dataBytes)

    let projectOffset = 0
    const schemaCount = projectReader.readInt(projectOffset)
    projectOffset = schemaCount.nextOffset

    const schemas: DatabaseSchema[] = []
    const records: DatabaseRecord[][] = []
    for (let index = 0; index < schemaCount.value; index += 1) {
      const schemaResult = this.readProjectSchema(projectReader, projectOffset)
      schemas.push(schemaResult.schema)
      records.push(schemaResult.records)
      projectOffset = schemaResult.nextOffset
    }

    let dataOffset = 11
    const typeCount = dataReader.readInt(dataOffset)
    dataOffset = typeCount.nextOffset
    let previousRecords: DatabaseRecord[] | null = null

    for (let tableIndex = 0; tableIndex < typeCount.value; tableIndex += 1) {
      const loadResult = this.readDataTable(
        dataReader,
        dataOffset,
        schemas[tableIndex],
        records[tableIndex],
        previousRecords,
      )
      dataOffset = loadResult.nextOffset
      if (!loadResult.usedPreviousNames) {
        previousRecords = records[tableIndex]
      }
    }

    return new WolfDatabaseStore(schemas, records)
  }

  private static readProjectSchema(
    reader: WolfBinaryReader,
    offset: number,
  ): { schema: DatabaseSchema; records: DatabaseRecord[]; nextOffset: number } {
    let currentOffset = offset
    const name = reader.readString(currentOffset)
    currentOffset = name.nextOffset

    const columnCount = reader.readInt(currentOffset)
    currentOffset = columnCount.nextOffset
    const columnNames: string[] = []
    for (let index = 0; index < columnCount.value; index += 1) {
      const columnName = reader.readString(currentOffset)
      columnNames.push(columnName.value)
      currentOffset = columnName.nextOffset
    }

    const recordCount = reader.readInt(currentOffset)
    currentOffset = recordCount.nextOffset
    const records: DatabaseRecord[] = []
    for (let index = 0; index < recordCount.value; index += 1) {
      const recordName = reader.readString(currentOffset)
      records.push({ name: recordName.value, intData: [], stringData: [] })
      currentOffset = recordName.nextOffset
    }

    currentOffset = reader.readString(currentOffset).nextOffset

    const customSelectCount = reader.readInt(currentOffset)
    currentOffset = customSelectCount.nextOffset + customSelectCount.value

    const memoCount = reader.readInt(currentOffset)
    currentOffset = memoCount.nextOffset
    for (let index = 0; index < memoCount.value; index += 1) {
      currentOffset = reader.readString(currentOffset).nextOffset
    }

    const stringParamCount = reader.readInt(currentOffset)
    currentOffset = stringParamCount.nextOffset
    for (let index = 0; index < stringParamCount.value; index += 1) {
      const selectCount = reader.readInt(currentOffset)
      currentOffset = selectCount.nextOffset
      for (let nested = 0; nested < selectCount.value; nested += 1) {
        currentOffset = reader.readString(currentOffset).nextOffset
      }
    }

    const numberParamCount = reader.readInt(currentOffset)
    currentOffset = numberParamCount.nextOffset
    for (let index = 0; index < numberParamCount.value; index += 1) {
      const selectCount = reader.readInt(currentOffset)
      currentOffset = selectCount.nextOffset
      currentOffset += selectCount.value * 4
    }

    const initValueCount = reader.readInt(currentOffset)
    currentOffset = initValueCount.nextOffset
    const initialValues: number[] = []
    for (let index = 0; index < initValueCount.value; index += 1) {
      const initialValue = reader.readInt(currentOffset)
      initialValues.push(initialValue.value)
      currentOffset = initialValue.nextOffset
    }

    return {
      schema: {
        name: name.value,
        columns: columnNames.map((columnName, index) => ({
          name: columnName,
          type: 'int',
          initialValue: initialValues[index] ?? 0,
        })),
      },
      records,
      nextOffset: currentOffset,
    }
  }

  private static readDataTable(
    reader: WolfBinaryReader,
    offset: number,
    schema: DatabaseSchema,
    records: DatabaseRecord[],
    previousRecords: DatabaseRecord[] | null,
  ): { nextOffset: number; usedPreviousNames: boolean } {
    let currentOffset = offset
    currentOffset = reader.readInt(currentOffset).nextOffset
    const idSelectType = reader.readInt(currentOffset)
    currentOffset = idSelectType.nextOffset

    const columnCount = reader.readInt(currentOffset)
    currentOffset = columnCount.nextOffset

    const columnTypes: number[] = []
    let intCount = 0
    let stringCount = 0
    for (let index = 0; index < columnCount.value; index += 1) {
      const columnType = reader.readInt(currentOffset)
      columnTypes.push(columnType.value)
      currentOffset = columnType.nextOffset
      if (columnType.value < 2000) {
        intCount += 1
        schema.columns[index].type = 'int'
      } else {
        stringCount += 1
        schema.columns[index].type = 'string'
      }
    }

    const recordCount = reader.readInt(currentOffset)
    currentOffset = recordCount.nextOffset
    for (let recordIndex = 0; recordIndex < recordCount.value; recordIndex += 1) {
      records[recordIndex].intData = []
      for (let intIndex = 0; intIndex < intCount; intIndex += 1) {
        const value = reader.readInt(currentOffset)
        records[recordIndex].intData.push(value.value)
        currentOffset = value.nextOffset
      }

      records[recordIndex].stringData = []
      for (let stringIndex = 0; stringIndex < stringCount; stringIndex += 1) {
        const value = reader.readString(currentOffset)
        records[recordIndex].stringData.push(value.value)
        currentOffset = value.nextOffset
      }
    }

    let usedPreviousNames = false
    if (idSelectType.value === 1 && stringCount > 0) {
      for (const record of records) {
        record.name = record.stringData[0] ?? record.name
      }
    } else if (idSelectType.value === 2 && previousRecords !== null) {
      usedPreviousNames = true
      const count = Math.min(records.length, previousRecords.length)
      for (let index = 0; index < count; index += 1) {
        records[index].name = previousRecords[index].name
      }
    }

    return { nextOffset: currentOffset, usedPreviousNames }
  }

  private static readTileSets(bytes: Uint8Array): TileSetData[] {
    const reader = new WolfBinaryReader(bytes)
    const settingCount = reader.readInt(0x0b)
    let offset = 0x0f
    const tileSets: TileSetData[] = []
    for (let settingIndex = 0; settingIndex < settingCount.value; settingIndex += 1) {
      const settingName = reader.readString(offset)
      offset = settingName.nextOffset

      const baseTileFilePath = reader.readString(offset)
      offset = baseTileFilePath.nextOffset

      const autoTileFilePaths: string[] = []
      for (let autoIndex = 0; autoIndex < 15; autoIndex += 1) {
        const autoTilePath = reader.readString(offset)
        autoTileFilePaths.push(autoTilePath.value)
        offset = autoTilePath.nextOffset
      }

      offset += 1
      const unitTagLength = reader.readInt(offset)
      offset = unitTagLength.nextOffset
      const unitTagNumbers: number[] = []
      for (let index = 0; index < unitTagLength.value; index += 1) {
        const tag = reader.readByte(offset)
        unitTagNumbers.push(tag.value)
        offset = tag.nextOffset
      }

      offset += 1
      const unitConfigLength = reader.readInt(offset)
      offset = unitConfigLength.nextOffset
      const unitTiles: UnitTile[] = []
      for (let index = 0; index < unitConfigLength.value; index += 1) {
        const raw = reader.readInt(offset)
        offset = raw.nextOffset
        const value = raw.value
        unitTiles.push({
          movableType: (value & 0x0f) === 0 ? 'movable' : 'immovable',
          passableByDirection: {
            down: (value & 1) === 0,
            left: (value & 2) === 0,
            right: (value & 4) === 0,
            up: (value & 8) === 0,
          },
          isCounter: (value & 0x80) > 0,
          tagNumber: unitTagNumbers[index] ?? 0,
        })
      }

      tileSets.push({
        settingName: settingName.value,
        baseTileFilePath: baseTileFilePath.value,
        autoTileFilePaths,
        unitTiles,
      })
    }

    return tileSets
  }

  private static readCommonEvents(
    bytes: Uint8Array,
    repositories: {
      systemDb: WolfDatabaseStore
      changeableDb: WolfDatabaseStore
      userDb: WolfDatabaseStore
    },
  ): CommonEventData[] {
    const reader = new WolfBinaryReader(bytes)
    let offset = 11
    const eventCount = reader.readInt(offset)
    offset = eventCount.nextOffset

    const repository = new WolfDataRepository(
      repositories.systemDb,
      repositories.changeableDb,
      repositories.userDb,
      [],
      [],
    )

    const commonEvents: CommonEventData[] = []
    for (let index = 0; index < eventCount.value; index += 1) {
      offset = reader.readByte(offset).nextOffset
      const eventId = reader.readInt(offset)
      offset = eventId.nextOffset

      offset = reader.readByte(offset).nextOffset
      offset = reader.readInt(offset).nextOffset
      offset = reader.readInt(offset).nextOffset

      offset = reader.readByte(offset).nextOffset
      offset = reader.readByte(offset).nextOffset

      const eventName = reader.readString(offset)
      offset = eventName.nextOffset

      const commandLength = reader.readInt(offset)
      offset = commandLength.nextOffset

      const commands: WolfCommand[] = []
      for (let commandIndex = 0; commandIndex < commandLength.value; commandIndex += 1) {
        const command = repository.readCommand(reader, offset)
        commands.push(command.command)
        offset = command.nextOffset
      }

      offset = reader.readBytes(offset, 5).nextOffset
      offset = reader.readString(offset).nextOffset
      offset = reader.readByte(offset).nextOffset

      const argNameCountRaw = reader.readInt(offset)
      let argNameCount = argNameCountRaw.value
      offset = argNameCountRaw.nextOffset
      argNameCount = 10
      for (let argIndex = 0; argIndex < argNameCount; argIndex += 1) {
        offset = reader.readString(offset).nextOffset
      }

      const argSpecifyTypeCount = reader.readInt(offset)
      offset = argSpecifyTypeCount.nextOffset + argSpecifyTypeCount.value

      const stringArgSpecifyParamCount = reader.readInt(offset)
      offset = stringArgSpecifyParamCount.nextOffset
      for (let stringIndex = 0; stringIndex < stringArgSpecifyParamCount.value; stringIndex += 1) {
        const paramCount = reader.readInt(offset)
        offset = paramCount.nextOffset
        for (let paramIndex = 0; paramIndex < paramCount.value; paramIndex += 1) {
          offset = reader.readString(offset).nextOffset
        }
      }

      const numberArgSpecifyParamCount = reader.readInt(offset)
      offset = numberArgSpecifyParamCount.nextOffset
      for (let numberIndex = 0; numberIndex < numberArgSpecifyParamCount.value; numberIndex += 1) {
        const paramCount = reader.readInt(offset)
        offset = paramCount.nextOffset + paramCount.value * 4
      }

      offset = reader.readBytes(offset, 20).nextOffset

      const argInitValueCount = reader.readInt(offset)
      offset = argInitValueCount.nextOffset + argInitValueCount.value * 4

      offset = reader.readByte(offset).nextOffset
      offset = reader.readInt(offset).nextOffset
      for (let variableIndex = 0; variableIndex < 100; variableIndex += 1) {
        offset = reader.readString(offset).nextOffset
      }

      offset = reader.readByte(offset).nextOffset
      offset = reader.readBytes(offset, 5).nextOffset
      offset = reader.readByte(offset).nextOffset
      offset = reader.readString(offset).nextOffset

      const returnValueField = reader.readInt(offset)
      offset = returnValueField.nextOffset
      const returnValueRaw = 15000000 + eventId.value * 100 + returnValueField.value
      offset = reader.readByte(offset).nextOffset

      commonEvents.push({
        id: eventId.value,
        name: eventName.value,
        commands,
        returnValueRaw,
        numberVariables: Array.from({ length: 95 }, () => 0),
        stringVariables: Array.from({ length: 5 }, () => ''),
      })
    }

    return commonEvents
  }
}

export function toCommonNumberVariableIndex(variableId: number): number {
  if (variableId >= 5 && variableId <= 9) {
    return -1
  }

  if (variableId <= 4) {
    return variableId
  }

  return variableId - 5
}

export function isPageConditionEnabled(condition: PageCondition): boolean {
  return !(condition.operatorRaw === 0 && condition.leftRaw === 0 && condition.rightRaw === 0)
}

export function buildPageConditionEntries(page: EventPage): ConditionEntry[] {
  return page.conditions
    .filter((condition) => isPageConditionEnabled(condition))
    .map((condition) => ({
      left: rawRef(condition.leftRaw),
      right: rawRef(condition.rightRaw),
      operator: condition.operatorRaw & 0x0f,
    }))
}

export function createDefaultContext(startLocation: StartLocation): CommandContext {
  return {
    mapId: startLocation.mapId,
    eventId: null,
    commonEventId: null,
  }
}
