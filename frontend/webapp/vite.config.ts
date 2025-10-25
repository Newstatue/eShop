import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import mkcert from 'vite-plugin-mkcert';
import tailwindcss from "@tailwindcss/vite"
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');

    return {
        plugins: [react(), mkcert(), tailwindcss()],
        server: {
            port: parseInt(env.VITE_PORT),
        },
        build: {
            outDir: 'dist',
            rollupOptions: {
                input: './index.html'
            }
        },
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            },
        },
    }
})
