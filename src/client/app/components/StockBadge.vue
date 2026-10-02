<script setup lang="ts">
import type { StockLevel } from '~/types/catalog'

defineProps<{ level: StockLevel }>()
</script>

<template>
  <span v-if="level === 'Low'" class="stock-badge low">Low stock</span>
  <span v-else-if="level === 'Out'" class="stock-badge out">Out of stock</span>
</template>
