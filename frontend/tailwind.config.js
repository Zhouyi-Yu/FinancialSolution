/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{vue,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                finance: {
                    bg: '#0B0E1B',
                    card: '#161B30',
                    text: '#FFFFFF',
                    muted: '#8A94A6',
                    cyan: '#00D1FF',
                    yellow: '#FFD600',
                    green: '#00EDA0',
                    red: '#F9575E'
                }
            }
        },
    },
    plugins: [],
}
