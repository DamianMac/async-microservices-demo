<script setup lang="ts">
import type { Address, Card } from '~/types/profile'
import type { Order } from '~/types/orders'

definePageMeta({ middleware: 'auth' })

const profile = useProfileApi()
const orders = useOrdersApi()
const { status: catalogStatus } = useCatalog()
const { lines, items, outOfStock, subtotal, shipping, total, clear } = useCart()

const { data: addresses, status: addressStatus, error: addressError } =
  useAsyncData('checkout-addresses', () => profile<Address[]>('/addresses'), { default: () => [] })
const { data: cards, status: cardStatus, error: cardError } =
  useAsyncData('checkout-cards', () => profile<Card[]>('/cards'), { default: () => [] })

const addressId = ref<number | null>(null)
const cardId = ref<number | null>(null)

// Start with the customer's defaults.
watchEffect(() => {
  if (addressId.value === null) addressId.value = addresses.value.find(a => a.isDefault)?.id ?? addresses.value[0]?.id ?? null
  if (cardId.value === null) cardId.value = cards.value.find(c => c.isDefault)?.id ?? cards.value[0]?.id ?? null
})

const address = computed(() => addresses.value.find(a => a.id === addressId.value))
const card = computed(() => cards.value.find(c => c.id === cardId.value))

