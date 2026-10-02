<script setup lang="ts">
import type { Product, SearchResult } from '~/types/catalog'

const route = useRoute()
const { public: { apiBase } } = useRuntimeConfig()
const { bySku } = useCatalog()

const q = computed(() => String(route.query.q ?? '').trim())

// Search returns ranked SKUs; the product details come from the catalog.
const { data: hits, status, error } = useFetch<SearchResult[]>(`${apiBase}/search/products`, {
  query: { q, limit: 50 },
  default: () => [],
})

const results = computed(() =>
  hits.value.map(h => bySku.value.get(h.sku)).filter((p): p is Product => !!p))

const suggestions = ['chocolate', 'fruity light roast', '"cold brew"', 'Ethiopia OR Kenya', 'espresso -decaf', 'grinder']

useHead({ title: () => q.value ? `“${q.value}” · Nimbus Coffee Co.` : 'Search · Nimbus Coffee Co.' })
</script>

<template>
  <div class="container">
    <header class="head">
      <div class="eyebrow">Search</div>
      <h1 v-if="q">Results for “{{ q }}”</h1>
      <h1 v-else>What are you in the mood for?</h1>
      <SearchBox class="big-search" />
      <div class="suggest">
        <span>Try</span>
        <NuxtLink v-for="s in suggestions" :key="s" :to="{ query: { q: s } }" class="chip">{{ s }}</NuxtLink>
      </div>
    </header>

    <template v-if="q">
      <p v-if="status === 'pending'" class="notice">Searching…</p>
      <p v-else-if="error" class="notice">Search is unavailable right now. Is the Search service running?</p>
      <div v-else-if="!results.length" class="notice">
        <p>No products match “{{ q }}”.</p>
        <NuxtLink to="/shop" class="btn btn-ghost">Browse everything instead</NuxtLink>
      </div>
      <template v-else>
        <p class="count">{{ results.length }} {{ results.length === 1 ? 'match' : 'matches' }}, best first</p>
        <div class="product-grid">
          <ProductCard v-for="p in results" :key="p.sku" :product="p" />
        </div>
      </template>
    </template>
  </div>
</template>

<style scoped>
.head { padding: 40px 0 28px; border-bottom: 1px solid var(--line); margin-bottom: 28px; }
.big-search { max-width: 560px; margin: 8px 0 16px; }
.big-search :deep(input) { padding-top: 14px; padding-bottom: 14px; font-size: 1rem; }
.suggest { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; font-size: .88rem; color: var(--muted); }
.suggest .chip { text-decoration: none; text-transform: none; }
.suggest .chip:hover { background: var(--accent); color: #fff; }
.count { color: var(--muted); font-size: .92rem; }
</style>
