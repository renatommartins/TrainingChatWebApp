import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';
import includePaths from 'rollup-plugin-includepaths';

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
        emptyOutDir: true,
    },
    plugins: [
        svelte(),
        includePaths({
            paths: [
                './',
                '../node_modules/materialize-css/dist/css/'
            ]
        })
    ],
})
