<script setup lang="ts">
const route = useRoute()
const { register } = useAuth()

const form = reactive({ firstName: '', lastName: '', email: '', password: '' })
const busy = ref(false)
const error = ref('')
const fieldErrors = ref<Record<string, string>>({})

async function submit() {
  busy.value = true
  error.value = ''
  fieldErrors.value = {}
  try {
    await register(form)
    await navigateTo(String(route.query.redirect ?? '/account'))
  } catch (e) {
    const { fields, general } = problemMessages(e)
    fieldErrors.value = fields
    error.value = general
  } finally {
    busy.value = false
  }
}

useHead({ title: 'Create an account · Nimbus Coffee Co.' })
</script>

<template>
  <div class="container auth">
    <div class="panel">
      <div class="eyebrow">Join the club</div>
      <h1>Create an account</h1>
      <p class="lede">Save your delivery addresses and cards for a quicker checkout.</p>

      <form class="form" @submit.prevent="submit">
        <p v-if="error" class="form-error">{{ error }}</p>
        <div class="form-row">
          <label class="field" :class="{ invalid: fieldErrors.firstName }">
            <span>First name</span>
            <input v-model="form.firstName" autocomplete="given-name" required>
            <span v-if="fieldErrors.firstName" class="error">{{ fieldErrors.firstName }}</span>
          </label>
          <label class="field" :class="{ invalid: fieldErrors.lastName }">
            <span>Last name</span>
            <input v-model="form.lastName" autocomplete="family-name" required>
            <span v-if="fieldErrors.lastName" class="error">{{ fieldErrors.lastName }}</span>
          </label>
        </div>
        <label class="field" :class="{ invalid: fieldErrors.email }">
          <span>Email</span>
          <input v-model="form.email" type="email" autocomplete="email" required>
          <span v-if="fieldErrors.email" class="error">{{ fieldErrors.email }}</span>
        </label>
        <label class="field" :class="{ invalid: fieldErrors.password }">
          <span>Password</span>
          <input v-model="form.password" type="password" autocomplete="new-password" minlength="8" required>
          <small v-if="!fieldErrors.password">At least 8 characters.</small>
          <span v-else class="error">{{ fieldErrors.password }}</span>
        </label>
        <div class="form-actions">
          <button class="btn btn-accent" :disabled="busy">{{ busy ? 'Creating account…' : 'Create account' }}</button>
        </div>
      </form>

      <p class="alt">Already have an account? <NuxtLink :to="{ path: '/login', query: route.query }">Sign in</NuxtLink></p>
    </div>
  </div>
</template>

<style scoped>
.auth { max-width: 520px; padding-top: 56px; }
h1 { font-size: 2.2rem; }
.lede { color: var(--muted); }
.alt { margin: 20px 0 0; font-size: .92rem; color: var(--muted); }
.alt a { color: var(--accent-dark); font-weight: 600; }
</style>
