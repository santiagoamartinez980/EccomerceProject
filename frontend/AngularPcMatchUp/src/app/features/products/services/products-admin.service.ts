import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ProductInterface } from '../interfaces/product.interface';
import { CreateProductDto } from '../interfaces/create-product.dto';
import { UpdateProductDto } from '../interfaces/update-product.dto';

@Injectable({
  providedIn: 'root',
})
export class ProductsAdminService{
  private http=inject(HttpClient);
  private base=environment.apiUrl+'/Producto';
  
  create(payload: CreateProductDto): Observable<ProductInterface> {
    return this.http
      .post<{ value: ProductInterface }>(this.base, payload)
      .pipe(map((r) => r.value));
  }
 
  update(id: number, payload: UpdateProductDto): Observable<ProductInterface> {
    return this.http
      .put<{ value: ProductInterface }>(`${this.base}/${id}`, payload)
      .pipe(map((r) => r.value));
  }
 
  delete(id: number): Observable<{ mensaje: string }> {
    return this.http.delete<{ mensaje: string }>(`${this.base}/${id}`);
  }
}
