import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { Acceso } from '../services/acceso';

export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('token')||"";
  const router = inject(Router);
  const acceso = inject(Acceso);
  if (!token) {
    router.navigate(['']);
    return false;
  }
  return acceso.validarToken(token).pipe(
    map((data: any) => {
      if (data) {
        return true;
      } else {
        router.navigate(['']);
        return false;
      }
    }),
    catchError(err => {
      console.error('Error al validar el token:', err);
      router.navigate(['']);
      return of(false);
    })
  );
};
