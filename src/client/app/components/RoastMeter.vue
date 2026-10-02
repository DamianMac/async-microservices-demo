<script setup lang="ts">
import { roasts } from '~/composables/useCatalog'

const props = defineProps<{ roast: string }>()
const level = computed(() => roasts.indexOf(props.roast as typeof roasts[number]))
</script>

<template>
  <span class="roast" :title="`${roast} roast`">
    <span v-for="(r, i) in roasts" :key="r" class="dot" :class="{ on: i <= level }" />
  </span>
</template>

<style scoped>
.roast { display: inline-flex; gap: 4px; vertical-align: middle; }
.dot { width: 9px; height: 9px; border-radius: 50%; background: var(--line); }
.dot.on { background: var(--bean); }
</style>
