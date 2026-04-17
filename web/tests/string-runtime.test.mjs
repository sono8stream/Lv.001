import assert from 'node:assert/strict'
import { after, before, test } from 'node:test'
import { spawn } from 'node:child_process'
import { setTimeout as delay } from 'node:timers/promises'
import { chromium } from 'playwright'

const BASE_URL = 'http://127.0.0.1:5173/'
let devServerProcess = null

async function isServerReady() {
  try {
    const response = await fetch(BASE_URL)
    return response.ok
  } catch {
    return false
  }
}

async function ensureServer() {
  if (await isServerReady()) {
    return
  }

  devServerProcess = spawn('npm', ['run', 'dev'], {
    cwd: new URL('..', import.meta.url),
    stdio: 'ignore',
  })

  for (let attempt = 0; attempt < 60; attempt += 1) {
    if (await isServerReady()) {
      return
    }
    await delay(500)
  }

  throw new Error('Vite dev server did not become ready on 127.0.0.1:5173')
}

async function withPage(run) {
  const browser = await chromium.launch({ headless: true })
  const page = await browser.newPage({ viewport: { width: 1280, height: 720 } })

  try {
    await page.goto(BASE_URL, { waitUntil: 'networkidle', timeout: 120000 })
    return await run(page)
  } finally {
    await browser.close()
  }
}

before(async () => {
  await ensureServer()
})

after(async () => {
  if (devServerProcess !== null) {
    devServerProcess.kill('SIGTERM')
  }
})

test('string command variants keep cself text stable', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_string_variants=1')
      const dataMod = await import('/src/wolf/data.ts?test_string_variants=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.repository = await dataMod.WolfDataRepository.create()
      const context = { mapId: 1, eventId: null, commonEventId: 70 }

      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600007, opRaw: 0, sourceRaw: null, texts: ['薬草'] }, context)
      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600008, opRaw: 769, sourceRaw: 1600007, texts: [] }, context)
      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600008, opRaw: 769, sourceRaw: 1600007, texts: [] }, context)
      runtime.assignNumberRef({ kind: 'raw', value: 1600011 }, 42, context)
      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600009, opRaw: 2, sourceRaw: 1600011, texts: [] }, context)

      return {
        s8: runtime.resolveStringRef(1600008, context),
        s9: runtime.resolveStringRef(1600009, context),
        message: runtime.interpolateString('「\\cself[8]」 x \\cself[9]', context),
      }
    }),
  )

  assert.equal(result.s8, '薬草')
  assert.equal(result.s9, '42')
  assert.equal(result.message, '「薬草」 x 42')
})

test('string-heavy common events only stall in known interactive loops', async () => {
  const profile = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_string_profile=1')
      const dataMod = await import('/src/wolf/data.ts?test_string_profile=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.showMessage = async () => {}
      runtime.showChoices = async () => 0
      runtime.resolveKeyInput = async () => 11
      runtime.waitFrames = async () => {}
      await runtime.boot()

      const events = runtime.repository.commonEvents
        .filter((event) =>
          event.commands.some((command) =>
            command.kind === 'changeString'
            || (command.kind === 'message' && (command.text.includes('\\cself[') || command.text.includes('\\self['))),
          ),
        )
        .map((event) => ({ id: event.id, name: event.name }))

      const results = []
      for (const event of events) {
        const commonEvent = runtime.repository.getCommonEventById(event.id)
        const command = {
          kind: 'callEvent',
          indent: 0,
          eventLookup: { type: 'name', name: commonEvent.name },
          numberArgs: [
            { kind: 'raw', value: 0 },
            { kind: 'raw', value: 1 },
            { kind: 'raw', value: 1 },
            { kind: 'raw', value: 0 },
          ],
          hasReturnValue: false,
          returnDestination: null,
        }

        try {
          await runtime.runCommonEvent(commonEvent, command, { mapId: 1, eventId: null, commonEventId: null })
        } catch (error) {
          results.push({
            id: commonEvent.id,
            name: commonEvent.name,
            error: String(error && error.message ? error.message : error),
          })
        }
      }

      return results
    }),
  )

  const stalledIds = profile.map((entry) => entry.id).sort((left, right) => left - right)
  assert.deepEqual(stalledIds, [])
})

test('cached string templates reflect updated cself values', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_cached_template=1')
      const dataMod = await import('/src/wolf/data.ts?test_cached_template=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.repository = await dataMod.WolfDataRepository.create()
      const context = { mapId: 1, eventId: null, commonEventId: 70 }
      const template = 'before=\\cself[7], after=\\cself[7]'

      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600007, opRaw: 0, sourceRaw: null, texts: ['薬草'] }, context)
      const before = runtime.interpolateString(template, context)

      runtime.applyChangeString({ kind: 'changeString', indent: 0, targetRaw: 1600007, opRaw: 0, sourceRaw: null, texts: ['特薬草'] }, context)
      const after = runtime.interpolateString(template, context)

      return { before, after }
    }),
  )

  assert.equal(result.before, 'before=薬草, after=薬草')
  assert.equal(result.after, 'before=特薬草, after=特薬草')
})

