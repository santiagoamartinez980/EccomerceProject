export interface CreateAddressRequest {
  addressLine: string;
  city: string;
  department: string;
  country: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  phone: string;
  notes?: string;
  placeId?: string;
  formattedAddress?: string;
  isDefault: boolean;
}
