import { CartItemDto } from "./cart-item.dto";

export interface CartDto {
  id: number;
  userId: number;
  isActive: boolean;
  items: CartItemDto[];
  total: number;
}