test('repeated cself interpolation resolves the common event once per template render', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_cself_lookup_count=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      let lookups = 0
      runtime.repository = {
        systemDb: { getString: () => '', getInt: () => 0, setInt: () => {} },
        changeableDb: { getString: () => '', getInt: () => 0, setInt: () => {} },
        userDb: { getString: () => '', getInt: () => 0, setInt: () => {} },
        getCommonEventById(id) {
          lookups += 1
          return {
            id,
            name: 'lookup-test',
            commands: [],
            returnValueRaw: null,
            numberVariables: [7],
            stringVariables: ['薬草'],
          }
        },
      }

      const text = runtime.interpolateString('\\cself[5]/\\cself[5]/\\cself[0]/\\cself[0]', {
        mapId: 1,
        eventId: null,
        commonEventId: 215,
      })

      return { text, lookups }
    }),
  )

  assert.equal(result.text, '薬草/薬草/7/7')
  assert.equal(result.lookups, 1)
})

test('string debug common event shows the hero name from changeable DB', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_common_event_215=1')
      const dataMod = await import('/src/wolf/data.ts?test_common_event_215=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.repository = await dataMod.WolfDataRepository.create()
      const commonEvent = runtime.repository.getCommonEventById(215)
      const messages = []
      runtime.showMessage = async (message) => {
        messages.push(message)
      }

      await runtime.runCommonEvent(
        commonEvent,
        {
          kind: 'callEvent',
          indent: 0,
          eventLookup: { type: 'id', rawEventId: 500215 },
          numberArgs: [],
          hasReturnValue: false,
          returnDestination: null,
        },
        { mapId: 1, eventId: 23, commonEventId: null },
      )

      return messages
    }),
  )

  assert.equal(result[0], '数値変数は1')
  assert.ok(result.slice(1).every((message) => message === '文字列変数はヒーローさん'))
})

test('debug panel shows executing event and command payload', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_debug_panel=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        debugPanel: document.querySelector('#debugPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.repository = {
        getCommonEventById(id) {
          return { id, name: 'debug-common', commands: [], returnValueRaw: null, numberVariables: [], stringVariables: [] }
        },
      }
      runtime.currentMap = {
        id: 1,
        events: [{ id: 23, name: '文字列操作のデバッグ用' }],
      }

      let snapshot = ''
      runtime.showMessage = async () => {
        snapshot = runtime.elements.debugPanel.textContent
      }

      await runtime.executeCommands(
        [{ kind: 'message', indent: 0, text: 'debug text' }],
        { mapId: 1, eventId: 23, commonEventId: 215 },
      )

      return {
        snapshot,
        after: runtime.elements.debugPanel.textContent,
      }
    }),
  )

  assert.match(result.snapshot, /mapEvent: 23 文字列操作のデバッグ用/)
  assert.match(result.snapshot, /commonEvent: 215 debug-common/)
  assert.match(result.snapshot, /kind: message/)
  assert.match(result.snapshot, /"text": "debug text"/)
  assert.match(result.after, /debug: idle/)
})

test('playerContact on blocked tile triggers before moving onto the event', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_player_contact_blocked=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.startLocation = { mapId: 1, x: 1, y: 1 }
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, false, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [{
          id: 10,
          name: 'blocked-contact',
          x: 2,
          y: 1,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'down',
            hasDirection: false,
            triggerType: 'playerContact',
            conditions: [],
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 3,
              moveType: 0,
              optionFlags: 8,
              moveFlags: 0,
              canPass: true,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 1

      const triggered = []
      runtime.runMapEvent = async (event) => {
        triggered.push({ id: event.id, x: runtime.playerX, y: runtime.playerY })
      }

      runtime.pressedKeys.add('ArrowRight')
      await runtime.stepPlayer()

      return { x: runtime.playerX, y: runtime.playerY, triggered }
    }),
  )

  assert.equal(result.x, 1)
  assert.equal(result.y, 1)
  assert.deepEqual(result.triggered, [{ id: 10, x: 1, y: 1 }])
})

