import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { OrderDto } from '../interfaces/order.dto';
import { CreateOrderDto } from '../interfaces/order-create.dto';


interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  value: T;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Order`;

  getAll(): Observable<OrderDto[]> {
    return this.http
      .get<ApiResponse<OrderDto[]>>(this.base)
      .pipe(map(r => r.value));
  }

  getById(id: number): Observable<OrderDto> {
    return this.http
      .get<ApiResponse<OrderDto>>(`${this.base}/${id}`)
      .pipe(map(r => r.value));
  }

  create(dto: CreateOrderDto): Observable<OrderDto> {
    return this.http
      .post<ApiResponse<OrderDto>>(this.base, dto)
      .pipe(map(r => r.value));
  }

  cancel(id: number): Observable<void> {
    return this.http
      .post<ApiResponse<void>>(`${this.base}/${id}/cancel`, {})
      .pipe(map(() => void 0));
  }
}