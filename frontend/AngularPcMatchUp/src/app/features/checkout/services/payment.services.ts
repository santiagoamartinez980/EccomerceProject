import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PaymentIntentDto } from '../checkout/interfaces/payment-interface.dto';


interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  value: T;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Payment`;

  createIntent(orderId: number): Observable<PaymentIntentDto> {
    return this.http
      .post<ApiResponse<PaymentIntentDto>>(`${this.base}/intent/${orderId}`, {})
      .pipe(map(r => r.value));
  }

  confirmPayment(orderId: number): Observable<any> {
    return this.http
      .post<ApiResponse<any>>(`${this.base}/confirm/${orderId}`, {})
      .pipe(map(r => r.value));
  }
}
