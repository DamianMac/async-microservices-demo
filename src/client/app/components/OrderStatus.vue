<script setup lang="ts">
defineProps<{ status: string }>()
</script>

<template>
  <span class="status" :data-status="status">{{ status }}</span>
</template>

<style scoped>
.status {
  display: inline-block; padding: .2em .8em; border-radius: 999px;
  font-size: .78rem; font-weight: 600; letter-spacing: .06em; text-transform: uppercase;
  background: var(--kraft); color: var(--bean);
}
.status[data-status='Pending'] { background: #fdf1d8; color: #8a5a00; }
.status[data-status='Paid'] { background: #e3f1e0; color: #2f6b2a; }
.status[data-status='Completed'] { background: #2f6b2a; color: #f2faf0; }
</style>
