import type { Product } from '~/types/catalog'

export const categories = [
  { slug: 'single-origin', label: 'Single Origin', blurb: 'One farm, one region, one story in the cup.' },
  { slug: 'blend', label: 'Blends', blurb: 'Balanced, dependable, built for every day.' },
  { slug: 'decaf', label: 'Decaf', blurb: 'All of the flavour, none of the jitters.' },
  { slug: 'equipment', label: 'Equipment', blurb: 'Grinders, drippers and kit for brewing at home.' }
] as const

export const roasts = ['light', 'medium', 'medium-dark', 'dark'] as const

export function categoryLabel(slug: string) {
  return categories.find(c => c.slug === slug)?.label ?? slug
}

export function formatPrice(product: Pick<Product, 'price' | 'currency'>) {
  return formatMoney(product.price, product.currency)
}

export function formatMoney(amount: number, currency = 'AUD') {
  return new Intl.NumberFormat('en-AU', { style: 'currency', currency: currency || 'AUD' }).format(amount)
}

// The whole catalog is small and served from Catalog's in-memory cache, so the shop
// loads it once and browses, filters and joins search results client-side.
export function useCatalog() {
  const { public: { apiBase } } = useRuntimeConfig()

  const { data, status, error, refresh } = useFetch<Product[]>(`${apiBase}/catalog/products`, {
    key: 'catalog',
    default: () => [],
    dedupe: 'defer'
  })

  const bySku = computed(() => new Map(data.value.map(p => [p.sku, p])))

  return { products: data, bySku, status, error, refresh }
}
