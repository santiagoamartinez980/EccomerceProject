import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { CartDto } from '../interfaces/cart.dto';
import { CartResponse } from '../interfaces/cart-item-response.interface';
import { CartItemRequest } from '../interfaces/cart-item-request.interface';
 
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Cart`;
 
  /** GET /api/Cart — obtiene o crea el carrito del usuario */
  getCart(): Observable<CartDto> {
    return this.http
      .get<CartResponse>(this.base)
      .pipe(map((r) => r.value));
  }
 
  /** POST /api/Cart/items — agrega o actualiza un item */
  addOrUpdateItem(request: CartItemRequest): Observable<CartDto> {
    return this.http
      .post<CartResponse>(`${this.base}/items`, request)
      .pipe(map((r) => r.value));
  }
 
  /** DELETE /api/Cart/items/:productId — elimina un item */
  removeItem(productId: number): Observable<CartDto> {
    return this.http
      .delete<CartResponse>(`${this.base}/items/${productId}`)
      .pipe(map((r) => r.value));
  }
 
  /** DELETE /api/Cart — vacía el carrito */
  clearCart(): Observable<void> {
    return this.http.delete<void>(this.base);
  }
}