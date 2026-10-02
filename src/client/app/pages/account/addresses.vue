<script setup lang="ts">
import type { Address, AddressInput } from '~/types/profile'

const api = useProfileApi()
const { data: addresses, status, error: loadError, refresh } = useAsyncData('account-addresses', () => api<Address[]>('/addresses'), { default: () => [] })

// null: not editing; 'new': the add form; otherwise the address being edited.
const editing = ref<Address | 'new' | null>(null)
const busy = ref(false)
const error = ref('')
const fieldErrors = ref<Record<string, string>>({})

function edit(target: Address | 'new' | null) {
  editing.value = target
  error.value = ''
  fieldErrors.value = {}
}

async function save(input: AddressInput) {
  busy.value = true
  error.value = ''
  fieldErrors.value = {}
  try {
    const target = editing.value
    if (target === 'new') await api('/addresses', { method: 'POST', body: input })
    else if (target) await api(`/addresses/${target.id}`, { method: 'PUT', body: input })
    await refresh()
    edit(null)
  } catch (e) {
    const { fields, general } = problemMessages(e)
    fieldErrors.value = fields
    error.value = general
  } finally {
    busy.value = false
  }
}

async function makeDefault(address: Address) {
  await api(`/addresses/${address.id}/default`, { method: 'POST' })
  await refresh()
}

async function remove(address: Address) {
  await api(`/addresses/${address.id}`, { method: 'DELETE' })
  if (editing.value !== 'new' && editing.value?.id === address.id) edit(null)
  await refresh()
}

useHead({ title: 'Delivery addresses · Nimbus Coffee Co.' })
</script>

<template>
  <div>
    <div class="section-head">
      <h1>Delivery addresses</h1>
      <button v-if="!editing" class="btn btn-accent" @click="edit('new')">Add address</button>
    </div>

    <div v-if="editing" class="panel">
      <h3>{{ editing === 'new' ? 'New address' : `Edit ${editing.label}` }}</h3>
      <AddressForm
        :key="editing === 'new' ? 'new' : editing.id"
        :address="editing === 'new' ? null : editing"
        :busy="busy" :errors="fieldErrors" :error="error"
        @submit="save" @cancel="edit(null)" />
    </div>

    <p v-if="status === 'pending' && !addresses.length" class="notice">Loading…</p>
    <p v-else-if="loadError" class="notice">Couldn't load your addresses. Is the Profile service running?</p>
    <div v-else-if="!addresses.length && !editing" class="notice">
      <p>You haven't saved any delivery addresses yet.</p>
      <button class="btn btn-ghost" @click="edit('new')">Add your first address</button>
    </div>

    <ul v-else class="list">
      <li v-for="a in addresses" :key="a.id" class="panel address" :class="{ default: a.isDefault }">
        <div>
          <div class="title">
            <strong>{{ a.label }}</strong>
            <span v-if="a.isDefault" class="chip">Default</span>
          </div>
          <address>
            {{ a.recipientName }}<br>
            {{ a.line1 }}<template v-if="a.line2">, {{ a.line2 }}</template><br>
            {{ a.suburb }} {{ a.state }} {{ a.postcode }}<br>
            {{ a.country }}
            <template v-if="a.phone"><br>{{ a.phone }}</template>
          </address>
        </div>
        <div class="actions">
          <button class="link-btn" @click="edit(a)">Edit</button>
          <button v-if="!a.isDefault" class="link-btn" @click="makeDefault(a)">Make default</button>
          <button class="link-btn danger" @click="remove(a)">Remove</button>
        </div>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.panel + .list, .panel + .notice { margin-top: 24px; }
.panel h3 { margin-bottom: 16px; }
.list { list-style: none; margin: 0; padding: 0; display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 16px; }
.address { display: flex; flex-direction: column; justify-content: space-between; gap: 16px; padding: 22px 24px; }
.address.default { border-color: var(--accent); }
.title { display: flex; align-items: center; gap: 10px; margin-bottom: 8px; }
address { font-style: normal; color: var(--muted); font-size: .94rem; }
.actions { display: flex; gap: 16px; }
</style>
