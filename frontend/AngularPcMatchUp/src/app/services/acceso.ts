import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { usuario } from '../interfaces/Usuario';
import { Observable } from 'rxjs';
import { responseAcceso } from '../interfaces/ResponseAcceso';
import { Login } from '../interfaces/Login';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Acceso {
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl;

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
