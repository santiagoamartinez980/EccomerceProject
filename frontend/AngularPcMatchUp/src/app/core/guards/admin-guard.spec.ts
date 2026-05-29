import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { adminGuard } from './admin-guard';
import { TokenService } from '../services/token.service';

const executeGuard: CanActivateFn = (...guardParameters) =>
  TestBed.runInInjectionContext(() => adminGuard(...guardParameters));

describe('adminGuard', () => {
  let tokenService: TokenService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    tokenService = TestBed.inject(TokenService);
    router = TestBed.inject(Router);

    jest.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    localStorage.clear();
    jest.restoreAllMocks();
  });

  // ─── Usuario no autenticado ──────────────────────────────────────────────────

  it('should return false when user is not logged in', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(false);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);

    const result = executeGuard({} as any, {} as any);

    expect(result).toBe(false);
  });

  it('should redirect to home when user is not logged in', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(false);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);

    executeGuard({} as any, {} as any);

    expect(router.navigate).toHaveBeenCalledWith(['']);
  });

  // ─── Usuario autenticado pero sin rol Admin ───────────────────────────────────

  it('should return false when user is logged in but not admin', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(true);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);

    const result = executeGuard({} as any, {} as any);

    expect(result).toBe(false);
  });

  it('should redirect to /productos when user is logged in but not admin', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(true);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);

    executeGuard({} as any, {} as any);

    expect(router.navigate).toHaveBeenCalledWith(['/productos']);
  });

  // ─── Usuario Admin ────────────────────────────────────────────────────────────

  it('should return true when user is logged in and is admin', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(true);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(true);

    const result = executeGuard({} as any, {} as any);

    expect(result).toBe(true);
  });

  it('should not redirect when user is admin', () => {
    jest.spyOn(tokenService, 'isLoggedIn').mockReturnValue(true);
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(true);

    executeGuard({} as any, {} as any);

    expect(router.navigate).not.toHaveBeenCalled();
  });
});