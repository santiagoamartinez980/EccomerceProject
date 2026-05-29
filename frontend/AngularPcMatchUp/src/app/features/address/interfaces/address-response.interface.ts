import { AddressDto } from './address.dto';

export interface AddressResponse<T> {
  isSuccess: boolean;
  message: string;
  value: T;
}

export interface AddressListResponse extends AddressResponse<AddressDto[]> {}
export interface AddressSingleResponse extends AddressResponse<AddressDto> {}