test('playerContact on passable tile triggers after moving onto the event', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_player_contact_passable=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.startLocation = { mapId: 1, x: 1, y: 1 }
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [{
          id: 11,
          name: 'passable-contact',
          x: 2,
          y: 1,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'down',
            hasDirection: false,
            triggerType: 'playerContact',
            conditions: [],
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 3,
              moveType: 0,
              optionFlags: 8,
              moveFlags: 0,
              canPass: true,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 1

      const triggered = []
      runtime.runMapEvent = async (event) => {
        triggered.push({ id: event.id, x: runtime.playerX, y: runtime.playerY })
      }

      runtime.pressedKeys.add('ArrowRight')
      await runtime.stepPlayer()

      return { x: runtime.playerX, y: runtime.playerY, triggered }
    }),
  )

  assert.equal(result.x, 2)
  assert.equal(result.y, 1)
  assert.deepEqual(result.triggered, [{ id: 11, x: 2, y: 1 }])
})

test('eventContact also triggers when the player walks into the event', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_event_contact_player_side=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.startLocation = { mapId: 1, x: 1, y: 1 }
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, false, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [{
          id: 12,
          name: 'event-contact',
          x: 2,
          y: 1,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'down',
            hasDirection: false,
            triggerType: 'eventContact',
            conditions: [],
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 3,
              moveType: 0,
              optionFlags: 8,
              moveFlags: 0,
              canPass: true,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 1

      const triggered = []
      runtime.runMapEvent = async (event) => {
        triggered.push({ id: event.id, x: runtime.playerX, y: runtime.playerY })
      }

      runtime.pressedKeys.add('ArrowRight')
      await runtime.stepPlayer()

      return { x: runtime.playerX, y: runtime.playerY, triggered }
    }),
  )

  assert.equal(result.x, 1)
  assert.equal(result.y, 1)
  assert.deepEqual(result.triggered, [{ id: 12, x: 1, y: 1 }])
})

test('eventContact triggers when a moving event reaches the player', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_event_contact_event_side=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [{
          id: 13,
          name: 'moving-event-contact',
          x: 2,
          y: 1,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'left',
            hasDirection: false,
            triggerType: 'eventContact',
            conditions: [],
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 5,
              moveType: 3,
              optionFlags: 0,
              moveFlags: 0,
              canPass: false,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 0
      runtime.playerY = 1

      const triggered = []
      runtime.runMapEvent = async (event) => {
        const pageData = runtime.getActivePage(event)
        const state = runtime.getEventState(event, pageData)
        triggered.push({ id: event.id, eventX: state.x, eventY: state.y, playerX: runtime.playerX, playerY: runtime.playerY })
      }

      const event = runtime.currentMap.events[0]
      const pageData = runtime.getActivePage(event)
      await runtime.processEventMovement()
      runtime.getEventState(event, pageData).moveCooldownRemaining = 0
      await runtime.processEventMovement()

      const state = runtime.getEventState(event, pageData)
      return { x: state.x, y: state.y, triggered }
    }),
  )

  assert.equal(result.x, 1)
  assert.equal(result.y, 1)
  assert.deepEqual(result.triggered, [{ id: 13, eventX: 1, eventY: 1, playerX: 0, playerY: 1 }])
})

test('pass-through moving event can overlap the player and trigger eventContact', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_event_contact_overlap=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [{
          id: 14,
          name: 'overlap-event-contact',
          x: 1,
          y: 0,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'left',
            hasDirection: false,
            triggerType: 'eventContact',
            conditions: [],
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 5,
              moveType: 1,
              optionFlags: 8,
              moveFlags: 1,
              canPass: true,
              moveCommands: [{ commandType: 0x01, args: [] }],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 0
      runtime.playerY = 0

      const triggered = []
      runtime.runMapEvent = async (event) => {
        const pageData = runtime.getActivePage(event)
        const state = runtime.getEventState(event, pageData)
        triggered.push({ id: event.id, eventX: state.x, eventY: state.y })
      }

      const event = runtime.currentMap.events[0]
      const pageData = runtime.getActivePage(event)
      await runtime.processEventMovement()

      const state = runtime.getEventState(event, pageData)
      return { x: state.x, y: state.y, triggered }
    }),
  )

  assert.equal(result.x, 0)
  assert.equal(result.y, 0)
  assert.deepEqual(result.triggered, [{ id: 14, eventX: 0, eventY: 0 }])
})

test('cancel key opens the menu common event when the player is idle', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_open_menu_idle=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.repository = {
        commonEvents: [],
        getCommonEventByName(name) {
          return name === 'X[移]メニュー起動'
            ? { id: 127, name, commands: [], returnValueRaw: null, numberVariables: [], stringVariables: [] }
            : null
        },
        getCommonEventById() {
          return null
        },
      }
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }

      const calls = []
      runtime.runCommonEvent = async (commonEvent, command, context) => {
        calls.push({
          id: commonEvent.id,
          name: commonEvent.name,
          lookupType: command.eventLookup.type,
          mapId: context.mapId,
        })
      }

      runtime.pressedKeys.add('Escape')
      await runtime.tick()
      return {
        calls,
        escapeStillPressed: runtime.pressedKeys.has('Escape'),
      }
    }),
  )

  assert.deepEqual(result.calls, [{ id: 127, name: 'X[移]メニュー起動', lookupType: 'name', mapId: 1 }])
  assert.equal(result.escapeStillPressed, false)
})

