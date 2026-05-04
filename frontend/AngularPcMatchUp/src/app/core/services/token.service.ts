
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';

import { environment } from '../../../environments/environment';

interface JwtPayload {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  role?: string;

  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;

  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;

  exp?: number;
}

@Injectable({
  providedIn: 'root',
})
export class TokenService {

  private http = inject(HttpClient);

  private apiUrl = environment.apiUrl;

  constructor() {}

  validateToken(token: string) {
    return this.http.get(
      `${this.apiUrl}/Access/validate-token?token=${token}`
    );
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  setToken(token: string): void {
    localStorage.setItem('token', token);
  }

  removeToken(): void {
    localStorage.removeItem('token');
  }

  decodeToken(): JwtPayload | null {

    const token = this.getToken();

    if (!token) return null;

    try {
      return jwtDecode<JwtPayload>(token);
    } catch {
      return null;
    }
  }

  getRole(): string | null {

    const decoded = this.decodeToken();

    if (!decoded) return null;

    return (
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
      decoded.role ??
      null
    );
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isLoggedIn(): boolean {

    const decoded = this.decodeToken();

    if (!decoded?.exp) return false;

    const currentTime = Math.floor(Date.now() / 1000);

    return decoded.exp > currentTime;
  }
}