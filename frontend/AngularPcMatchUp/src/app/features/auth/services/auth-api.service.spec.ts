import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { AuthApiService } from './auth-api.service';
import { environment } from '../../../../environments/environment';

describe('AuthApiService', () => {
  let service: AuthApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AuthApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── register() ─────────────────────────────────────────────────────────────

  it('should POST to /Access/register with the given object', () => {
    const payload = { nombre: 'Juan', apellidos: 'Pérez', correo: 'juan@test.com', clave: '123456' };

    service.register(payload as any).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ value: { token: 'fake-token' } });
  });

  it('should return the response from /Access/register', () => {
    const payload = { nombre: 'Juan', apellidos: 'Pérez', correo: 'juan@test.com', clave: '123456' };
    const mockResponse = { value: { token: 'fake-token' } };
    let result: any;

    service.register(payload as any).subscribe(res => (result = res));

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/register`);
    req.flush(mockResponse);

    expect(result).toEqual(mockResponse);
  });

  it('should propagate HTTP errors from /Access/register', () => {
    const payload = { nombre: 'Juan', apellidos: 'Pérez', correo: 'juan@test.com', clave: '123456' };
    let errorReceived = false;

    service.register(payload as any).subscribe({
      next: () => fail('debería haber fallado'),
      error: () => (errorReceived = true),
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/register`);
    req.flush('Conflict', { status: 409, statusText: 'Conflict' });

    expect(errorReceived).toBe(true);
  });

  // ─── login() ────────────────────────────────────────────────────────────────

  it('should POST to /Access/login with the given credentials', () => {
    const payload = { correo: 'juan@test.com', clave: '123456' };

    service.login(payload as any).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ value: { token: 'fake-token' } });
  });

  it('should return the response from /Access/login', () => {
    const payload = { correo: 'juan@test.com', clave: '123456' };
    const mockResponse = { value: { token: 'fake-token' } };
    let result: any;

    service.login(payload as any).subscribe(res => (result = res));

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/login`);
    req.flush(mockResponse);

    expect(result).toEqual(mockResponse);
  });

  it('should propagate HTTP errors from /Access/login', () => {
    const payload = { correo: 'juan@test.com', clave: 'wrong' };
    let errorReceived = false;

    service.login(payload as any).subscribe({
      next: () => fail('debería haber fallado'),
      error: () => (errorReceived = true),
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Access/login`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(errorReceived).toBe(true);
  });
});