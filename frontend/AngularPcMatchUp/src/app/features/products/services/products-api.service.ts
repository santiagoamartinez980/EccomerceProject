import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../interfaces/api-response.interface';
import { ProductInterface } from '../interfaces/product.interface';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ProductsApiService {
  delete(idProducto: number) {
    throw new Error('Method not implemented.');
  }
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl+'/Producto';
  

  getAll(): Observable<ProductInterface[]> {

    return this.http
      .get<ApiResponse<ProductInterface[]>>(this.apiUrl)
      .pipe(map(res => res.value));

  }

  getList(): Observable<ProductInterface[]> {

    return this.http
      .get<ApiResponse<ProductInterface[]>>(`${this.apiUrl}/lista`)
      .pipe(map(res => res.value));

  }

  getById(id: number): Observable<ProductInterface> {

    return this.http
      .get<ApiResponse<ProductInterface >>(`${this.apiUrl}/${id}`)
      .pipe(map(res => res.value));

  }

  getByIdPublic(id: number): Observable<ProductInterface> {

    return this.http
      .get<ApiResponse<ProductInterface>>(`${this.apiUrl}/public/${id}`)
      .pipe(map(res => res.value));

  }
}
