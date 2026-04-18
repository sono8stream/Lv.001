import '@fontsource/noto-sans-jp/400.css'
import '@fontsource/noto-sans-jp/700.css'
import './style.css'
import { WolfRuntime } from './wolf/runtime'

const app = document.querySelector<HTMLDivElement>('#app')

if (app === null) {
  throw new Error('App root was not found.')
}

app.innerHTML = `
  <div class="shell">
    <aside class="sidebar">
      <h1>WebWolf</h1>
      <p class="summary">WOLF RPG エディタの StreamingAssets をそのまま読む TypeScript ランタイム。</p>
      <dl class="legend">
        <div><dt>移動</dt><dd>矢印 / WASD</dd></div>
        <div><dt>決定</dt><dd>Z / Enter / Space</dd></div>
        <div><dt>キャンセル</dt><dd>X / Escape</dd></div>
        <div><dt>スマホ</dt><dd>画面下の仮想キー</dd></div>
      </dl>
      <pre id="statusPanel" class="panel">booting...</pre>
      <pre id="debugPanel" class="panel debug-panel">debug: booting...</pre>
    </aside>
    <main class="game-area">
      <div class="viewport-frame">
        <canvas id="gameCanvas" width="960" height="640" aria-label="WebWolf game screen"></canvas>
        <div id="pictureLayer" class="picture-layer"></div>
        <div id="messageBox" class="message-box hidden">
          <div class="message-inner">
            <div id="messageText" class="message-text"></div>
            <div class="message-hint">Z / Enter / Space / A</div>
          </div>
        </div>
        <div id="choiceBox" class="choice-box hidden">
          <div class="choice-inner">
            <div id="choiceTitle" class="choice-title">選択してください</div>
            <div id="choiceList" class="choice-list"></div>
          </div>
        </div>
        <div id="errorBox" class="error-box hidden"></div>
      </div>
      <div class="mobile-controls" aria-label="virtual controls">
        <div class="dpad" role="group" aria-label="movement controls">
          <div class="dpad-grid">
            <div class="dpad-spacer"></div>
            <button type="button" class="virtual-button dpad-up" data-virtual-key="ArrowUp" aria-label="Move up">UP</button>
            <div class="dpad-spacer"></div>
            <button type="button" class="virtual-button dpad-left" data-virtual-key="ArrowLeft" aria-label="Move left">LEFT</button>
            <div class="dpad-spacer dpad-center" aria-hidden="true"></div>
            <button type="button" class="virtual-button dpad-right" data-virtual-key="ArrowRight" aria-label="Move right">RIGHT</button>
            <div class="dpad-spacer"></div>
            <button type="button" class="virtual-button dpad-down" data-virtual-key="ArrowDown" aria-label="Move down">DOWN</button>
            <div class="dpad-spacer"></div>
          </div>
        </div>
        <div class="action-pad" role="group" aria-label="action controls">
          <button type="button" class="virtual-button action-button action-confirm" data-virtual-key="Enter" aria-label="Confirm">A</button>
          <button type="button" class="virtual-button action-button action-cancel" data-virtual-key="Escape" aria-label="Cancel">B</button>
        </div>
      </div>
    </main>
  </div>
`

const runtime = new WolfRuntime({
  canvas: document.querySelector<HTMLCanvasElement>('#gameCanvas')!,
  statusPanel: document.querySelector<HTMLPreElement>('#statusPanel')!,
  debugPanel: document.querySelector<HTMLPreElement>('#debugPanel')!,
  messageBox: document.querySelector<HTMLDivElement>('#messageBox')!,
  messageText: document.querySelector<HTMLDivElement>('#messageText')!,
  choiceBox: document.querySelector<HTMLDivElement>('#choiceBox')!,
  choiceList: document.querySelector<HTMLDivElement>('#choiceList')!,
  choiceTitle: document.querySelector<HTMLDivElement>('#choiceTitle')!,
  pictureLayer: document.querySelector<HTMLDivElement>('#pictureLayer')!,
  errorBox: document.querySelector<HTMLDivElement>('#errorBox')!,
})

void runtime.boot()
