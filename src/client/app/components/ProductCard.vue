<script setup lang="ts">
import type { Product } from '~/types/catalog'

defineProps<{ product: Product }>()
</script>

<template>
  <NuxtLink :to="`/products/${product.sku}`" class="card">
    <div class="image">
      <img v-if="product.imageUrl" :src="product.imageUrl" :alt="product.name" loading="lazy">
      <StockBadge :level="product.stockLevel" class="badge" />
    </div>
    <div class="body">
      <div class="meta">
        <span>{{ product.coffee ? product.coffee.origin : categoryLabel(product.category) }}</span>
        <RoastMeter v-if="product.coffee" :roast="product.coffee.roast" />
      </div>
      <h3>{{ product.name }}</h3>
      <p v-if="product.coffee?.tastingNotes.length" class="notes">{{ product.coffee.tastingNotes.join(' · ') }}</p>
      <div class="price">
        {{ formatPrice(product) }}
        <small v-if="product.coffee?.weightGrams">/ {{ product.coffee.weightGrams >= 1000 ? `${product.coffee.weightGrams / 1000}kg` : `${product.coffee.weightGrams}g` }}</small>
      </div>
    </div>
  </NuxtLink>
</template>

<style scoped>
.card { display: flex; flex-direction: column; text-decoration: none; color: inherit; }
.image {
  position: relative;
  aspect-ratio: 3 / 4; border-radius: var(--radius); overflow: hidden;
  background: var(--kraft);
}
.image img { width: 100%; height: 100%; object-fit: cover; transition: transform .35s ease; }
.badge { position: absolute; top: 12px; left: 12px; }
.card:hover .image img { transform: scale(1.04); }
.body { padding: 14px 2px 0; }
.meta { display: flex; justify-content: space-between; align-items: center; font-size: .78rem; letter-spacing: .08em; text-transform: uppercase; color: var(--muted); }
h3 { margin: 6px 0 4px; }
.card:hover h3 { color: var(--accent-dark); }
.notes { font-size: .88rem; font-style: italic; color: var(--muted); margin: 0 0 8px; }
.price { font-weight: 600; }
.price small { font-weight: 400; color: var(--muted); }
</style>
