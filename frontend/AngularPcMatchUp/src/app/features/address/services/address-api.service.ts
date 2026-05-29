import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { AddressDto } from '../interfaces/address.dto';
import { CreateAddressRequest } from '../interfaces/create-address.interface';
import { AddressListResponse, AddressSingleResponse } from '../interfaces/address-response.interface';

@Injectable({ providedIn: 'root' })
export class AddressApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Address`;

  // GET /api/address — obtiene todas las direcciones del usuario
  getAll(): Observable<AddressDto[]> {
    return this.http
      .get<AddressListResponse>(this.base)
      .pipe(map(r => r.value));
  }

  // GET /api/address/default — obtiene la dirección predeterminada
  getDefault(): Observable<AddressDto> {
    return this.http
      .get<AddressSingleResponse>(`${this.base}/default`)
      .pipe(map(r => r.value));
  }

  // POST /api/address — crea una nueva dirección
  create(request: CreateAddressRequest): Observable<AddressDto> {
    return this.http
      .post<AddressSingleResponse>(this.base, request)
      .pipe(map(r => r.value));
  }

  // PUT /api/address/:id — actualiza una dirección
  update(id: number, request: CreateAddressRequest): Observable<AddressDto> {
    return this.http
      .put<AddressSingleResponse>(`${this.base}/${id}`, request)
      .pipe(map(r => r.value));
  }

  // DELETE /api/address/:id — elimina una dirección
  delete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.base}/${id}`);
  }
}
