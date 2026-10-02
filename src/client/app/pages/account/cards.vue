<script setup lang="ts">
import type { Card, CardInput, TestCard } from '~/types/profile'

const api = useProfileApi()
const { account } = useAuth()
const { data: cards, status, error: loadError, refresh } = useAsyncData('account-cards', () => api<Card[]>('/cards'), { default: () => [] })
const { data: testCards } = useAsyncData('test-cards', () => api<TestCard[]>('/cards/test-numbers'), { default: () => [] })

const thisYear = new Date().getFullYear()
const years = Array.from({ length: 12 }, (_, i) => thisYear + i)
const months = Array.from({ length: 12 }, (_, i) => i + 1)

const adding = ref(false)
const busy = ref(false)
const error = ref('')
const fieldErrors = ref<Record<string, string>>({})
const form = reactive<CardInput>(blank())

function blank(): CardInput {
  return {
    cardholderName: account.value ? `${account.value.firstName} ${account.value.lastName}` : '',
    number: '',
    expiryMonth: 12,
    expiryYear: thisYear + 3,
    cvc: '',
    isDefault: false,
  }
}

function startAdding() {
  Object.assign(form, blank())
  error.value = ''
  fieldErrors.value = {}
  adding.value = true
}

const groupDigits = (digits: string) => digits.replace(/\D/g, '').replace(/(\d{4})(?=\d)/g, '$1 ')

function onNumberInput(event: Event) {
  form.number = groupDigits((event.target as HTMLInputElement).value).slice(0, 23)
}

function fillTestCard(card: TestCard) {
  form.number = groupDigits(card.number)
  form.cvc = card.brand === 'Amex' ? '1234' : '123'
}

async function save() {
  busy.value = true
  error.value = ''
  fieldErrors.value = {}
  try {
    await api('/cards', { method: 'POST', body: form })
    await refresh()
    adding.value = false
  } catch (e) {
    const { fields, general } = problemMessages(e)
    fieldErrors.value = fields
    error.value = general
  } finally {
    busy.value = false
  }
}

async function makeDefault(card: Card) {
  await api(`/cards/${card.id}/default`, { method: 'POST' })
  await refresh()
}

async function remove(card: Card) {
  await api(`/cards/${card.id}`, { method: 'DELETE' })
  await refresh()
}

const expiry = (c: Pick<Card, 'expiryMonth' | 'expiryYear'>) =>
  `${String(c.expiryMonth).padStart(2, '0')}/${String(c.expiryYear).slice(-2)}`

useHead({ title: 'Payment cards · Nimbus Coffee Co.' })
</script>

