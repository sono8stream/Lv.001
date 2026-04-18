# Battle implementation plan

## Goal

Enable the first playable battle vertical slice in the web runtime without trying to finish the whole battle system at once.

## Chosen first milestone

Because battle is broad and the user was unavailable to refine scope, the initial target is the **smallest end-to-end slice**:

1. A battle encounter can start from the existing basic-system common events.
2. The main battle common events run instead of stopping at the current "not supported" guard.
3. Initial battle state and battle-screen drawing execute far enough to show the battle scene and command UI in a stable layout.
4. A single player-side command path can advance through one turn and reach the existing battle-end cleanup path.
5. The slice is covered by automated tests before expanding command coverage, enemy AI, and polish.

## Current findings

- The runtime still hard-blocks battle entry in `web/src/wolf/runtime.ts` for:
  - `◆バトルの発生`
  - `X◆戦闘処理`
- The project already contains a large battle common-event graph, including:
  - `28` `◆バトルの発生`
  - `188` `X◆戦闘処理`
  - `189` `X┣◆戦闘初期化`
  - `190` `X┣◆戦闘キャラ配置`
  - `201` `X┗◆戦闘終了処理`
  - several DB-copy, command-list, message, damage, and status-calculation helpers
- Main battle event `188` parses mostly into supported command kinds, but still contains **one `unknown` command** (`key: 124`) that must be identified before the first slice can run reliably.
- Existing web tests do **not** yet cover battle flow.

## Proposed implementation phases

### Phase 1: unblock and observe

- Trace the actual event path from `◆バトルの発生` into `X◆戦闘処理`.
- Identify what command `key: 124` represents and add parser/runtime support if needed.
- Remove the hard block only once the event path can run under test with controlled input.
- Add a regression test that boots an existing battle and asserts the runtime reaches battle initialization instead of showing the "未対応" message.

### Phase 2: initial battle rendering

- Validate picture/message/window commands used by `189` and `190`.
- Fix any battle-specific layout issues the same way menu/shop regressions were handled: test first, then patch runtime behavior.
- Add a regression test that verifies battle UI elements appear in sane positions with sane scale.

### Phase 3: one-turn vertical slice

- Drive one deterministic player command path through:
  - command selection
  - action setup
  - damage/message handling
  - turn end
- Add a regression test that proves the battle loop advances and exits through existing cleanup.

### Phase 4: expand coverage

- enemy actions
- victory / defeat branches
- repeated turns
- status effects and battle-only command variations

## Risks / likely gaps

- parser support for currently unknown command `124`
- battle-only picture/effect behavior not yet exercised by menu/shop work
- additional battle-specific variable indirection or DB conventions
- heavy dependence on existing common-event side effects, which means the safest path is test-driven vertical slices instead of speculative rewrites

## Test strategy

- Prefer high-level runtime tests in `web/tests/string-runtime.test.mjs`
- Reuse real repository data and existing battle common events
- Stub only user input / waits / message display where necessary
- Add focused assertions for:
  - battle entry no longer short-circuiting
  - battle UI geometry
  - one-turn progression
  - cleanup / return to field state

## Immediate next tasks

1. Identify command `124` in battle event `188`
2. Add a first failing battle-entry regression
3. Make the main battle common event run under test
4. Fix the first runtime/parser gap revealed by that regression
