<script setup lang="ts">
import type { Order } from '~/types/orders'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const api = useOrdersApi()
const id = String(route.params.id)
const justPlaced = computed(() => route.query.placed === '1')

const { data: order, status, error } = useAsyncData(`order-${id}`, () => api<Order>(`/orders/${id}`))

const placedAt = computed(() => order.value
  ? new Date(order.value.placedAt).toLocaleString('en-AU', { dateStyle: 'medium', timeStyle: 'short' })
  : '')

useHead({ title: `Order ${orderRef(id)} · Nimbus Coffee Co.` })
</script>

<template>
  <div class="container narrow">
    <p v-if="status === 'pending'" class="notice">Loading…</p>

    <div v-else-if="error || !order" class="notice">
      <h2>We couldn't find that order</h2>
      <NuxtLink to="/account/orders" class="btn">Your orders</NuxtLink>
    </div>

    <template v-else>
      <div class="head">
        <div class="eyebrow">Order {{ orderRef(order.id) }} · {{ placedAt }}</div>
        <h1>{{ justPlaced ? 'Thanks for your order!' : `Order ${orderRef(order.id)}` }}</h1>
        <p v-if="justPlaced" class="lead">We've received it and it's waiting to be processed.</p>
        <OrderStatus :status="order.status" />
      </div>

      <div class="panel">
        <ul class="items">
          <li v-for="l in order.lines" :key="l.sku">
            <NuxtLink :to="`/products/${l.sku}`">{{ l.name }}</NuxtLink>
            <span class="qty">{{ l.quantity }} × {{ formatMoney(l.unitPrice, order.currency) }}</span>
            <strong>{{ formatMoney(l.lineTotal, order.currency) }}</strong>
          </li>
        </ul>
        <dl class="totals">
          <div><dt>Subtotal</dt><dd>{{ formatMoney(order.subtotal, order.currency) }}</dd></div>
          <div><dt>Shipping</dt><dd>{{ order.shipping ? formatMoney(order.shipping, order.currency) : 'Free' }}</dd></div>
          <div class="total"><dt>Total</dt><dd>{{ formatMoney(order.total, order.currency) }}</dd></div>
        </dl>
      </div>

      <div class="details">
        <div class="panel">
          <h3>Delivering to</h3>
          <address>
            {{ order.deliveryAddress.recipientName }}<br>
            {{ order.deliveryAddress.line1 }}<template v-if="order.deliveryAddress.line2">, {{ order.deliveryAddress.line2 }}</template><br>
            {{ order.deliveryAddress.suburb }} {{ order.deliveryAddress.state }} {{ order.deliveryAddress.postcode }}<br>
            {{ order.deliveryAddress.country }}
          </address>
        </div>
        <div class="panel">
          <h3>Paying with</h3>
          <p>{{ order.paymentCard.brand }} •••• {{ order.paymentCard.last4 }}</p>
        </div>
      </div>

      <div class="form-actions">
        <NuxtLink to="/shop" class="btn">Keep shopping</NuxtLink>
        <NuxtLink to="/account/orders" class="btn btn-ghost">All your orders</NuxtLink>
      </div>
    </template>
  </div>
</template>

<style scoped>
.narrow { max-width: 760px; }
.head { padding: 40px 0 24px; }
.head h1 { margin-top: 6px; }
.lead { font-size: 1.1rem; color: var(--muted); }

.items { list-style: none; margin: 0; padding: 0; }
.items li { display: grid; grid-template-columns: 1fr auto 100px; gap: 16px; padding: 10px 0; border-bottom: 1px solid var(--line); }
.items a { font-weight: 600; text-decoration: none; }
.items a:hover { color: var(--accent-dark); }
.qty { color: var(--muted); }
.items strong { text-align: right; font-variant-numeric: tabular-nums; }
.totals { margin: 14px 0 0 auto; max-width: 280px; display: grid; gap: 8px; }
.totals div { display: flex; justify-content: space-between; }
.totals dt { color: var(--muted); }
.totals dd { margin: 0; font-variant-numeric: tabular-nums; }
.total { border-top: 1px solid var(--line); padding-top: 10px; font-weight: 600; font-size: 1.1rem; }
.total dt { color: var(--ink); }

.details { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin: 24px 0; }
.details h3 { margin-bottom: 8px; }
.details address, .details p { font-style: normal; margin: 0; }
@media (max-width: 640px) { .details { grid-template-columns: 1fr; } }
</style>
