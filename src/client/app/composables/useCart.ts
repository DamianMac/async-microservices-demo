import type { Product } from '~/types/catalog'
import type { CartLine } from '~/types/orders'

const CART_KEY = 'nimbus.cart'
const MAX_QUANTITY = 99

// Matches Orders' Pricing/Shipping, which decides what the order is actually charged.
export const FREE_SHIPPING_FROM = 60
export const shippingFor = (subtotal: number) => subtotal >= FREE_SHIPPING_FROM ? 0 : 9.95

// Short, readable order reference from the order's GUID.
export const orderRef = (id: string) => id.slice(0, 8).toUpperCase()

function load(): CartLine[] {
  try { return JSON.parse(localStorage.getItem(CART_KEY) ?? '[]') as CartLine[] } catch { return [] }
}

// The cart only lives in the browser (localStorage) until checkout, when Orders prices it.
// Lines hold SKUs and quantities; names and prices come from the catalog.
export function useCart() {
  const lines = useState<CartLine[]>('cart', load)
  const { bySku } = useCatalog()

  function save() {
    try { localStorage.setItem(CART_KEY, JSON.stringify(lines.value)) } catch { /* private mode etc. */ }
  }

  // Lines joined to the catalog. A SKU that's left the catalog is dropped from the view.
  const items = computed(() => lines.value.flatMap((line) => {
    const product = bySku.value.get(line.sku)
    return product ? [{ ...line, product, lineTotal: product.price * line.quantity }] : []
  }))

  // Checkout is blocked while any of these are in the cart.
  const outOfStock = computed(() => items.value.filter(i => i.product.stockLevel === 'Out'))

  const count = computed(() => lines.value.reduce((n, l) => n + l.quantity, 0))
  const subtotal = computed(() => items.value.reduce((n, i) => n + i.lineTotal, 0))
  const shipping = computed(() => items.value.length ? shippingFor(subtotal.value) : 0)
  const total = computed(() => subtotal.value + shipping.value)

  function add(product: Pick<Product, 'sku'>, quantity = 1) {
    const line = lines.value.find(l => l.sku === product.sku)
    if (line) line.quantity = Math.min(line.quantity + quantity, MAX_QUANTITY)
    else lines.value.push({ sku: product.sku, quantity: Math.min(quantity, MAX_QUANTITY) })
    save()
  }

  function setQuantity(sku: string, quantity: number) {
    if (quantity < 1) return remove(sku)
    const line = lines.value.find(l => l.sku === sku)
    if (line) line.quantity = Math.min(quantity, MAX_QUANTITY)
    save()
  }

  function remove(sku: string) {
    lines.value = lines.value.filter(l => l.sku !== sku)
    save()
  }

  function clear() {
    lines.value = []
    save()
  }

  return { lines, items, outOfStock, count, subtotal, shipping, total, add, setQuantity, remove, clear }
}
