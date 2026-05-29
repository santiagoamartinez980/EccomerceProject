import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { authGuard } from './auth-guard';
import { TokenService } from '../services/token.service';

const executeGuard: CanActivateFn = (...guardParameters) =>
  TestBed.runInInjectionContext(() => authGuard(...guardParameters));

describe('authGuard', () => {
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

  // ─── Sin token ───────────────────────────────────────────────────────────────

  it('should return false when there is no token in localStorage', () => {
    localStorage.removeItem('token');

    const result = executeGuard({} as any, {} as any);

    expect(result).toBe(false);
  });

  it('should redirect to /auth/login when there is no token', () => {
    localStorage.removeItem('token');

    executeGuard({} as any, {} as any);

    expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
  });

  // ─── Token válido ────────────────────────────────────────────────────────────

  it('should return true when token is valid and server responds isSuccess: true', done => {
    localStorage.setItem('token', 'valid-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      of({ isSuccess: true })
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe((result: boolean) => {
      expect(result).toBe(true);
      done();
    });
  });

  it('should not redirect when token is valid', done => {
    localStorage.setItem('token', 'valid-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      of({ isSuccess: true })
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe(() => {
      expect(router.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  // ─── Token inválido (servidor responde isSuccess: false) ──────────────────────

  it('should return false when server responds isSuccess: false', done => {
    localStorage.setItem('token', 'invalid-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      of({ isSuccess: false })
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe((result: boolean) => {
      expect(result).toBe(false);
      done();
    });
  });

  it('should redirect to /auth/login when server responds isSuccess: false', done => {
    localStorage.setItem('token', 'invalid-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      of({ isSuccess: false })
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe(() => {
      expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
      done();
    });
  });

  // ─── Error de red ────────────────────────────────────────────────────────────

  it('should return false when validateToken throws an error', done => {
    localStorage.setItem('token', 'some-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      throwError(() => new Error('Network error'))
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe((result: boolean) => {
      expect(result).toBe(false);
      done();
    });
  });

  it('should redirect to /auth/login when validateToken throws an error', done => {
    localStorage.setItem('token', 'some-token');
    jest.spyOn(tokenService, 'validateToken').mockReturnValue(
      throwError(() => new Error('Network error'))
    );

    const result$ = executeGuard({} as any, {} as any) as any;

    result$.subscribe(() => {
      expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
      done();
    });
  });
});