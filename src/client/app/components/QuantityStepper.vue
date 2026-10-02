<script setup lang="ts">
const quantity = defineModel<number>({ required: true })
withDefaults(defineProps<{ min?: number, max?: number }>(), { min: 1, max: 99 })
</script>

<template>
  <div class="stepper">
    <button type="button" aria-label="Decrease quantity" :disabled="quantity <= min" @click="quantity--">−</button>
    <span aria-live="polite">{{ quantity }}</span>
    <button type="button" aria-label="Increase quantity" :disabled="quantity >= max" @click="quantity++">+</button>
  </div>
</template>

<style scoped>
.stepper {
  display: inline-flex; align-items: center;
  border: 1.5px solid var(--line); border-radius: 999px; background: var(--card);
}
button {
  width: 36px; height: 36px; border: 0; background: none; cursor: pointer;
  font: 600 1.1rem var(--sans); color: var(--ink); border-radius: 50%;
}
button:hover:not(:disabled) { background: var(--kraft); }
button:disabled { color: var(--line); cursor: default; }
span { min-width: 28px; text-align: center; font-weight: 600; font-variant-numeric: tabular-nums; }
</style>
