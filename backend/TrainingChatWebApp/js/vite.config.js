import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
    build:{
        rollupOptions:{
            input:{
                app: "./index_test.html"
            },
            output:{
                dir: "../wwwroot-nocache/"
            }
        }
    },
    plugins: [svelte()],
})
