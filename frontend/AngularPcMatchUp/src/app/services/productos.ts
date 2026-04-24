import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { responseProducto } from '../interfaces/ResponseProducto';

@Injectable({
  providedIn: 'root',
})
export class Productos {
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl;

  constructor() {}
  lista():Observable<responseProducto>{
    return this.http.get<responseProducto>(`${this.apiUrl}/Producto/lista`);
  }
}
