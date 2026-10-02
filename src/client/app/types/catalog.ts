// Shapes returned by the Catalog and Search services.

export interface CoffeeDetails {
  origin: string
  region: string
  process: string
  roast: string
  weightGrams: number
  tastingNotes: string[]
}

// From the warehouse's LowStockWarning and OutOfStock events, via Catalog.
export type StockLevel = 'InStock' | 'Low' | 'Out'

export interface Product {
  sku: string
  name: string
  category: string
  description: string
  price: number
  currency: string
  imageUrl: string | null
  coffee: CoffeeDetails | null
  updatedAt: string
  stockLevel: StockLevel
}

export interface SearchResult {
  sku: string
  name: string
  category: string
  rank: number
}