test('cancel key does not open the menu while another event is running', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_open_menu_busy=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 64
      layer.height = 64
      runtime.repository = {
        commonEvents: [],
        getCommonEventByName(name) {
          return name === 'X[移]メニュー起動'
            ? { id: 127, name, commands: [], returnValueRaw: null, numberVariables: [], stringVariables: [] }
            : null
        },
        getCommonEventById() {
          return null
        },
      }
      runtime.currentMap = {
        id: 1,
        width: 4,
        height: 4,
        movableGrid: [
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
          [true, true, true, true],
        ],
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }

      let callCount = 0
      runtime.runCommonEvent = async () => {
        callCount += 1
      }

      runtime.eventBusy = true
      runtime.pressedKeys.add('Escape')
      await runtime.tick()
      return {
        callCount,
        escapeStillPressed: runtime.pressedKeys.has('Escape'),
      }
    }),
  )

  assert.equal(result.callCount, 0)
  assert.equal(result.escapeStillPressed, true)
})

test('check trigger respects expanded trigger range', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_check_range=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 96
      layer.height = 96
      runtime.currentMap = {
        id: 1,
        width: 6,
        height: 6,
        movableGrid: Array.from({ length: 6 }, () => Array.from({ length: 6 }, () => true)),
        events: [{
          id: 20,
          name: 'wide-check',
          x: 3,
          y: 2,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'left',
            hasDirection: false,
            triggerType: 'check',
            conditions: [],
            rangeExtendX: 1,
            rangeExtendY: 0,
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 3,
              moveType: 0,
              optionFlags: 8,
              moveFlags: 0,
              canPass: true,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 2
      runtime.playerDirection = 'right'

      const triggered = []
      runtime.runMapEvent = async (event) => {
        triggered.push(event.id)
      }

      await runtime.tryInteract()
      return triggered
    }),
  )

  assert.deepEqual(result, [20])
})

test('playerContact respects expanded trigger range', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_player_contact_range=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 96
      layer.height = 96
      runtime.currentMap = {
        id: 1,
        width: 6,
        height: 6,
        movableGrid: Array.from({ length: 6 }, () => Array.from({ length: 6 }, () => true)),
        events: [{
          id: 21,
          name: 'wide-player-contact',
          x: 3,
          y: 2,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'down',
            hasDirection: false,
            triggerType: 'playerContact',
            conditions: [],
            rangeExtendX: 1,
            rangeExtendY: 0,
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 3,
              moveType: 0,
              optionFlags: 8,
              moveFlags: 0,
              canPass: true,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 2

      const triggered = []
      runtime.runMapEvent = async (event) => {
        triggered.push({ id: event.id, x: runtime.playerX, y: runtime.playerY })
      }

      runtime.pressedKeys.add('ArrowRight')
      await runtime.stepPlayer()
      return { x: runtime.playerX, y: runtime.playerY, triggered }
    }),
  )

  assert.equal(result.x, 2)
  assert.equal(result.y, 2)
  assert.deepEqual(result.triggered, [{ id: 21, x: 2, y: 2 }])
})

test('moving eventContact respects expanded trigger range', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_event_contact_range=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 96
      layer.height = 96
      runtime.currentMap = {
        id: 1,
        width: 6,
        height: 6,
        movableGrid: Array.from({ length: 6 }, () => Array.from({ length: 6 }, () => true)),
        events: [{
          id: 22,
          name: 'wide-event-contact',
          x: 3,
          y: 2,
          pages: [{
            pageIndex: 0,
            tileNo: -1,
            chipImgName: '',
            direction: 'left',
            hasDirection: false,
            triggerType: 'eventContact',
            conditions: [],
            rangeExtendX: 1,
            rangeExtendY: 0,
            moveData: {
              animationSpeed: 3,
              moveSpeed: 3,
              moveFrequency: 5,
              moveType: 3,
              optionFlags: 0,
              moveFlags: 0,
              canPass: false,
              moveCommands: [],
            },
            commands: [],
          }],
        }],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 2

      const triggered = []
      runtime.runMapEvent = async (event) => {
        const pageData = runtime.getActivePage(event)
        const state = runtime.getEventState(event, pageData)
        triggered.push({ id: event.id, eventX: state.x, eventY: state.y })
      }

      await runtime.processEventMovement()
      const event = runtime.currentMap.events[0]
      const pageData = runtime.getActivePage(event)
      const state = runtime.getEventState(event, pageData)
      return { x: state.x, y: state.y, triggered }
    }),
  )

  assert.equal(result.x, 2)
  assert.equal(result.y, 2)
  assert.deepEqual(result.triggered, [{ id: 22, eventX: 2, eventY: 2 }])
})

