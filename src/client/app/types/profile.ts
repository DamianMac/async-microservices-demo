// Shapes returned by the Profile service.

export interface Account {
  id: number
  email: string
  firstName: string
  lastName: string
  createdAt: string
}

export interface AuthResponse {
  token: string
  account: Account
}

export interface Address {
  id: number
  label: string
  recipientName: string
  line1: string
  line2: string | null
  suburb: string
  state: string
  postcode: string
  country: string
  phone: string | null
  isDefault: boolean
}

export type AddressInput = Omit<Address, 'id'>

export interface Card {
  id: number
  cardholderName: string
  brand: string
  last4: string
  expiryMonth: number
  expiryYear: number
  isDefault: boolean
}

export interface CardInput {
  cardholderName: string
  number: string
  expiryMonth: number
  expiryYear: number
  cvc: string
  isDefault: boolean
}

export interface TestCard {
  number: string
  brand: string
}
