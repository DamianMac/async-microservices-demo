<script setup lang="ts">
const route = useRoute()
const query = ref('')

// Keep the box in step with the search page's query.
watch(() => route.query.q, q => { query.value = route.path === '/search' ? String(q ?? '') : '' }, { immediate: true })

function submit() {
  const q = query.value.trim()
  if (q) navigateTo({ path: '/search', query: { q } })
}
</script>

<template>
  <form class="search" role="search" @submit.prevent="submit">
    <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="7" /><path d="m20 20-3.5-3.5" /></svg>
    <input v-model="query" type="search" placeholder="Search coffee, origins, notes…" aria-label="Search products">
  </form>
</template>

<style scoped>
.search { position: relative; }
svg { position: absolute; left: 14px; top: 50%; width: 18px; transform: translateY(-50%); fill: none; stroke: var(--muted); stroke-width: 2; stroke-linecap: round; }
input {
  width: 100%; padding: 10px 16px 10px 40px;
  border: 1.5px solid var(--line); border-radius: 999px;
  background: var(--card); font: inherit; font-size: .92rem; color: var(--ink);
}
input:focus { outline: none; border-color: var(--accent); }
</style>
