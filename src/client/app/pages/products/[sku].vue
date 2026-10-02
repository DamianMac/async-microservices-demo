<script setup lang="ts">
const route = useRoute()
const { products, bySku, status } = useCatalog()

const product = computed(() => bySku.value.get(String(route.params.sku)))

const related = computed(() => {
  const p = product.value
  if (!p) return []
  return products.value
    .filter(o => o.sku !== p.sku && o.category === p.category)
    .sort((a, b) => Number(b.coffee?.roast === p.coffee?.roast) - Number(a.coffee?.roast === p.coffee?.roast))
    .slice(0, 4)
})

const weight = computed(() => {
  const g = product.value?.coffee?.weightGrams
  return g ? (g >= 1000 ? `${g / 1000}kg` : `${g}g`) : null
})

const { add } = useCart()
const quantity = ref(1)
const added = ref(false)
watch(() => route.params.sku, () => { quantity.value = 1; added.value = false })

const outOfStock = computed(() => product.value?.stockLevel === 'Out')

function addToCart() {
  if (!product.value || outOfStock.value) return
  add(product.value, quantity.value)
  added.value = true
  quantity.value = 1
}

useHead({ title: () => product.value ? `${product.value.name} · Nimbus Coffee Co.` : 'Nimbus Coffee Co.' })
</script>

<template>
  <div class="container">
    <p v-if="status === 'pending' && !products.length" class="notice">Loading…</p>

    <div v-else-if="!product" class="notice">
      <h2>We couldn't find that product</h2>
      <p>It may no longer be in the catalog.</p>
      <NuxtLink to="/shop" class="btn">Back to the shop</NuxtLink>
    </div>

    <template v-else>
      <nav class="crumbs">
        <NuxtLink to="/shop">Shop</NuxtLink> /
        <NuxtLink :to="`/shop?category=${product.category}`">{{ categoryLabel(product.category) }}</NuxtLink> /
        <span>{{ product.name }}</span>
      </nav>

      <div class="detail">
        <div class="image">
          <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name">
        </div>

        <div class="info">
          <div class="eyebrow">{{ product.coffee ? `${product.coffee.origin} · ${product.coffee.region}` : categoryLabel(product.category) }}</div>
          <h1>{{ product.name }}</h1>
          <div class="price">
            {{ formatPrice(product) }}
            <small v-if="weight">/ {{ weight }}</small>
          </div>

          <div v-if="product.coffee?.tastingNotes.length" class="notes">
            <NuxtLink v-for="n in product.coffee.tastingNotes" :key="n" :to="{ path: '/search', query: { q: n } }" class="chip">{{ n }}</NuxtLink>
          </div>

          <p class="desc">{{ product.description }}</p>

          <dl v-if="product.coffee" class="specs">
            <div><dt>Origin</dt><dd>{{ product.coffee.origin }}</dd></div>
            <div><dt>Region</dt><dd>{{ product.coffee.region }}</dd></div>
            <div><dt>Process</dt><dd class="cap">{{ product.coffee.process }}</dd></div>
            <div><dt>Roast</dt><dd><RoastMeter :roast="product.coffee.roast" /> <span class="cap">{{ product.coffee.roast }}</span></dd></div>
            <div v-if="weight"><dt>Weight</dt><dd>{{ weight }}</dd></div>
          </dl>

          <div class="buy">
            <QuantityStepper v-if="!outOfStock" v-model="quantity" />
            <button class="btn btn-accent" :disabled="outOfStock" @click="addToCart">{{ outOfStock ? 'Out of stock' : 'Add to cart' }}</button>
            <StockBadge v-if="!added" :level="product.stockLevel" />
            <span v-if="added">Added. <NuxtLink to="/cart">View cart →</NuxtLink></span>
          </div>

          <p class="sku">SKU {{ product.sku }}</p>
        </div>
      </div>

      <section v-if="related.length" class="section">
        <div class="section-head">
          <h2>You might also like</h2>
          <NuxtLink :to="`/shop?category=${product.category}`">More {{ categoryLabel(product.category).toLowerCase() }} →</NuxtLink>
        </div>
        <div class="product-grid">
          <ProductCard v-for="p in related" :key="p.sku" :product="p" />
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.crumbs { padding: 24px 0; font-size: .88rem; color: var(--muted); }
.crumbs a { text-decoration: none; color: var(--muted); }
.crumbs a:hover { color: var(--ink); }
.crumbs span { color: var(--ink); }

.detail { display: grid; grid-template-columns: 1fr 1fr; gap: 56px; align-items: start; }
.image { border-radius: 24px; overflow: hidden; background: var(--kraft); position: sticky; top: 96px; }
.image img { width: 100%; aspect-ratio: 3 / 4; object-fit: cover; }

.info h1 { margin-top: 8px; }
.price { font-size: 1.5rem; font-weight: 600; margin-bottom: 20px; }
.price small { font-size: 1rem; font-weight: 400; color: var(--muted); }
.notes { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 20px; }
.notes .chip { text-decoration: none; }
.notes .chip:hover { background: var(--accent); color: #fff; }
.desc { font-size: 1.08rem; }

.specs { display: grid; grid-template-columns: 1fr 1fr; gap: 0; margin: 28px 0; border-top: 1px solid var(--line); }
.specs div { padding: 12px 0; border-bottom: 1px solid var(--line); }
.specs dt { font-size: .72rem; letter-spacing: .14em; text-transform: uppercase; color: var(--muted); }
.specs dd { margin: 2px 0 0; font-weight: 500; }
.cap { text-transform: capitalize; }

.buy { display: flex; align-items: center; gap: 16px; margin: 28px 0 12px; color: var(--muted); font-size: .9rem; }
.buy a { font-weight: 600; color: var(--accent-dark); text-decoration: none; }
.sku { font-size: .8rem; color: var(--muted); }

@media (max-width: 800px) {
  .detail { grid-template-columns: 1fr; gap: 28px; }
  .image { position: static; }
}
</style>
