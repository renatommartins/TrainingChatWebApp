import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
    build:{
        rollupOptions:{
            input:{
                login: "./login.html",
                chat: "./chat.html",
                signup: "./signup.html",
            },
            output:{
                dir: "../wwwroot-nocache/"
            }
        },
        sourcemap: true,
    },
    plugins: [svelte()],
})
