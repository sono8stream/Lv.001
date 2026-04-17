import type { CommonEventData, UnknownCommand, WolfCommand, WolfMapData } from './types'

export interface SupportedCommandInventoryEntry {
  kind: string
  count: number
}

export interface CommonCommandSample {
  scope: 'common'
  commonEventId: number
  commonEventName: string
}

export interface MapCommandSample {
  scope: 'map'
  mapId: number
  eventId: number
  eventName: string
  pageIndex: number
}

export type UnsupportedCommandSample = CommonCommandSample | MapCommandSample

export interface UnsupportedCommandInventoryEntry {
  key: number
  count: number
  samples: UnsupportedCommandSample[]
}

export interface CommandInventory {
  totalCommands: number
  supportedKinds: SupportedCommandInventoryEntry[]
  unsupportedCommandKeys: UnsupportedCommandInventoryEntry[]
}

const MAX_SAMPLES_PER_KEY = 8

function isUnknownCommand(command: WolfCommand): command is UnknownCommand {
  return command.kind === 'unknown'
}

function sortSupportedKinds(entries: Map<string, number>): SupportedCommandInventoryEntry[] {
  return [...entries.entries()]
    .map(([kind, count]) => ({ kind, count }))
    .sort((left, right) => right.count - left.count || left.kind.localeCompare(right.kind))
}

function sortUnsupportedKeys(
  entries: Map<number, { count: number; samples: UnsupportedCommandSample[] }>,
): UnsupportedCommandInventoryEntry[] {
  return [...entries.entries()]
    .map(([key, value]) => ({ key, count: value.count, samples: value.samples }))
    .sort((left, right) => left.key - right.key)
}

function pushUnsupportedSample(
  entry: { count: number; samples: UnsupportedCommandSample[] },
  sample: UnsupportedCommandSample,
): void {
  if (entry.samples.length >= MAX_SAMPLES_PER_KEY) {
    return
  }
  entry.samples.push(sample)
}

export function buildCommandInventory(commonEvents: CommonEventData[], maps: WolfMapData[]): CommandInventory {
  let totalCommands = 0
  const supportedKinds = new Map<string, number>()
  const unsupportedCommandKeys = new Map<number, { count: number; samples: UnsupportedCommandSample[] }>()

  for (const commonEvent of commonEvents) {
    for (const command of commonEvent.commands) {
      totalCommands += 1
      if (isUnknownCommand(command)) {
        const entry = unsupportedCommandKeys.get(command.key) ?? { count: 0, samples: [] }
        entry.count += 1
        if (!unsupportedCommandKeys.has(command.key)) {
          unsupportedCommandKeys.set(command.key, entry)
        }
        pushUnsupportedSample(entry, {
          scope: 'common',
          commonEventId: commonEvent.id,
          commonEventName: commonEvent.name,
        })
        continue
      }

      supportedKinds.set(command.kind, (supportedKinds.get(command.kind) ?? 0) + 1)
    }
  }

  for (const map of maps) {
    for (const event of map.events) {
      for (const page of event.pages) {
        for (const command of page.commands) {
          totalCommands += 1
          if (isUnknownCommand(command)) {
            const entry = unsupportedCommandKeys.get(command.key) ?? { count: 0, samples: [] }
            entry.count += 1
            if (!unsupportedCommandKeys.has(command.key)) {
              unsupportedCommandKeys.set(command.key, entry)
            }
            pushUnsupportedSample(entry, {
              scope: 'map',
              mapId: map.id,
              eventId: event.id,
              eventName: event.name,
              pageIndex: page.pageIndex,
            })
            continue
          }

          supportedKinds.set(command.kind, (supportedKinds.get(command.kind) ?? 0) + 1)
        }
      }
    }
  }

  return {
    totalCommands,
    supportedKinds: sortSupportedKinds(supportedKinds),
    unsupportedCommandKeys: sortUnsupportedKeys(unsupportedCommandKeys),
  }
}
