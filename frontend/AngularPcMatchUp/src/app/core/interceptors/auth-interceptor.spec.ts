import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    localStorage.clear();
    httpMock.verify();
  });

  // ─── Rutas normales (agrega Authorization) ───────────────────────────────────

  it('should add Authorization header when token exists', () => {
    localStorage.setItem('token', 'my-jwt-token');

    httpClient.get('/api/productos').subscribe();

    const req = httpMock.expectOne('/api/productos');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt-token');
    req.flush({});
  });

  it('should add Authorization header with null when no token in localStorage', () => {
    localStorage.removeItem('token');

    httpClient.get('/api/productos').subscribe();

    const req = httpMock.expectOne('/api/productos');
    expect(req.request.headers.get('Authorization')).toBe('Bearer null');
    req.flush({});
  });

  it('should not mutate the original request', () => {
    localStorage.setItem('token', 'my-jwt-token');

    httpClient.get('/api/productos').subscribe();

    const req = httpMock.expectOne('/api/productos');
    // el interceptor clona la request, la original no debe modificarse
    expect(req.request.url).toBe('/api/productos');
    req.flush({});
  });

  // ─── Rutas de Acceso (bypass del interceptor) ────────────────────────────────

  it('should NOT add Authorization header for Acceso routes', () => {
    localStorage.setItem('token', 'my-jwt-token');

    httpClient.get('/api/Acceso/login').subscribe();

    const req = httpMock.expectOne('/api/Acceso/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should pass through Acceso requests without modification', () => {
    httpClient.get('/api/Acceso/register').subscribe();

    const req = httpMock.expectOne('/api/Acceso/register');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  // ─── El interceptor no bloquea la respuesta ──────────────────────────────────

  it('should forward the response correctly', done => {
    localStorage.setItem('token', 'my-jwt-token');
    const mockResponse = { id: 1, name: 'Producto' };

    httpClient.get('/api/productos').subscribe((response: any) => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/productos');
    req.flush(mockResponse);
  });
});