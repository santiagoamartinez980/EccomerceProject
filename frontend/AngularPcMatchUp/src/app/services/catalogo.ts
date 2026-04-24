// services/catalogo.service.ts
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Producto } from '../interfaces/Producto';
import { responseProducto, ResponseProductoSingle } from '../interfaces/ResponseProducto';

@Injectable({
  providedIn: 'root',
})
export class Catalogo {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;


  lista(): Observable<Producto[]> {
    return this.http
      .get<responseProducto>(`${this.apiUrl}/Producto`)
      .pipe(map((res) => res.value));
  }

  porCategoria(categoria: string): Observable<Producto[]> {
    const params = new HttpParams().set('categoria', categoria);
    return this.http
      .get<responseProducto>(`${this.apiUrl}/Producto/categoria`, { params })
      .pipe(map((res) => res.value));
  }

  
  buscarPorNombre(nombre: string): Observable<Producto[]> {
    const params = new HttpParams().set('nombre', nombre);
    return this.http
      .get<responseProducto>(`${this.apiUrl}/Producto/buscar`, { params })
      .pipe(map((res) => res.value));
  }

  
  detalle(id: number): Observable<Producto> {
    return this.http
      .get<ResponseProductoSingle>(`${this.apiUrl}/Producto/${id}`)
      .pipe(map((res) => res.value));
  }
}