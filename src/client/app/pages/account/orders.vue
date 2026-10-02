<script setup lang="ts">
import type { OrderSummary } from '~/types/orders'

const api = useOrdersApi()
const { data: orders, status, error } = useAsyncData('account-orders', () => api<OrderSummary[]>('/orders'), { default: () => [] })

const placed = (o: OrderSummary) => new Date(o.placedAt).toLocaleDateString('en-AU', { dateStyle: 'medium' })

useHead({ title: 'Orders · Nimbus Coffee Co.' })
</script>

<template>
  <div>
    <div class="section-head">
      <h1>Orders</h1>
    </div>

    <p v-if="status === 'pending' && !orders.length" class="notice">Loading…</p>
    <p v-else-if="error" class="notice">Couldn't load your orders. Is the Orders service running?</p>
    <div v-else-if="!orders.length" class="notice">
      <p>You haven't placed any orders yet.</p>
      <NuxtLink to="/shop" class="btn btn-ghost">Browse the shop</NuxtLink>
    </div>

    <ul v-else class="list">
      <li v-for="o in orders" :key="o.id">
        <NuxtLink :to="`/orders/${o.id}`" class="panel row">
          <strong class="ref">{{ orderRef(o.id) }}</strong>
          <span class="muted">{{ placed(o) }}</span>
          <span class="muted">{{ o.itemCount }} {{ o.itemCount === 1 ? 'item' : 'items' }}</span>
          <OrderStatus :status="o.status" />
          <strong class="amount">{{ formatMoney(o.total, o.currency) }}</strong>
        </NuxtLink>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.list { list-style: none; margin: 0; padding: 0; display: grid; gap: 12px; }
.row {
  display: grid; grid-template-columns: 120px 1fr 90px 110px 100px; gap: 16px; align-items: center;
  padding: 18px 24px; text-decoration: none; color: inherit;
}
.row:hover { border-color: var(--accent); }
.ref { font-family: ui-monospace, 'SF Mono', Menlo, monospace; letter-spacing: .04em; }
.muted { color: var(--muted); font-size: .92rem; }
.amount { text-align: right; font-variant-numeric: tabular-nums; }
@media (max-width: 700px) {
  .row { grid-template-columns: 1fr auto; }
}
</style>