test('holding a movement key repeats player movement across ticks', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_hold_move_repeat=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 128
      layer.height = 128
      runtime.repository = { commonEvents: [] }
      runtime.currentMap = {
        id: 1,
        width: 8,
        height: 4,
        movableGrid: Array.from({ length: 4 }, () => Array.from({ length: 8 }, () => true)),
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 1

      runtime.pressedKeys.add('ArrowRight')
      for (let index = 0; index < 12; index += 1) {
        await runtime.tick()
      }
      return { x: runtime.playerX, y: runtime.playerY }
    }),
  )

  assert.equal(result.x, 3)
  assert.equal(result.y, 1)
})

test('virtual movement keys stay pressed until released explicitly', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_virtual_hold=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.pressVirtualKey('ArrowRight')
      await new Promise((resolve) => window.setTimeout(resolve, 220))
      const stillPressed = runtime.pressedKeys.has('ArrowRight')
      runtime.releaseVirtualKey('ArrowRight')
      return { stillPressed, released: runtime.pressedKeys.has('ArrowRight') }
    }),
  )

  assert.equal(result.stillPressed, true)
  assert.equal(result.released, false)
})

test('player walking animation advances on movement and returns to standing frame when idle', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_player_walk_anim=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 128
      layer.height = 128
      runtime.repository = { commonEvents: [] }
      runtime.currentMap = {
        id: 1,
        width: 8,
        height: 4,
        movableGrid: Array.from({ length: 4 }, () => Array.from({ length: 8 }, () => true)),
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.playerX = 1
      runtime.playerY = 1

      runtime.pressedKeys.add('ArrowRight')
      await runtime.stepPlayer()
      runtime.pressedKeys.delete('ArrowRight')
      const movingFrame = runtime.playerAnimationFrame
      for (let index = 0; index < 8; index += 1) {
        await runtime.tick()
      }
      return { movingFrame, idleFrame: runtime.playerAnimationFrame }
    }),
  )

  assert.notEqual(result.movingFrame, 1)
  assert.equal(result.idleFrame, 1)
})

test('command inventory groups supported kinds and unsupported raw keys', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const inventoryMod = await import('/src/wolf/command-inventory.ts?test_inventory_grouping=1')
      return inventoryMod.buildCommandInventory(
        [{ id: 1, name: 'common-a', commands: [{ kind: 'message' }, { kind: 'unknown', key: 999 }, { kind: 'message' }] }],
        [{ id: 1, events: [{ id: 3, name: 'map-a', pages: [{ pageIndex: 0, commands: [{ kind: 'choice' }, { kind: 'unknown', key: 999 }, { kind: 'unknown', key: 314 }] }] }] }],
      )
    }),
  )

  assert.equal(result.totalCommands, 6)
  assert.deepEqual(result.supportedKinds, [
    { kind: 'message', count: 2 },
    { kind: 'choice', count: 1 },
  ])
  assert.deepEqual(result.unsupportedCommandKeys, [
    { key: 314, count: 1, samples: [{ scope: 'map', mapId: 1, eventId: 3, eventName: 'map-a', pageIndex: 0 }] },
    { key: 999, count: 2, samples: [
      { scope: 'common', commonEventId: 1, commonEventName: 'common-a' },
      { scope: 'map', mapId: 1, eventId: 3, eventName: 'map-a', pageIndex: 0 },
    ] },
  ])
})

test('command inventory snapshot matches current repository data', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const inventoryMod = await import('/src/wolf/command-inventory.ts?test_inventory_snapshot=1')
      const dataMod = await import('/src/wolf/data.ts?test_inventory_snapshot=1')
      const repo = await dataMod.WolfDataRepository.create()
      const maps = []
      let consecutiveMisses = 0
      for (let mapId = 1; mapId <= 300 && consecutiveMisses < 40; mapId += 1) {
        try {
          maps.push(await repo.loadMap(mapId))
          consecutiveMisses = 0
        } catch {
          consecutiveMisses += 1
        }
      }

      const generated = inventoryMod.buildCommandInventory(repo.commonEvents, maps)
      const committed = await fetch('/command-inventory.json').then((response) => response.json())
      return { generated, committed }
    }),
  )

  assert.deepEqual(result.generated, result.committed)
})

