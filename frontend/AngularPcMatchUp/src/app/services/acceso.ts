import { inject, Injectable } from '@angular/core';
import { AppSettings } from '../settings/appsettings';
import { HttpClient } from '@angular/common/http';
import { usuario } from '../interfaces/Usuario';
import { Observable } from 'rxjs';
import { responseAcceso } from '../interfaces/ResponseAcceso';
import { Login } from '../interfaces/Login';

@Injectable({
  providedIn: 'root',
})
export class Acceso {
  private http=inject(HttpClient);
  private apiUrl=AppSettings.apiUrl;

  constructor() {}

  registrarse(objeto:usuario):Observable<responseAcceso>{
    return this.http.post<responseAcceso>(`${this.apiUrl}/Acceso/registrar`, objeto);
  }

  login(objeto:Login):Observable<responseAcceso>{
    return this.http.post<responseAcceso>(`${this.apiUrl}/Acceso/login`, objeto);
  }

  validarToken(token:string):Observable<responseAcceso>{
    return this.http.get<responseAcceso>(`${this.apiUrl}/Acceso/ValidarToken?token=${token}`);
  }

  
}
