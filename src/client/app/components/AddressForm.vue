<script setup lang="ts">
import type { Address, AddressInput } from '~/types/profile'

const props = defineProps<{ address?: Address | null, busy?: boolean, errors?: Record<string, string>, error?: string }>()
const emit = defineEmits<{ submit: [AddressInput], cancel: [] }>()

const states = ['ACT', 'NSW', 'NT', 'QLD', 'SA', 'TAS', 'VIC', 'WA']

const { account } = useAuth()
const form = reactive<AddressInput>({
  label: props.address?.label ?? 'Home',
  recipientName: props.address?.recipientName ?? (account.value ? `${account.value.firstName} ${account.value.lastName}` : ''),
  line1: props.address?.line1 ?? '',
  line2: props.address?.line2 ?? '',
  suburb: props.address?.suburb ?? '',
  state: props.address?.state ?? 'VIC',
  postcode: props.address?.postcode ?? '',
  country: props.address?.country ?? 'Australia',
  phone: props.address?.phone ?? '',
  isDefault: props.address?.isDefault ?? false,
})

const e = computed(() => props.errors ?? {})
</script>

<template>
  <form class="form" @submit.prevent="emit('submit', { ...form })">
    <p v-if="error" class="form-error">{{ error }}</p>
    <div class="form-row">
      <label class="field" :class="{ invalid: e.label }">
        <span>Label</span>
        <input v-model="form.label" placeholder="Home, Work…" required>
        <span v-if="e.label" class="error">{{ e.label }}</span>
      </label>
      <label class="field" :class="{ invalid: e.recipientName }">
        <span>Recipient</span>
        <input v-model="form.recipientName" autocomplete="name" required>
        <span v-if="e.recipientName" class="error">{{ e.recipientName }}</span>
      </label>
    </div>
    <label class="field" :class="{ invalid: e.line1 }">
      <span>Street address</span>
      <input v-model="form.line1" autocomplete="address-line1" required>
      <span v-if="e.line1" class="error">{{ e.line1 }}</span>
    </label>
    <label class="field">
      <span>Apartment, unit, etc. <small>(optional)</small></span>
      <input v-model="form.line2" autocomplete="address-line2">
    </label>
    <div class="form-row">
      <label class="field" :class="{ invalid: e.suburb }">
        <span>Suburb</span>
        <input v-model="form.suburb" autocomplete="address-level2" required>
        <span v-if="e.suburb" class="error">{{ e.suburb }}</span>
      </label>
      <label class="field" :class="{ invalid: e.state }">
        <span>State</span>
        <select v-model="form.state" autocomplete="address-level1">
          <option v-for="s in states" :key="s">{{ s }}</option>
        </select>
      </label>
      <label class="field" :class="{ invalid: e.postcode }">
        <span>Postcode</span>
        <input v-model="form.postcode" inputmode="numeric" maxlength="4" autocomplete="postal-code" required>
        <span v-if="e.postcode" class="error">{{ e.postcode }}</span>
      </label>
    </div>
    <label class="field" :class="{ invalid: e.phone }">
      <span>Phone <small>(optional, for the courier)</small></span>
      <input v-model="form.phone" type="tel" autocomplete="tel">
      <span v-if="e.phone" class="error">{{ e.phone }}</span>
    </label>
    <label class="check">
      <input v-model="form.isDefault" type="checkbox">
      Use as my default delivery address
    </label>
    <div class="form-actions">
      <button class="btn btn-accent" :disabled="busy">{{ busy ? 'Saving…' : address ? 'Save address' : 'Add address' }}</button>
      <button type="button" class="btn btn-ghost" @click="emit('cancel')">Cancel</button>
    </div>
  </form>
</template>