test('menu common event parses loop-continue commands with a dedicated kind', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const dataMod = await import('/src/wolf/data.ts?test_menu_loop_continue_kind=1')
      const repo = await dataMod.WolfDataRepository.create()
      const commonEvent = repo.getCommonEventById(127)

      return {
        name: commonEvent.name,
        continueKinds: [
          commonEvent.commands[80].kind,
          commonEvent.commands[88].kind,
          commonEvent.commands[103].kind,
        ],
      }
    }),
  )

  assert.equal(result.name, 'X[移]メニュー起動')
  assert.deepEqual(result.continueKinds, ['loopContinue', 'loopContinue', 'loopContinue'])
})

test('loop-continue jumps back to the current loop start without running the rest of the body', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_loop_continue_runtime=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const commonEvent = { id: 127, name: 'X[移]メニュー起動', commands: [], returnValueRaw: null, numberVariables: [], stringVariables: [] }
      runtime.repository = {
        commonEvents: [],
        getCommonEventById(id) {
          return id === 127 ? commonEvent : null
        },
        getCommonEventByName() {
          return null
        },
      }

      const context = { mapId: 1, eventId: null, commonEventId: 127 }
      const countRaw = 1600010
      const bodyRaw = 1600011
      const commands = [
        { kind: 'loopStart', indent: 0, isInfinite: false, loopCount: { kind: 'raw', value: 3 } },
        {
          kind: 'changeVariable',
          indent: 1,
          updaters: [{ left: { kind: 'raw', value: countRaw }, right1: { kind: 'raw', value: 1 }, right2: { kind: 'raw', value: 0 }, assignOperator: 1, rightOperator: 0 }],
        },
        {
          kind: 'conditionalFork',
          indent: 1,
          conditions: [{ left: { kind: 'raw', value: countRaw }, right: { kind: 'raw', value: 2 }, operator: 2 }],
        },
        { kind: 'forkBegin', indent: 1, label: '1.1' },
        { kind: 'loopContinue', indent: 2 },
        { kind: 'blank', indent: 2 },
        { kind: 'forkEnd', indent: 1, label: '1.0' },
        {
          kind: 'changeVariable',
          indent: 1,
          updaters: [{ left: { kind: 'raw', value: bodyRaw }, right1: { kind: 'raw', value: 1 }, right2: { kind: 'raw', value: 0 }, assignOperator: 1, rightOperator: 0 }],
        },
        { kind: 'loopEnd', indent: 0 },
      ]

      await runtime.executeCommands(commands, context)
      return {
        count: runtime.resolveNumberRef({ kind: 'raw', value: countRaw }, context),
        body: runtime.resolveNumberRef({ kind: 'raw', value: bodyRaw }, context),
      }
    }),
  )

  assert.equal(result.count, 3)
  assert.equal(result.body, 2)
})

test('menu draw common events parse dedicated picture helper kinds', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const dataMod = await import('/src/wolf/data.ts?test_menu_draw_kinds=1')
      const repo = await dataMod.WolfDataRepository.create()
      const windowDraw = repo.getCommonEventById(64)
      const menuDraw = repo.getCommonEventById(88)
      const characterDraw = repo.getCommonEventById(90)

      return {
        dynamicPictureKind: windowDraw.commands[177].kind,
        movePictureKind: menuDraw.commands[192].kind,
        checkpointKind: menuDraw.commands[199].kind,
        picturePropertyKind: menuDraw.commands[186].kind,
        pictureEffectKind: characterDraw.commands[241].kind,
      }
    }),
  )

  assert.equal(result.dynamicPictureKind, 'showPictureString')
  assert.equal(result.movePictureKind, 'movePicture')
  assert.equal(result.checkpointKind, 'checkpoint')
  assert.equal(result.picturePropertyKind, 'readPictureProperty')
  assert.equal(result.pictureEffectKind, 'pictureEffect')
})

