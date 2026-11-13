import { AddressDto } from '../../address/interfaces/address.dto';
import { OrderDetailDto } from './order-detail.dto';

export interface OrderDto {
  orderId: number;
  status: string;
  total: number;
  createdAt: string;
  customerName: string;
  customerEmail: string;
  address: AddressDto;
  details: OrderDetailDto[];
}
