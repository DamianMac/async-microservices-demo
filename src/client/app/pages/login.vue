<script setup lang="ts">
const route = useRoute()
const { login } = useAuth()

const email = ref('')
const password = ref('')
const busy = ref(false)
const error = ref('')

const redirect = computed(() => String(route.query.redirect ?? '/account'))

async function submit() {
  busy.value = true
  error.value = ''
  try {
    await login(email.value, password.value)
    await navigateTo(redirect.value)
  } catch (e) {
    error.value = problemMessages(e).general
  } finally {
    busy.value = false
  }
}

useHead({ title: 'Sign in · Nimbus Coffee Co.' })
</script>

<template>
  <div class="container auth">
    <div class="panel">
      <div class="eyebrow">Welcome back</div>
      <h1>Sign in</h1>

      <form class="form" @submit.prevent="submit">
        <p v-if="error" class="form-error">{{ error }}</p>
        <label class="field">
          <span>Email</span>
          <input v-model="email" type="email" autocomplete="email" required>
        </label>
        <label class="field">
          <span>Password</span>
          <input v-model="password" type="password" autocomplete="current-password" required>
        </label>
        <div class="form-actions">
          <button class="btn btn-accent" :disabled="busy">{{ busy ? 'Signing in…' : 'Sign in' }}</button>
        </div>
      </form>

      <p class="alt">New here? <NuxtLink :to="{ path: '/register', query: route.query }">Create an account</NuxtLink></p>
    </div>
  </div>
</template>

<style scoped>
.auth { max-width: 460px; padding-top: 56px; }
h1 { font-size: 2.2rem; }
.alt { margin: 20px 0 0; font-size: .92rem; color: var(--muted); }
.alt a { color: var(--accent-dark); font-weight: 600; }
</style>