test('picture helper commands read size and move an existing picture entry', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_picture_helpers_runtime=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const commonEvent = { id: 127, name: 'X[移]メニュー起動', commands: [], returnValueRaw: null, numberVariables: [], stringVariables: [] }
      runtime.repository = {
        commonEvents: [],
        getCommonEventById(id) {
          return id === 127 ? commonEvent : null
        },
        getCommonEventByName() {
          return null
        },
      }

      const entry = document.createElement('div')
      entry.className = 'picture-entry'
      entry.style.position = 'absolute'
      entry.style.width = '96px'
      entry.style.height = '32px'
      entry.style.left = '12px'
      entry.style.top = '20px'
      entry.style.transform = 'scale(1)'
      runtime.elements.pictureLayer.append(entry)
      runtime.pictureEntries.set(7, entry)

      const context = { mapId: 1, eventId: null, commonEventId: 127 }
      await runtime.executeCommands([
        { kind: 'readPictureProperty', indent: 0, targetRaw: 1600010, pictureId: { kind: 'raw', value: 7 }, propertyId: 2 },
        { kind: 'readPictureProperty', indent: 0, targetRaw: 1600011, pictureId: { kind: 'raw', value: 7 }, propertyId: 3 },
        { kind: 'movePicture', indent: 0, pictureId: { kind: 'raw', value: 7 }, x: { kind: 'raw', value: 48 }, y: { kind: 'raw', value: 72 }, scale: { kind: 'raw', value: 200 } },
      ], context)

      return {
        width: runtime.resolveNumberRef({ kind: 'raw', value: 1600010 }, context),
        height: runtime.resolveNumberRef({ kind: 'raw', value: 1600011 }, context),
        left: entry.style.left,
        top: entry.style.top,
        transform: entry.style.transform,
      }
    }),
  )

  assert.equal(result.width, 96)
  assert.equal(result.height, 32)
  assert.equal(result.left, '48px')
  assert.equal(result.top, '72px')
  assert.equal(result.transform, 'scale(2)')
})

test('menu common event produces window-like picture entries after draw helpers run', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_menu_draw_positions=1')
      const dataMod = await import('/src/wolf/data.ts?test_menu_draw_positions=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        debugPanel: document.querySelector('#debugPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 320
      layer.height = 240
      runtime.repository = await dataMod.WolfDataRepository.create()
      runtime.currentMap = {
        id: 1,
        width: 20,
        height: 15,
        movableGrid: Array.from({ length: 20 }, () => Array.from({ length: 15 }, () => true)),
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.startLocation = { mapId: 1, x: 12, y: 8 }
      runtime.resolveKeyInput = async () => 11
      runtime.waitFrames = async () => {}
      runtime.showMessage = async () => {}

      const menu = runtime.repository.getCommonEventById(127)
      await runtime.runCommonEvent(menu, {
        kind: 'callEvent',
        indent: 0,
        eventLookup: { type: 'name', name: menu.name },
        numberArgs: [],
        hasReturnValue: false,
        returnDestination: null,
      }, { mapId: 1, eventId: null, commonEventId: null })

      const entries = [...runtime.elements.pictureLayer.children].map((node) => ({
        background: node.style.background,
        width: node.style.width,
        height: node.style.height,
      }))

      return {
        pictureCount: entries.length,
        windowHelperCount: entries.filter((entry) => entry.background.length > 0 || entry.width.length > 0 || entry.height.length > 0).length,
      }
    }),
  )

  assert.ok(result.pictureCount >= 10)
  assert.ok(result.windowHelperCount >= 3)
})

test('menu picture text strips formatting control tokens before rendering', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_menu_text_rendering=1')
      const dataMod = await import('/src/wolf/data.ts?test_menu_text_rendering=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        debugPanel: document.querySelector('#debugPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 320
      layer.height = 240
      runtime.repository = await dataMod.WolfDataRepository.create()
      runtime.currentMap = {
        id: 1,
        width: 20,
        height: 15,
        movableGrid: Array.from({ length: 20 }, () => Array.from({ length: 15 }, () => true)),
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.startLocation = { mapId: 1, x: 12, y: 8 }
      runtime.resolveKeyInput = async () => 11
      runtime.waitFrames = async () => {}
      runtime.showMessage = async () => {}

      const menu = runtime.repository.getCommonEventById(127)
      await runtime.runCommonEvent(menu, {
        kind: 'callEvent',
        indent: 0,
        eventLookup: { type: 'name', name: menu.name },
        numberArgs: [],
        hasReturnValue: false,
        returnDestination: null,
      }, { mapId: 1, eventId: null, commonEventId: null })

      return [...runtime.elements.pictureLayer.children]
        .map((node) => node.textContent ?? '')
        .filter((text) => text.length > 0)
    }),
  )

  assert.equal(result.some((text) => /\\f\[|\\ax\[|\\ay\[|<R>|\\A|\\E/.test(text)), false)
})

test('menu window helper entries have non-zero size', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_menu_window_size=1')
      const dataMod = await import('/src/wolf/data.ts?test_menu_window_size=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        debugPanel: document.querySelector('#debugPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      const layer = document.createElement('canvas')
      layer.width = 320
      layer.height = 240
      runtime.repository = await dataMod.WolfDataRepository.create()
      runtime.currentMap = {
        id: 1,
        width: 20,
        height: 15,
        movableGrid: Array.from({ length: 20 }, () => Array.from({ length: 15 }, () => true)),
        events: [],
        lowerCanvas: layer,
        upperCanvas: layer,
      }
      runtime.startLocation = { mapId: 1, x: 12, y: 8 }
      runtime.resolveKeyInput = async () => 11
      runtime.waitFrames = async () => {}
      runtime.showMessage = async () => {}

      const menu = runtime.repository.getCommonEventById(127)
      await runtime.runCommonEvent(menu, {
        kind: 'callEvent',
        indent: 0,
        eventLookup: { type: 'name', name: menu.name },
        numberArgs: [],
        hasReturnValue: false,
        returnDestination: null,
      }, { mapId: 1, eventId: null, commonEventId: null })

      const helpers = [...runtime.elements.pictureLayer.children].map((node) => ({
        background: node.style.background,
        width: node.style.width,
        height: node.style.height,
      })).filter((entry) => entry.background.length > 0 || entry.width.length > 0 || entry.height.length > 0)

      return {
        helperCount: helpers.length,
        nonZeroSizeCount: helpers.filter((entry) => entry.width !== '0px' || entry.height !== '0px').length,
      }
    }),
  )

  assert.ok(result.helperCount >= 3)
  assert.ok(result.nonZeroSizeCount >= 3)
})

