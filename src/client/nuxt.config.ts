// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  // Run as a client-side SPA; the browser calls the APIs directly
  ssr: false,

  css: ['~/assets/css/main.css'],

  app: {
    head: {
      title: 'Nimbus Coffee Co.',
      htmlAttrs: { lang: 'en' },
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,500;9..144,700&family=Inter:wght@400;500;600&display=swap' }
      ]
    }
  },

  devServer: {
    // IPv4 so Caddy (in Docker, via host.docker.internal) can reach it
    host: '127.0.0.1',
    port: 3000
  },

  runtimeConfig: {
    public: {
      apiBase: 'http://api.localtest.me'
    }
  },

  vite: {
    server: {
      // Allow requests proxied through Caddy
      allowedHosts: ['shop.localtest.me']
    }
  }
})
