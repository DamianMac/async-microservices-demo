<script setup lang="ts">
import type { Product } from '~/types/catalog'

const route = useRoute()
const router = useRouter()
const { products, status } = useCatalog()

// Filters live in the query string, so a filtered view can be linked and shared.
const category = computed(() => String(route.query.category ?? ''))
const roast = computed(() => String(route.query.roast ?? ''))
const origin = computed(() => String(route.query.origin ?? ''))
const sort = computed(() => String(route.query.sort ?? 'featured'))

function setFilter(name: string, value: string) {
  const query = { ...route.query, [name]: value || undefined }
  // Roast and origin only apply to coffee.
  if (name === 'category' && value === 'equipment') {
    delete query.roast
    delete query.origin
  }
  router.replace({ query })
}

const inCategory = computed(() =>
  category.value ? products.value.filter(p => p.category === category.value) : products.value)

const origins = computed(() =>
  [...new Set(inCategory.value.map(p => p.coffee?.origin).filter((o): o is string => !!o && o !== 'Blend'))].sort())

const sorters: Record<string, (a: Product, b: Product) => number> = {
  featured: () => 0,
  'price-asc': (a, b) => a.price - b.price,
  'price-desc': (a, b) => b.price - a.price,
  name: (a, b) => a.name.localeCompare(b.name)
}

const results = computed(() =>
  inCategory.value
    .filter(p => !roast.value || p.coffee?.roast === roast.value)
    .filter(p => !origin.value || p.coffee?.origin === origin.value)
    .slice()
    .sort(sorters[sort.value] ?? sorters.featured))

const heading = computed(() => category.value ? categoryLabel(category.value) : 'All products')
const blurb = computed(() => categories.find(c => c.slug === category.value)?.blurb ?? 'Everything we roast, and everything you need to brew it.')
const isCoffee = computed(() => category.value !== 'equipment')

useHead({ title: () => `${heading.value} · Nimbus Coffee Co.` })
</script>

<template>
  <div class="container shop">
    <header class="shop-head">
      <div class="eyebrow">Shop</div>
      <h1>{{ heading }}</h1>
      <p>{{ blurb }}</p>
    </header>

    <div class="layout">
      <aside class="filters">
        <div class="group">
          <h4>Category</h4>
          <button :class="{ active: !category }" @click="setFilter('category', '')">All</button>
          <button v-for="c in categories" :key="c.slug" :class="{ active: category === c.slug }" @click="setFilter('category', c.slug)">
            {{ c.label }}
          </button>
        </div>

        <div v-if="isCoffee" class="group">
          <h4>Roast</h4>
          <button :class="{ active: !roast }" @click="setFilter('roast', '')">Any</button>
          <button v-for="r in roasts" :key="r" :class="{ active: roast === r }" @click="setFilter('roast', r)">
            <RoastMeter :roast="r" /> <span class="cap">{{ r }}</span>
          </button>
        </div>

        <div v-if="isCoffee && origins.length" class="group">
          <h4>Origin</h4>
          <select :value="origin" @change="setFilter('origin', ($event.target as HTMLSelectElement).value)">
            <option value="">Any origin</option>
            <option v-for="o in origins" :key="o" :value="o">{{ o }}</option>
          </select>
        </div>
      </aside>

      <section>
        <div class="toolbar">
          <span>{{ results.length }} {{ results.length === 1 ? 'product' : 'products' }}</span>
          <label>
            Sort
            <select :value="sort" @change="setFilter('sort', ($event.target as HTMLSelectElement).value)">
              <option value="featured">Featured</option>
              <option value="price-asc">Price: low to high</option>
              <option value="price-desc">Price: high to low</option>
              <option value="name">Name</option>
            </select>
          </label>
        </div>

        <p v-if="status === 'pending' && !products.length" class="notice">Loading the catalog…</p>
        <p v-else-if="status === 'error'" class="notice">We couldn't reach the catalog. Is the Catalog service running?</p>
        <div v-else-if="!results.length" class="notice">
          <p>Nothing matches those filters.</p>
          <NuxtLink :to="{ query: { category: category || undefined } }" class="btn btn-ghost">Clear filters</NuxtLink>
        </div>
        <div v-else class="product-grid">
          <ProductCard v-for="p in results" :key="p.sku" :product="p" />
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.shop-head { padding: 40px 0 28px; border-bottom: 1px solid var(--line); margin-bottom: 28px; }
.shop-head p { color: var(--muted); margin: 0; }

.layout { display: grid; grid-template-columns: 220px 1fr; gap: 40px; align-items: start; }

.filters { position: sticky; top: 96px; display: flex; flex-direction: column; gap: 28px; }
.group { display: flex; flex-direction: column; align-items: flex-start; gap: 4px; }
.group h4 { margin: 0 0 6px; font-size: .75rem; letter-spacing: .16em; text-transform: uppercase; color: var(--muted); }
.group button {
  display: flex; align-items: center; gap: 8px;
  background: none; border: 0; padding: 5px 10px; margin-left: -10px; border-radius: 8px;
  font: inherit; font-size: .95rem; color: var(--ink); cursor: pointer; text-align: left;
}
.group button:hover { background: var(--kraft); }
.group button.active { background: var(--ink); color: var(--paper); }
.group button.active :deep(.dot.on) { background: var(--paper); }
.group button.active :deep(.dot) { background: rgba(245, 240, 232, .3); }
.cap { text-transform: capitalize; }

select {
  font: inherit; font-size: .92rem; padding: 7px 10px;
  border: 1.5px solid var(--line); border-radius: 8px; background: var(--card); color: var(--ink);
}
.group select { width: 100%; }

.toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; color: var(--muted); font-size: .92rem; }
.toolbar label { display: flex; align-items: center; gap: 8px; }

@media (max-width: 760px) {
  .layout { grid-template-columns: 1fr; gap: 20px; }
  .filters { position: static; flex-direction: row; flex-wrap: wrap; gap: 20px; }
  .group { flex-direction: row; flex-wrap: wrap; align-items: center; }
  .group h4 { width: 100%; }
  .group button { margin-left: 0; }
}
</style>
