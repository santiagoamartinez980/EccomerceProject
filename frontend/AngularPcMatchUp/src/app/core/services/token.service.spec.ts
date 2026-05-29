import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TokenService } from './token.service';
import { environment } from '../../../environments/environment';

// Token JWT falso con rol Admin, expira en el futuro
const FAKE_TOKEN_ADMIN =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
  btoa(JSON.stringify({
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Admin',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'admin@test.com',
    exp: Math.floor(Date.now() / 1000) + 3600, // expira en 1 hora
  })).replace(/=/g, '') +
  '.fake-signature';

// Token JWT falso con rol User, expira en el futuro
const FAKE_TOKEN_USER =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
  btoa(JSON.stringify({
    role: 'User',
    exp: Math.floor(Date.now() / 1000) + 3600,
  })).replace(/=/g, '') +
  '.fake-signature';

// Token JWT falso ya expirado
const FAKE_TOKEN_EXPIRED =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.' +
  btoa(JSON.stringify({
    role: 'User',
    exp: Math.floor(Date.now() / 1000) - 100, // expiró hace 100 segundos
  })).replace(/=/g, '') +
  '.fake-signature';

describe('TokenService', () => {
  let service: TokenService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(TokenService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    localStorage.clear();
    httpMock.verify(); // verifica que no haya requests HTTP pendientes
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── getToken / setToken / removeToken ───────────────────────────────────────

  it('should return null when no token in localStorage', () => {
    expect(service.getToken()).toBeNull();
  });

  it('should store and retrieve a token', () => {
    service.setToken('my-token');
    expect(service.getToken()).toBe('my-token');
  });

  it('should remove the token from localStorage', () => {
    service.setToken('my-token');
    service.removeToken();
    expect(service.getToken()).toBeNull();
  });

  // ─── isLoggedIn signal ───────────────────────────────────────────────────────

  it('should set isLoggedIn$ to true after setToken', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    expect(service.isLoggedIn$()).toBe(true);
  });

  it('should set isLoggedIn$ to false after removeToken', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    service.removeToken();
    expect(service.isLoggedIn$()).toBe(false);
  });

  // ─── decodeToken ─────────────────────────────────────────────────────────────

  it('should return null when no token is stored', () => {
    expect(service.decodeToken()).toBeNull();
  });

  it('should return null for an invalid token', () => {
    localStorage.setItem('token', 'not-a-valid-jwt');
    expect(service.decodeToken()).toBeNull();
  });

  it('should decode a valid token and return payload', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    const decoded = service.decodeToken();
    expect(decoded).not.toBeNull();
    expect(decoded?.exp).toBeDefined();
  });

  // ─── getRole ─────────────────────────────────────────────────────────────────

  it('should return null when no token is stored', () => {
    expect(service.getRole()).toBeNull();
  });

  it('should return Admin role from MS claims namespace', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    expect(service.getRole()).toBe('Admin');
  });

  it('should return role from generic "role" claim', () => {
    service.setToken(FAKE_TOKEN_USER);
    expect(service.getRole()).toBe('User');
  });

  // ─── isAdmin ─────────────────────────────────────────────────────────────────

  it('should return true when role is Admin', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    expect(service.isAdmin()).toBe(true);
  });

  it('should return false when role is User', () => {
    service.setToken(FAKE_TOKEN_USER);
    expect(service.isAdmin()).toBe(false);
  });

  it('should return false when no token is stored', () => {
    expect(service.isAdmin()).toBe(false);
  });

  // ─── isLoggedIn ──────────────────────────────────────────────────────────────

  it('should return false when no token is stored', () => {
    expect(service.isLoggedIn()).toBe(false);
  });

  it('should return true when token is valid and not expired', () => {
    service.setToken(FAKE_TOKEN_ADMIN);
    expect(service.isLoggedIn()).toBe(true);
  });

  it('should return false when token is expired', () => {
    service.setToken(FAKE_TOKEN_EXPIRED);
    expect(service.isLoggedIn()).toBe(false);
  });

  // ─── validateToken (HTTP) ────────────────────────────────────────────────────

  it('should call the validate-token endpoint with the correct URL', () => {
    service.validateToken('abc123').subscribe();

    const req = httpMock.expectOne(
      `${environment.apiUrl}/Access/validate-token?token=abc123`
    );
    expect(req.request.method).toBe('GET');
    req.flush({ valid: true });
  });
});