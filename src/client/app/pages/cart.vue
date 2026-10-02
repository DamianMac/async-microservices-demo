<script setup lang="ts">
const { status } = useCatalog()
const { lines, items, outOfStock, subtotal, shipping, total, setQuantity, remove } = useCart()

const toFreeShipping = computed(() => Math.max(FREE_SHIPPING_FROM - subtotal.value, 0))

useHead({ title: 'Cart · Nimbus Coffee Co.' })
</script>

<template>
  <div class="container">
    <h1 class="title">Your cart</h1>

    <p v-if="status === 'pending' && lines.length && !items.length" class="notice">Loading…</p>

    <div v-else-if="!items.length" class="notice">
      <p>Your cart is empty.</p>
      <NuxtLink to="/shop" class="btn">Browse the shop</NuxtLink>
    </div>

    <div v-else class="layout">
      <ul class="lines">
        <li v-for="item in items" :key="item.sku" class="line">
          <NuxtLink :to="`/products/${item.sku}`" class="thumb">
            <img v-if="item.product.imageUrl" :src="item.product.imageUrl" :alt="item.product.name">
          </NuxtLink>
          <div class="what">
            <NuxtLink :to="`/products/${item.sku}`" class="name">{{ item.product.name }}</NuxtLink>
            <span class="unit">{{ formatPrice(item.product) }} each</span>
            <StockBadge :level="item.product.stockLevel" />
            <button class="link-btn danger" @click="remove(item.sku)">Remove</button>
          </div>
          <QuantityStepper :model-value="item.quantity" :min="0" @update:model-value="setQuantity(item.sku, $event)" />
          <strong class="amount">{{ formatMoney(item.lineTotal, item.product.currency) }}</strong>
        </li>
      </ul>

      <aside class="panel summary">
        <h3>Summary</h3>
        <dl>
          <div><dt>Subtotal</dt><dd>{{ formatMoney(subtotal) }}</dd></div>
          <div><dt>Shipping</dt><dd>{{ shipping ? formatMoney(shipping) : 'Free' }}</dd></div>
          <div class="total"><dt>Total</dt><dd>{{ formatMoney(total) }}</dd></div>
        </dl>
        <p v-if="toFreeShipping" class="hint">Spend {{ formatMoney(toFreeShipping) }} more for free shipping.</p>
        <p v-if="outOfStock.length" class="hint">Remove the out of stock items to check out.</p>
        <span v-if="outOfStock.length" class="btn wide disabled">Checkout</span>
        <NuxtLink v-else to="/checkout" class="btn btn-accent wide">Checkout</NuxtLink>
        <NuxtLink to="/shop" class="keep">Keep shopping</NuxtLink>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.title { padding-top: 40px; }
.layout { display: grid; grid-template-columns: 1fr 340px; gap: 40px; align-items: start; }
.lines { list-style: none; margin: 0; padding: 0; border-top: 1px solid var(--line); }
.line {
  display: grid; grid-template-columns: 88px 1fr auto 100px; gap: 20px; align-items: center;
  padding: 18px 0; border-bottom: 1px solid var(--line);
}
.thumb { border-radius: 10px; overflow: hidden; background: var(--kraft); aspect-ratio: 3 / 4; }
.thumb img { width: 100%; height: 100%; object-fit: cover; }
.what { display: grid; gap: 4px; justify-items: start; }
.name { font: 700 1.05rem var(--serif); text-decoration: none; }
.name:hover { color: var(--accent-dark); }
.unit { color: var(--muted); font-size: .88rem; }
.amount { text-align: right; font-variant-numeric: tabular-nums; }

.summary { position: sticky; top: 96px; display: grid; gap: 14px; }
.summary h3 { margin: 0; }
dl { margin: 0; display: grid; gap: 8px; }
dl div { display: flex; justify-content: space-between; }
dt { color: var(--muted); }
dd { margin: 0; font-variant-numeric: tabular-nums; }
.total { border-top: 1px solid var(--line); padding-top: 10px; font-weight: 600; font-size: 1.1rem; }
.total dt { color: var(--ink); }
.hint { margin: 0; font-size: .88rem; color: var(--bean); background: var(--kraft); border-radius: 10px; padding: .6em .9em; }
.wide { text-align: center; }
.disabled { opacity: .45; cursor: not-allowed; }
.keep { justify-self: center; font-size: .9rem; font-weight: 600; color: var(--accent-dark); text-decoration: none; }

@media (max-width: 860px) {
  .layout { grid-template-columns: 1fr; }
  .summary { position: static; }
  .line { grid-template-columns: 64px 1fr auto; }
  .amount { grid-column: 2 / -1; }
}
</style>