// Sent with the order, so a retry of the same checkout (double click, timeout) doesn't place it twice.
// crypto.randomUUID needs a secure context, which http://shop.localtest.me isn't.
function newOrderId() {
  const b = crypto.getRandomValues(new Uint8Array(16))
  b[6] = (b[6]! & 0x0f) | 0x40
  b[8] = (b[8]! & 0x3f) | 0x80
  const hex = Array.from(b, x => x.toString(16).padStart(2, '0')).join('')
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`
}
const orderId = newOrderId()

const busy = ref(false)
const error = ref('')

async function placeOrder() {
  if (!address.value || !card.value) return
  busy.value = true
  error.value = ''
  try {
    const a = address.value
    const order = await orders<Order>('/orders', {
      method: 'POST',
      body: {
        id: orderId,
        items: items.value.map(i => ({ sku: i.sku, quantity: i.quantity })),
        deliveryAddress: {
          recipientName: a.recipientName, line1: a.line1, line2: a.line2, suburb: a.suburb,
          state: a.state, postcode: a.postcode, country: a.country, phone: a.phone,
        },
        paymentCard: { id: card.value.id, brand: card.value.brand, last4: card.value.last4 },
      },
    })
    await navigateTo({ path: `/orders/${order.id}`, query: { placed: '1' } })
    clear()
  } catch (e) {
    const { fields, general } = problemMessages(e, 'Orders')
    error.value = fields.items ?? general
  } finally {
    busy.value = false
  }
}

const expiry = (c: Card) => `${String(c.expiryMonth).padStart(2, '0')}/${String(c.expiryYear).slice(-2)}`

useHead({ title: 'Checkout · Nimbus Coffee Co.' })
</script>

<template>
  <div class="container">
    <h1 class="title">Checkout</h1>

    <p v-if="catalogStatus === 'pending' && lines.length && !items.length" class="notice">Loading…</p>

    <div v-else-if="!items.length" class="notice">
      <p>Your cart is empty.</p>
      <NuxtLink to="/shop" class="btn">Browse the shop</NuxtLink>
    </div>

    <div v-else class="layout">
      <div class="steps">
        <section class="panel">
          <div class="step-head">
            <h3><span class="num">1</span> Deliver to</h3>
            <NuxtLink to="/account/addresses">Manage addresses</NuxtLink>
          </div>
          <p v-if="addressStatus === 'pending' && !addresses.length" class="muted">Loading…</p>
          <p v-else-if="addressError" class="form-error">Couldn't load your addresses. Is the Profile service running?</p>
          <p v-else-if="!addresses.length" class="muted">
            You haven't saved a delivery address yet. <NuxtLink to="/account/addresses">Add one</NuxtLink>, then come back.
          </p>
          <div v-else class="options">
            <label v-for="a in addresses" :key="a.id" class="option" :class="{ selected: a.id === addressId }">
              <input v-model="addressId" type="radio" name="address" :value="a.id">
              <span>
                <strong>{{ a.label }}</strong> <span v-if="a.isDefault" class="chip">Default</span><br>
                {{ a.recipientName }}, {{ a.line1 }}<template v-if="a.line2">, {{ a.line2 }}</template>,
                {{ a.suburb }} {{ a.state }} {{ a.postcode }}
              </span>
            </label>
          </div>
        </section>

        <section class="panel">
          <div class="step-head">
            <h3><span class="num">2</span> Pay with</h3>
            <NuxtLink to="/account/cards">Manage cards</NuxtLink>
          </div>
          <p v-if="cardStatus === 'pending' && !cards.length" class="muted">Loading…</p>
          <p v-else-if="cardError" class="form-error">Couldn't load your cards. Is the Profile service running?</p>
          <p v-else-if="!cards.length" class="muted">
            You haven't saved a card yet. <NuxtLink to="/account/cards">Add a test card</NuxtLink>, then come back.
          </p>
          <div v-else class="options">
            <label v-for="c in cards" :key="c.id" class="option" :class="{ selected: c.id === cardId }">
              <input v-model="cardId" type="radio" name="card" :value="c.id">
              <span>
                <strong>{{ c.brand }} •••• {{ c.last4 }}</strong> <span v-if="c.isDefault" class="chip">Default</span><br>
                {{ c.cardholderName }} · expires {{ expiry(c) }}
              </span>
            </label>
          </div>
        </section>
      </div>

      <aside class="panel summary">
        <h3>Your order</h3>
        <ul class="items">
          <li v-for="i in items" :key="i.sku">
            <span>{{ i.quantity }} × {{ i.product.name }}</span>
            <span>{{ formatMoney(i.lineTotal, i.product.currency) }}</span>
          </li>
        </ul>
        <dl>
          <div><dt>Subtotal</dt><dd>{{ formatMoney(subtotal) }}</dd></div>
          <div><dt>Shipping</dt><dd>{{ shipping ? formatMoney(shipping) : 'Free' }}</dd></div>
          <div class="total"><dt>Total</dt><dd>{{ formatMoney(total) }}</dd></div>
        </dl>
        <p v-if="outOfStock.length" class="form-error">
          {{ outOfStock.map(i => i.product.name).join(', ') }} {{ outOfStock.length === 1 ? 'is' : 'are' }} out of stock.
          <NuxtLink to="/cart">Update your cart</NuxtLink>
        </p>
        <p v-if="error" class="form-error">{{ error }}</p>
        <button class="btn btn-accent" :disabled="busy || !address || !card || outOfStock.length > 0" @click="placeOrder">
          {{ busy ? 'Placing order…' : 'Checkout now' }}
        </button>
        <NuxtLink to="/cart" class="back">Back to cart</NuxtLink>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.title { padding-top: 40px; }
.layout { display: grid; grid-template-columns: 1fr 360px; gap: 40px; align-items: start; }
.steps { display: grid; gap: 24px; }
.step-head { display: flex; justify-content: space-between; align-items: baseline; gap: 12px; margin-bottom: 14px; }
.step-head h3 { margin: 0; display: flex; align-items: center; gap: 10px; }
.step-head a, .muted a { font-weight: 600; font-size: .9rem; color: var(--accent-dark); text-decoration: none; }
.num {
  width: 26px; height: 26px; border-radius: 50%; display: inline-grid; place-items: center;
  background: var(--ink); color: var(--paper); font: 600 .85rem var(--sans);
}
.muted { color: var(--muted); margin: 0; }

.options { display: grid; gap: 10px; }
.option {
  display: flex; gap: 12px; align-items: flex-start; cursor: pointer;
  border: 1.5px solid var(--line); border-radius: 12px; padding: 14px 16px; font-size: .92rem;
}
.option:hover { border-color: var(--muted); }
.option.selected { border-color: var(--accent); background: rgba(212, 106, 138, .06); }
.option input { accent-color: var(--accent); margin-top: 3px; }
.option .chip { font-size: .68rem; }

.summary { position: sticky; top: 96px; display: grid; gap: 14px; }
.summary h3 { margin: 0; }
.items { list-style: none; margin: 0; padding: 0 0 12px; display: grid; gap: 8px; font-size: .92rem; border-bottom: 1px solid var(--line); }
.items li { display: flex; justify-content: space-between; gap: 12px; }
dl { margin: 0; display: grid; gap: 8px; }
dl div { display: flex; justify-content: space-between; }
dt { color: var(--muted); }
dd { margin: 0; font-variant-numeric: tabular-nums; }
.total { border-top: 1px solid var(--line); padding-top: 10px; font-weight: 600; font-size: 1.1rem; }
.total dt { color: var(--ink); }
.back { justify-self: center; font-size: .9rem; font-weight: 600; color: var(--accent-dark); text-decoration: none; }

@media (max-width: 860px) {
  .layout { grid-template-columns: 1fr; }
  .summary { position: static; }
}
</style>
