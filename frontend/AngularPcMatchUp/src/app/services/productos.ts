import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AppSettings } from '../settings/appsettings';
import { Observable } from 'rxjs';
import { responseProducto } from '../interfaces/ResponseProducto';

@Injectable({
  providedIn: 'root',
})
export class Productos {
  private http=inject(HttpClient);
  private apiUrl=AppSettings.apiUrl;

  constructor() {}
  lista():Observable<responseProducto>{
    return this.http.get<responseProducto>(`${this.apiUrl}/Producto/lista`);
  }
}
