import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { OrderDto } from '../../order/interfaces/order.dto';

interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  value: T;
}

export type AdminOrderStatus = 1 | 2 | 3 | 4;

@Injectable({ providedIn: 'root' })
export class AdminOrdersService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Order`;

  list(): Observable<OrderDto[]> {
    return this.http.get<ApiResponse<OrderDto[]>>(`${this.base}/admin`).pipe(map(r => r.value));
  }

  updateStatus(id: number, status: AdminOrderStatus): Observable<OrderDto> {
    return this.http.patch<ApiResponse<OrderDto>>(`${this.base}/${id}/status`, { status })
      .pipe(map(r => r.value));
  }
}
