import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Login } from './login';
import { AuthApiService } from '../../services/auth-api.service';
import { TokenService } from '../../../../core/services/token.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let router: Router;
  let authApiService: AuthApiService;
  let tokenService: TokenService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    authApiService = TestBed.inject(AuthApiService);
    tokenService = TestBed.inject(TokenService);
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── formLogin ──────────────────────────────────────────────────────────────

  it('should initialize the form with empty fields', () => {
    expect(component.formLogin.value).toEqual({ correo: '', clave: '' });
  });

  it('should mark the form as invalid when fields are empty', () => {
    expect(component.formLogin.invalid).toBe(true);
  });

  it('should mark the form as valid when both fields are filled', () => {
    component.formLogin.setValue({ correo: 'user@test.com', clave: '123456' });
    expect(component.formLogin.valid).toBe(true);
  });

  // ─── hidePassword ───────────────────────────────────────────────────────────

  it('should initialize hidePassword as true', () => {
    expect(component.hidePassword).toBe(true);
  });

  // ─── inisiarSesion() ────────────────────────────────────────────────────────

  it('should not call authApiService.login if the form is invalid', () => {
    jest.spyOn(authApiService, 'login');
    component.formLogin.setValue({ correo: '', clave: '' });

    component.inisiarSesion();

    expect(authApiService.login).not.toHaveBeenCalled();
  });

  it('should call authApiService.login with the form values', () => {
    jest.spyOn(authApiService, 'login').mockReturnValue(of({ value: { token: 'fake-token' } } as any));
    jest.spyOn(tokenService, 'setToken').mockImplementation(() => {});
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formLogin.setValue({ correo: 'user@test.com', clave: '123456' });
    component.inisiarSesion();

    expect(authApiService.login).toHaveBeenCalledWith({
      correo: 'user@test.com',
      clave: '123456',
    });
  });

  it('should store the token when login is successful', () => {
    jest.spyOn(authApiService, 'login').mockReturnValue(of({ value: { token: 'fake-token' } } as any));
    jest.spyOn(tokenService, 'setToken').mockImplementation(() => {});
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formLogin.setValue({ correo: 'user@test.com', clave: '123456' });
    component.inisiarSesion();

    expect(tokenService.setToken).toHaveBeenCalledWith('fake-token');
  });

  it('should navigate to /admin when the user is admin', () => {
    jest.spyOn(authApiService, 'login').mockReturnValue(of({ value: { token: 'fake-token' } } as any));
    jest.spyOn(tokenService, 'setToken').mockImplementation(() => {});
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(true);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formLogin.setValue({ correo: 'admin@test.com', clave: '123456' });
    component.inisiarSesion();

    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
  });

  it('should navigate to /productos when the user is not admin', () => {
    jest.spyOn(authApiService, 'login').mockReturnValue(of({ value: { token: 'fake-token' } } as any));
    jest.spyOn(tokenService, 'setToken').mockImplementation(() => {});
    jest.spyOn(tokenService, 'isAdmin').mockReturnValue(false);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formLogin.setValue({ correo: 'user@test.com', clave: '123456' });
    component.inisiarSesion();

    expect(router.navigate).toHaveBeenCalledWith(['/productos']);
  });

  it('should not navigate when login fails', () => {
    jest.spyOn(authApiService, 'login').mockReturnValue(throwError(() => new Error('Unauthorized')));
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formLogin.setValue({ correo: 'user@test.com', clave: 'wrong' });
    component.inisiarSesion();

    expect(router.navigate).not.toHaveBeenCalled();
  });

  // ─── registrarse() ──────────────────────────────────────────────────────────

  it('should navigate to /auth/registro when registrarse() is called', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.registrarse();

    expect(router.navigate).toHaveBeenCalledWith(['/auth/registro']);
  });
});