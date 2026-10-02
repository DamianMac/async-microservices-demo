<script setup lang="ts">
const { products, bySku, status } = useCatalog()

const hero = computed(() => bySku.value.get('SO-PAN-BOQ') ?? products.value[0])
const newArrivals = computed(() =>
  products.value.filter(p => p.category === 'single-origin' && p.coffee?.roast === 'light').slice(0, 4))
const blends = computed(() => products.value.filter(p => p.category === 'blend').slice(0, 4))
const equipment = computed(() => products.value.filter(p => p.category === 'equipment').slice(0, 4))
const countFor = (slug: string) => products.value.filter(p => p.category === slug).length
</script>

<template>
  <div>
    <section class="hero">
      <div class="container hero-grid">
        <div>
          <div class="eyebrow">Specialty coffee · Roasted weekly</div>
          <h1>Coffee with its head in the clouds.</h1>
          <p class="lede">Single origins from the world's best growing regions, blends built for your morning, and the kit to brew them well.</p>
          <div class="actions">
            <NuxtLink to="/shop?category=single-origin" class="btn btn-accent">Shop single origin</NuxtLink>
            <NuxtLink to="/shop" class="btn btn-ghost">Browse everything</NuxtLink>
          </div>
        </div>
        <NuxtLink v-if="hero" :to="`/products/${hero.sku}`" class="hero-product">
          <img v-if="hero.imageUrl" :src="hero.imageUrl" :alt="hero.name">
          <div class="hero-tag">
            <span class="eyebrow">Featured</span>
            <strong>{{ hero.name }}</strong>
            <span>{{ formatPrice(hero) }}</span>
          </div>
        </NuxtLink>
      </div>
    </section>

    <section class="section container">
      <div class="cats">
        <NuxtLink v-for="c in categories" :key="c.slug" :to="`/shop?category=${c.slug}`" class="cat">
          <h3>{{ c.label }}</h3>
          <p>{{ c.blurb }}</p>
          <span class="count">{{ countFor(c.slug) }} products →</span>
        </NuxtLink>
      </div>
    </section>

    <p v-if="status === 'pending' && !products.length" class="notice">Loading the catalog…</p>
    <p v-else-if="status === 'error'" class="notice">We couldn't reach the catalog. Is the Catalog service running?</p>

    <template v-else>
      <section class="section container">
        <div class="section-head">
          <h2>Bright &amp; fruity single origins</h2>
          <NuxtLink to="/shop?category=single-origin&roast=light">See all light roasts →</NuxtLink>
        </div>
        <div class="product-grid">
          <ProductCard v-for="p in newArrivals" :key="p.sku" :product="p" />
        </div>
      </section>

      <section class="section container">
        <div class="section-head">
          <h2>House blends</h2>
          <NuxtLink to="/shop?category=blend">All blends →</NuxtLink>
        </div>
        <div class="product-grid">
          <ProductCard v-for="p in blends" :key="p.sku" :product="p" />
        </div>
      </section>

      <section class="banner">
        <div class="container">
          <h2>Not sure where to start?</h2>
          <p>Search by what you like to taste: try <NuxtLink to="/search?q=chocolate">chocolate</NuxtLink>, <NuxtLink to="/search?q=berry OR blueberry">berry</NuxtLink> or <NuxtLink to="/search?q=espresso">espresso</NuxtLink>.</p>
        </div>
      </section>

      <section class="section container">
        <div class="section-head">
          <h2>Brew better at home</h2>
          <NuxtLink to="/shop?category=equipment">All equipment →</NuxtLink>
        </div>
        <div class="product-grid">
          <ProductCard v-for="p in equipment" :key="p.sku" :product="p" />
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.hero { padding: 56px 0 24px; }
.hero-grid { display: grid; grid-template-columns: 1.1fr .9fr; gap: 48px; align-items: center; }
.lede { font-size: 1.15rem; color: var(--muted); max-width: 34em; }
.actions { display: flex; gap: 12px; flex-wrap: wrap; margin-top: 24px; }

.hero-product { position: relative; display: block; border-radius: 24px; overflow: hidden; background: var(--kraft); text-decoration: none; }
.hero-product img { width: 100%; aspect-ratio: 4 / 4.2; object-fit: cover; }
.hero-tag {
  position: absolute; left: 20px; bottom: 20px;
  display: flex; flex-direction: column; gap: 2px;
  background: var(--card); padding: 12px 18px; border-radius: 12px;
  box-shadow: 0 8px 24px rgba(42, 33, 27, .12);
}
.hero-tag strong { font-family: var(--serif); font-size: 1.1rem; }

.cats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
.cat {
  padding: 24px; border-radius: var(--radius); background: var(--card);
  border: 1px solid var(--line); text-decoration: none;
  transition: transform .15s, border-color .15s;
}
.cat:hover { transform: translateY(-3px); border-color: var(--accent); }
.cat p { color: var(--muted); font-size: .92rem; }
.count { font-weight: 600; font-size: .88rem; color: var(--accent-dark); }

.banner { background: var(--accent); color: #fff; padding: 48px 0; text-align: center; margin: 24px 0; }
.banner p { font-size: 1.1rem; margin: 0; }
.banner a { color: #fff; font-weight: 600; }

@media (max-width: 860px) {
  .hero-grid { grid-template-columns: 1fr; }
  .cats { grid-template-columns: repeat(2, 1fr); }
}
</style>
