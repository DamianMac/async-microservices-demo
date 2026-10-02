import type { Account, AuthResponse } from '~/types/profile'

const TOKEN_KEY = 'nimbus.token'
const ACCOUNT_KEY = 'nimbus.account'

function read(key: string) {
  try { return localStorage.getItem(key) } catch { return null }
}

function write(key: string, value: string | null) {
  try {
    if (value === null) localStorage.removeItem(key)
    else localStorage.setItem(key, value)
  } catch { /* private mode etc.; stays signed in for this tab only */ }
}

// The Profile service issues a JWT on register or login. The shop is an SPA, so the token
// and the account summary live in localStorage and every Profile call sends the token.
export function useAuth() {
  const token = useState<string | null>('auth-token', () => read(TOKEN_KEY))
  const account = useState<Account | null>('auth-account', () => {
    const json = read(ACCOUNT_KEY)
    return json ? JSON.parse(json) as Account : null
  })

  const signedIn = computed(() => !!token.value)

  function setSession(session: AuthResponse | null) {
    token.value = session?.token ?? null
    account.value = session?.account ?? null
    write(TOKEN_KEY, token.value)
    write(ACCOUNT_KEY, account.value ? JSON.stringify(account.value) : null)
  }

  function setAccount(value: Account) {
    account.value = value
    write(ACCOUNT_KEY, JSON.stringify(value))
  }

  const { public: { apiBase } } = useRuntimeConfig()

  async function login(email: string, password: string) {
    setSession(await $fetch<AuthResponse>(`${apiBase}/profile/auth/login`, { method: 'POST', body: { email, password } }))
  }

  async function register(body: { email: string, password: string, firstName: string, lastName: string }) {
    setSession(await $fetch<AuthResponse>(`${apiBase}/profile/auth/register`, { method: 'POST', body }))
  }

  function logout() {
    setSession(null)
  }

  return { token, account, signedIn, login, register, logout, setAccount }
}

// $fetch for a service's endpoints for the signed-in customer (e.g. 'profile', 'orders'). A 401
// means the token has expired or the account is gone (e.g. after ./run.sh reset), so sign out
// and go to the login page.
export function useServiceApi(service: string) {
  const { public: { apiBase } } = useRuntimeConfig()
  const { token, logout } = useAuth()
  const route = useRoute()

  return $fetch.create({
    baseURL: `${apiBase}/${service}`,
    onRequest({ options }) {
      if (token.value) options.headers.set('Authorization', `Bearer ${token.value}`)
    },
    onResponseError({ response }) {
      if (response.status === 401) {
        logout()
        navigateTo({ path: '/login', query: { redirect: route.fullPath } })
      }
    }
  })
}

export const useProfileApi = () => useServiceApi('profile')
export const useOrdersApi = () => useServiceApi('orders')

// Turns a service error (ProblemDetails, with per-field errors for validation) into
// field messages keyed by camelCase field name, plus a general message.
export function problemMessages(error: unknown, service = 'Profile') {
  const data = (error as { data?: { title?: string, errors?: Record<string, string[]> } })?.data
  const fields: Record<string, string> = {}
  for (const [key, messages] of Object.entries(data?.errors ?? {}))
    fields[key.charAt(0).toLowerCase() + key.slice(1)] = messages[0] ?? ''

  const general = data?.errors
    ? 'Please check the highlighted fields.'
    : data?.title ?? `Something went wrong. Is the ${service} service running?`

  return { fields, general }
}
