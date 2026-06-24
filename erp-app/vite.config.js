import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 61104,
        proxy: {
            '/api': {
                target: process.env.VITE_API_URL || 'http://localhost:5254',
                changeOrigin: true,
            },
        },
    },
});