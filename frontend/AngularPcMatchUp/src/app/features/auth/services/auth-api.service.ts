import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginResponse } from '../interfaces/login-response.interface';
import { environment } from '../../../../environments/environment';
import { RegisterRequest } from '../interfaces/register-request.interface';
import { LoginRequest } from '../interfaces/login-request.interface';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl;

  constructor() {}

  register(object: RegisterRequest):Observable<LoginResponse>{
    return this.http.post<LoginResponse>(`${this.apiUrl}/Access/register`, object);
  }

  login(object:LoginRequest):Observable<LoginResponse>{
    return this.http.post<LoginResponse>(`${this.apiUrl}/Access/login`, object);
  }



}
