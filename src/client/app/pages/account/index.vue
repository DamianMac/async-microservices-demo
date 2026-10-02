<script setup lang="ts">
import type { Account, Address, Card } from '~/types/profile'

const api = useProfileApi()
const { account, setAccount } = useAuth()

const form = reactive({ firstName: account.value?.firstName ?? '', lastName: account.value?.lastName ?? '' })
const busy = ref(false)
const saved = ref(false)
const error = ref('')
const fieldErrors = ref<Record<string, string>>({})

// A glance at what's saved, for the summary cards.
const { data: addresses } = useAsyncData('account-addresses', () => api<Address[]>('/addresses'), { default: () => [] })
const { data: cards } = useAsyncData('account-cards', () => api<Card[]>('/cards'), { default: () => [] })
const defaultAddress = computed(() => addresses.value.find(a => a.isDefault))
const defaultCard = computed(() => cards.value.find(c => c.isDefault))

async function save() {
  busy.value = true
  saved.value = false
  error.value = ''
  fieldErrors.value = {}
  try {
    setAccount(await api<Account>('/account', { method: 'PUT', body: form }))
    saved.value = true
  } catch (e) {
    const { fields, general } = problemMessages(e)
    fieldErrors.value = fields
    error.value = general
  } finally {
    busy.value = false
  }
}

const memberSince = computed(() => account.value
  ? new Date(account.value.createdAt).toLocaleDateString('en-AU', { month: 'long', year: 'numeric' })
  : '')

useHead({ title: 'Your account · Nimbus Coffee Co.' })
</script>

<template>
  <div>
    <h1>Details</h1>
    <p class="muted">{{ account?.email }} · Member since {{ memberSince }}</p>

    <div class="summary">
      <NuxtLink to="/account/addresses" class="panel tile">
        <div class="eyebrow">Default delivery</div>
        <template v-if="defaultAddress">
          <strong>{{ defaultAddress.label }}</strong>
          <span>{{ defaultAddress.line1 }}, {{ defaultAddress.suburb }} {{ defaultAddress.state }}</span>
        </template>
        <span v-else>No addresses yet. Add one →</span>
      </NuxtLink>
      <NuxtLink to="/account/cards" class="panel tile">
        <div class="eyebrow">Default card</div>
        <template v-if="defaultCard">
          <strong>{{ defaultCard.brand }} •••• {{ defaultCard.last4 }}</strong>
          <span>Expires {{ String(defaultCard.expiryMonth).padStart(2, '0') }}/{{ defaultCard.expiryYear }}</span>
        </template>
        <span v-else>No cards yet. Add one →</span>
      </NuxtLink>
    </div>

    <form class="panel form" @submit.prevent="save">
      <h3>Your name</h3>
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
      <div class="form-actions">
        <button class="btn" :disabled="busy">{{ busy ? 'Saving…' : 'Save' }}</button>
        <span v-if="saved" class="muted">Saved.</span>
      </div>
    </form>
  </div>
</template>

<style scoped>
.muted { color: var(--muted); }
.summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; margin: 24px 0; }
.tile { display: grid; gap: 4px; text-decoration: none; padding: 20px 24px; transition: border-color .15s; }
.tile:hover { border-color: var(--accent); }
.tile span { color: var(--muted); font-size: .92rem; }
form h3 { margin: 0; }
</style>
