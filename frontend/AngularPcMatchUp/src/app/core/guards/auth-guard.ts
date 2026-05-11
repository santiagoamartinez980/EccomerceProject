import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { TokenService } from '../services/token.service';
//20-04-2026 se agrega el guard para validar el token antes de acceder a las rutas protegidas, si el token no es valido se redirige al login
export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('token')||"";
  const router = inject(Router);
  const tokenService = inject(TokenService);
  if (!token) {
    router.navigate(['/auth/login']);
    return false;
  }
  return tokenService.validateToken(token).pipe(
    map((data: any) => {
      console.log('GUARD DATA:', data);  // ← agrega esto
      if (data.isSuccess) {
        return true;
      } else {
        router.navigate(['/auth/login']);
        return false;
      }
    }),
    catchError(err => {
      console.error('Error al validar el token:', err);
      router.navigate(['/auth/login']);
      return of(false);
    })
  );
};
