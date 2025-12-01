import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  // file:// で読み込むために相対パスを指定
  base: './',
  // ビルド成果物を 'dist' に出力
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
})