test('shop common event parses control-flow commands with dedicated kinds', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const dataMod = await import('/src/wolf/data.ts?test_shop_command_kinds=1')
      const repo = await dataMod.WolfDataRepository.create()
      const commonEvent = repo.getCommonEventById(84)

      return {
        firstKinds: commonEvent.commands.slice(0, 12).map((command) => command.kind),
        keyInputKind: commonEvent.commands[47].kind,
        elseKind: commonEvent.commands[98].kind,
        labelSetKind: commonEvent.commands[24].kind,
        labelJumpKind: commonEvent.commands[80].kind,
        waitKind: commonEvent.commands[252].kind,
        abortKind: commonEvent.commands[253].kind,
      }
    }),
  )

  assert.equal(result.firstKinds[0], 'debugComment')
  assert.equal(result.firstKinds[11], 'blank')
  assert.equal(result.keyInputKind, 'keyInput')
  assert.equal(result.elseKind, 'branchElse')
  assert.equal(result.labelSetKind, 'labelSet')
  assert.equal(result.labelJumpKind, 'labelJump')
  assert.equal(result.waitKind, 'wait')
  assert.equal(result.abortKind, 'abortEvent')
})

test('chef takeout branch enters shop and exits on cancel input', async () => {
  const result = await withPage(async (page) =>
    page.evaluate(async () => {
      const runtimeMod = await import('/src/wolf/runtime.ts?test_chef_takeout=1')
      const dataMod = await import('/src/wolf/data.ts?test_chef_takeout=1')
      const runtime = new runtimeMod.WolfRuntime({
        canvas: document.querySelector('#gameCanvas'),
        statusPanel: document.querySelector('#statusPanel'),
        debugPanel: document.querySelector('#debugPanel'),
        messageBox: document.querySelector('#messageBox'),
        messageText: document.querySelector('#messageText'),
        choiceBox: document.querySelector('#choiceBox'),
        choiceList: document.querySelector('#choiceList'),
        choiceTitle: document.querySelector('#choiceTitle'),
        pictureLayer: document.querySelector('#pictureLayer'),
        errorBox: document.querySelector('#errorBox'),
      })

      runtime.repository = await dataMod.WolfDataRepository.create()
      runtime.startLocation = { mapId: 1, x: 12, y: 8 }
      await runtime.changeMap(1, 12, 8)

      const chef = runtime.currentMap.events.find((event) => event.id === 9)
      const pageData = runtime.getActivePage(chef)
      const snapshots = []

      runtime.showMessage = async (message) => {
        snapshots.push({ type: 'message', message })
      }
      runtime.showChoices = async (command) => {
        snapshots.push({ type: 'choice', options: command.options })
        return 1
      }
      runtime.resolveKeyInput = async () => 11
      runtime.waitFrames = async () => {}

      try {
        await runtime.executeCommands(pageData.commands, { mapId: 1, eventId: chef.id, commonEventId: null })
        return {
          ok: true,
          debug: runtime.elements.debugPanel?.textContent ?? '',
          snapshots,
        }
      } catch (error) {
        return {
          ok: false,
          error: String(error && error.message ? error.message : error),
          debug: runtime.elements.debugPanel?.textContent ?? '',
          snapshots,
        }
      }
    }),
  )

  assert.equal(result.ok, true)
  assert.equal(result.snapshots.find((entry) => entry.type === 'choice').options[1], 'お持ち帰りでお願いします')
  assert.match(result.debug, /debug: idle/)
})
