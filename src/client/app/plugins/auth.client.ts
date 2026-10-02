import type { Account } from '~/types/profile'

// Refreshes the stored account on load, and quietly signs out a token that no longer works.
export default defineNuxtPlugin(() => {
  const { token, setAccount, logout } = useAuth()
  if (!token.value) return

  const { public: { apiBase } } = useRuntimeConfig()
  $fetch<Account>(`${apiBase}/profile/account`, { headers: { Authorization: `Bearer ${token.value}` } })
    .then(setAccount)
    .catch((error) => { if (error?.response?.status === 401) logout() })
})
