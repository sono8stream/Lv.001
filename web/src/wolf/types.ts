export const TILE_SIZE = 16

export type EventTriggerType =
  | 'check'
  | 'auto'
  | 'parallel'
  | 'playerContact'
  | 'eventContact'

export type DatabaseType = 'system' | 'changeable' | 'user'

export type Direction = 'down' | 'left' | 'right' | 'up'

export interface DatabaseColumn {
  name: string
  type: 'int' | 'string'
  initialValue: number
}

export interface DatabaseSchema {
  name: string
  columns: DatabaseColumn[]
}

export interface DatabaseRecord {
  name: string
  intData: number[]
  stringData: string[]
}

export interface UnitTile {
  movableType: 'movable' | 'immovable'
  passableByDirection: Record<Direction, boolean>
  isCounter: boolean
  tagNumber: number
}

export interface TileSetData {
  settingName: string
  baseTileFilePath: string
  autoTileFilePaths: string[]
  unitTiles: UnitTile[]
}

export interface PageCondition {
  operatorRaw: number
  leftRaw: number
  rightRaw: number
}

export interface EventMoveData {
  canPass: boolean
}

export interface MessageCommand {
  kind: 'message'
  indent: number
  text: string
}

export interface ChoiceCommand {
  kind: 'choice'
  indent: number
  options: string[]
}

export interface ForkBeginCommand {
  kind: 'forkBegin'
  indent: number
  label: string
}

export interface ForkEndCommand {
  kind: 'forkEnd'
  indent: number
  label: string
}

export interface NumberRefRaw {
  kind: 'raw'
  value: number
}

export interface NumberRefDatabase {
  kind: 'db'
  database: DatabaseType
  table: NumberRef
  record: NumberRef
  field: NumberRef
}

export type NumberRef = NumberRefRaw | NumberRefDatabase

export interface ConditionEntry {
  left: NumberRef
  right: NumberRef
  operator: number
}

export interface ConditionalForkCommand {
  kind: 'conditionalFork'
  indent: number
  conditions: ConditionEntry[]
}

export interface Updater {
  left: NumberRef
  right1: NumberRef
  right2: NumberRef | null
  assignOperator: number
  rightOperator: number
}

export interface ChangeVariableCommand {
  kind: 'changeVariable'
  indent: number
  updaters: Updater[]
}

export interface MovePositionCommand {
  kind: 'movePosition'
  indent: number
  targetEventId: number
  x: number
  y: number
  mapId: number
}

export interface CallEventCommand {
  kind: 'callEvent'
  indent: number
  eventLookup:
    | { type: 'id'; rawEventId: number }
    | { type: 'name'; name: string }
  numberArgs: NumberRef[]
  hasReturnValue: boolean
  returnDestination: NumberRef | null
}

export interface LoopStartCommand {
  kind: 'loopStart'
  indent: number
  isInfinite: boolean
  loopCount: NumberRef | null
}

export interface LoopEndCommand {
  kind: 'loopEnd'
  indent: number
}

export interface LoopBreakCommand {
  kind: 'loopBreak'
  indent: number
}

export type PicturePivot = 'leftTop' | 'center' | 'leftBottom' | 'rightTop' | 'rightBottom'

export interface ShowPictureCommand {
  kind: 'showPicture'
  indent: number
  pictureId: number
  filePath: string
  pivot: PicturePivot
  x: number
  y: number
  scale: number
}

export interface ShowMessagePictureCommand {
  kind: 'showMessagePicture'
  indent: number
  pictureId: NumberRef
  message: string
  pivot: PicturePivot
  x: NumberRef
  y: NumberRef
  scale: number
}

export interface RemovePictureCommand {
  kind: 'removePicture'
  indent: number
  pictureId: number
}

export interface UnknownCommand {
  kind: 'unknown'
  indent: number
  key: number
}

export type WolfCommand =
  | MessageCommand
  | ChoiceCommand
  | ForkBeginCommand
  | ForkEndCommand
  | ConditionalForkCommand
  | ChangeVariableCommand
  | MovePositionCommand
  | CallEventCommand
  | LoopStartCommand
  | LoopEndCommand
  | LoopBreakCommand
  | ShowPictureCommand
  | ShowMessagePictureCommand
  | RemovePictureCommand
  | UnknownCommand

export interface EventPage {
  pageIndex: number
  tileNo: number
  chipImgName: string
  direction: Direction
  hasDirection: boolean
  triggerType: EventTriggerType
  conditions: PageCondition[]
  moveData: EventMoveData
  commands: WolfCommand[]
}

export interface WolfMapEvent {
  id: number
  name: string
  x: number
  y: number
  pages: EventPage[]
}

export interface CommonEventData {
  id: number
  name: string
  commands: WolfCommand[]
  returnValueRaw: number | null
  numberVariables: number[]
  stringVariables: string[]
}

export interface WolfMapData {
  id: number
  width: number
  height: number
  tileSetId: number
  layers: number[][][]
  events: WolfMapEvent[]
  movableGrid: boolean[][]
  lowerCanvas: HTMLCanvasElement
  upperCanvas: HTMLCanvasElement
}

export interface StartLocation {
  mapId: number
  x: number
  y: number
}

export interface CommandContext {
  mapId: number
  eventId: number | null
  commonEventId: number | null
}
