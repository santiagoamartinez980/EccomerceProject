import { CartDto } from "./cart.dto";

export interface CartResponse {
  isSuccess: boolean;
  message: string;
  value: CartDto;
}