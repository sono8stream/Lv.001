import { resolve } from 'node:path'
import { defineConfig } from 'vite'

export default defineConfig({
  publicDir: resolve(__dirname, '../Assets/StreamingAssets'),
})