<template>
  <div>
    <div class="section-head">
      <h1>Payment cards</h1>
      <button v-if="!adding" class="btn btn-accent" @click="startAdding">Add card</button>
    </div>

    <p class="fake">
      <strong>Pretend money only.</strong> This is a demo shop: it only accepts well-known test card numbers,
      keeps just the last four digits, and never charges anything.
    </p>

    <div v-if="adding" class="panel add">
      <div class="preview" :data-brand="testCards.find(t => t.number === form.number.replace(/\D/g, ''))?.brand">
        <span class="chip-art" />
        <span class="num">{{ form.number || '•••• •••• •••• ••••' }}</span>
        <span class="row">
          <span>{{ form.cardholderName || 'Cardholder' }}</span>
          <span>{{ expiry(form) }}</span>
        </span>
      </div>

      <form class="form" @submit.prevent="save">
        <p v-if="error" class="form-error">{{ error }}</p>

        <div class="tests">
          <span>Test cards</span>
          <button v-for="t in testCards" :key="t.number" type="button" class="chip" @click="fillTestCard(t)">
            {{ t.brand }} ···{{ t.number.slice(-4) }}
          </button>
        </div>

        <label class="field" :class="{ invalid: fieldErrors.cardholderName }">
          <span>Name on card</span>
          <input v-model="form.cardholderName" autocomplete="off" required>
          <span v-if="fieldErrors.cardholderName" class="error">{{ fieldErrors.cardholderName }}</span>
        </label>
        <label class="field" :class="{ invalid: fieldErrors.number }">
          <span>Card number</span>
          <input :value="form.number" inputmode="numeric" autocomplete="off" placeholder="4242 4242 4242 4242" required @input="onNumberInput">
          <span v-if="fieldErrors.number" class="error">{{ fieldErrors.number }}</span>
        </label>
        <div class="form-row">
          <label class="field" :class="{ invalid: fieldErrors.expiryMonth }">
            <span>Month</span>
            <select v-model.number="form.expiryMonth">
              <option v-for="m in months" :key="m" :value="m">{{ String(m).padStart(2, '0') }}</option>
            </select>
          </label>
          <label class="field" :class="{ invalid: fieldErrors.expiryYear }">
            <span>Year</span>
            <select v-model.number="form.expiryYear">
              <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
            </select>
            <span v-if="fieldErrors.expiryYear" class="error">{{ fieldErrors.expiryYear }}</span>
          </label>
          <label class="field" :class="{ invalid: fieldErrors.cvc }">
            <span>CVC</span>
            <input v-model="form.cvc" inputmode="numeric" maxlength="4" autocomplete="off" placeholder="123" required>
            <span v-if="fieldErrors.cvc" class="error">{{ fieldErrors.cvc }}</span>
          </label>
        </div>
        <label class="check">
          <input v-model="form.isDefault" type="checkbox">
          Use as my default card
        </label>
        <div class="form-actions">
          <button class="btn btn-accent" :disabled="busy">{{ busy ? 'Saving…' : 'Save card' }}</button>
          <button type="button" class="btn btn-ghost" @click="adding = false">Cancel</button>
        </div>
      </form>
    </div>

    <p v-if="status === 'pending' && !cards.length" class="notice">Loading…</p>
    <p v-else-if="loadError" class="notice">Couldn't load your cards. Is the Profile service running?</p>
    <div v-else-if="!cards.length && !adding" class="notice">
      <p>No saved cards yet.</p>
      <button class="btn btn-ghost" @click="startAdding">Add a test card</button>
    </div>

    <ul v-else class="list">
      <li v-for="c in cards" :key="c.id" class="saved">
        <div class="preview small" :data-brand="c.brand">
          <span class="row top">
            <strong>{{ c.brand }}</strong>
            <span v-if="c.isDefault" class="badge">Default</span>
          </span>
          <span class="num">•••• {{ c.last4 }}</span>
          <span class="row">
            <span>{{ c.cardholderName }}</span>
            <span>{{ expiry(c) }}</span>
          </span>
        </div>
        <div class="actions">
          <button v-if="!c.isDefault" class="link-btn" @click="makeDefault(c)">Make default</button>
          <button class="link-btn danger" @click="remove(c)">Remove</button>
        </div>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.fake { background: var(--kraft); color: var(--bean); border-radius: 10px; padding: .8em 1.1em; font-size: .92rem; }

.add { display: grid; grid-template-columns: 300px 1fr; gap: 32px; align-items: start; margin-bottom: 24px; }
@media (max-width: 900px) { .add { grid-template-columns: 1fr; } }

.tests { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; font-size: .85rem; color: var(--muted); }
.tests .chip { border: 0; cursor: pointer; text-transform: none; font: 500 .8rem var(--sans); }
.tests .chip:hover { background: var(--accent); color: #fff; }

/* A card-shaped preview, coloured by brand. */
.preview {
  --card-bg: linear-gradient(135deg, var(--bean), var(--ink));
  aspect-ratio: 1.586;
  border-radius: 16px;
  padding: 22px;
  background: var(--card-bg);
  color: #fff;
  display: flex; flex-direction: column; justify-content: space-between;
  box-shadow: 0 12px 28px rgba(42, 33, 27, .22);
  font-size: .85rem;
}
.preview[data-brand='Visa'] { --card-bg: linear-gradient(135deg, #274b8f, #16294f); }
.preview[data-brand='Mastercard'] { --card-bg: linear-gradient(135deg, #d46a8a, #7a2e46); }
.preview[data-brand='Amex'] { --card-bg: linear-gradient(135deg, #3f8f8a, #1d4b48); }
.preview[data-brand='Discover'] { --card-bg: linear-gradient(135deg, #d9893b, #8a4a14); }
.preview .num { font: 500 1.2rem ui-monospace, 'SF Mono', Menlo, monospace; letter-spacing: .08em; }
.preview .row { display: flex; justify-content: space-between; gap: 12px; text-transform: uppercase; letter-spacing: .06em; }
.chip-art { width: 38px; height: 28px; border-radius: 6px; background: linear-gradient(135deg, #f3d98b, #c9a24a); }

.list { list-style: none; margin: 0; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 24px; }
.saved { display: grid; gap: 12px; }
.preview.small { padding: 18px 20px; }
.top strong { font: 700 1.05rem var(--serif); text-transform: none; letter-spacing: 0; }
.badge { background: rgba(255, 255, 255, .2); border-radius: 999px; padding: .1em .7em; font-size: .72rem; }
.actions { display: flex; gap: 16px; padding-left: 4px; }
</style>
