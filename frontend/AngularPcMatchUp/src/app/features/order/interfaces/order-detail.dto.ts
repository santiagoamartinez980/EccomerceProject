export interface OrderDetailDto {
  productId: number;
  productName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}
 