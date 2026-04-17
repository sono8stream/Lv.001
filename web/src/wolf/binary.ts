const shiftJisDecoder = new TextDecoder('shift_jis')

export function toAssetUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`
  return encodeURI(normalized)
}

export async function loadBinary(path: string): Promise<Uint8Array> {
  const response = await fetch(toAssetUrl(path))
  if (!response.ok) {
    throw new Error(`Failed to load binary asset: ${path}`)
  }

  return new Uint8Array(await response.arrayBuffer())
}

export function clamp(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, value))
}

export class WolfBinaryReader {
  private readonly view: DataView
  private readonly bytes: Uint8Array

  constructor(bytes: Uint8Array) {
    this.bytes = bytes
    this.view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength)
  }

  readByte(offset: number): { value: number; nextOffset: number } {
    return { value: this.bytes[offset] ?? 0, nextOffset: offset + 1 }
  }

  readInt(offset: number): { value: number; nextOffset: number } {
    return { value: this.view.getInt32(offset, true), nextOffset: offset + 4 }
  }

  readBytes(offset: number, size: number): { value: Uint8Array; nextOffset: number } {
    return { value: this.bytes.slice(offset, offset + size), nextOffset: offset + size }
  }

  readString(offset: number): { value: string; nextOffset: number } {
    const { value: byteLength, nextOffset: afterLength } = this.readInt(offset)
    const bytes = this.bytes.slice(afterLength, afterLength + byteLength)
    let value = shiftJisDecoder.decode(bytes)
    if (value.endsWith('\0')) {
      value = value.slice(0, -1)
    }
    return { value, nextOffset: afterLength + byteLength }
  }
}

export async function loadImage(path: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const image = new Image()
    image.decoding = 'async'
    image.onload = () => resolve(image)
    image.onerror = () => reject(new Error(`Failed to load image: ${path}`))
    image.src = toAssetUrl(path)
  })
}

export function createCanvas(width: number, height: number): HTMLCanvasElement {
  const canvas = document.createElement('canvas')
  canvas.width = width
  canvas.height = height
  return canvas
}
