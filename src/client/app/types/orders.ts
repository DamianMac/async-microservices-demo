// Shapes returned by the Orders service.

export interface CartLine {
  sku: string
  quantity: number
}

export interface OrderSummary {
  id: string
  status: string
  itemCount: number
  total: number
  currency: string
  placedAt: string
}

export interface OrderLine {
  sku: string
  name: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface Order {
  id: string
  status: string
  lines: OrderLine[]
  deliveryAddress: {
    recipientName: string
    line1: string
    line2: string | null
    suburb: string
    state: string
    postcode: string
    country: string
    phone: string | null
  }
  paymentCard: { brand: string, last4: string }
  subtotal: number
  shipping: number
  total: number
  currency: string
  placedAt: string
}
