import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { ProductInterface } from '../interfaces/product.interface';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../interfaces/api-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductsQueryService {
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl+'/Producto';

  search(name: string): Observable<ProductInterface[]> {

    const params = new HttpParams()
      .set('name', name);

    return this.http
      .get<ApiResponse<ProductInterface[]>>(
        `${this.apiUrl}/buscar`,
        { params }
      )
      .pipe(map(res => res.value));

  }

}
