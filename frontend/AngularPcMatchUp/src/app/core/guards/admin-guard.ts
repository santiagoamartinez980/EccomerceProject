// core/guards/admin.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';



export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const tokenService = inject(TokenService);

  if (!tokenService.isLoggedIn()) {
    router.navigate(['']);
    return false;
  }

  if (tokenService.isAdmin()) {
    return true;
  }

  router.navigate(['/productos']);
  return false;
};