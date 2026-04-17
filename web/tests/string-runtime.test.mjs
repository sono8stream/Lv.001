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
  assert.deepEqual(stalledIds, [109, 131, 159, 161])
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
