export interface CartItemDto {
  cartItemId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  imagenUrl?: string;
  stock: number;
}