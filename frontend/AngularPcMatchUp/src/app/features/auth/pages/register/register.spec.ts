import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { Register } from './register';
import { AuthApiService } from '../../services/auth-api.service';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let router: Router;
  let authApiService: AuthApiService;
  let snackOpenSpy: jest.SpyInstance;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    authApiService = TestBed.inject(AuthApiService);

    // Espiar la instancia real de MatSnackBar que usa el componente
    const snackBar = (component as any)['snack'] as MatSnackBar;
    snackOpenSpy = jest.spyOn(snackBar, 'open').mockReturnValue({} as any);

    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── formRegistro ───────────────────────────────────────────────────────────

  it('should initialize the form with empty fields', () => {
    expect(component.formRegistro.value).toEqual({
      nombre: '',
      apellidos: '',
      correo: '',
      clave: '',
    });
  });

  it('should mark the form as invalid when fields are empty', () => {
    expect(component.formRegistro.invalid).toBe(true);
  });

  it('should mark the form as valid when all fields are filled', () => {
    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    expect(component.formRegistro.valid).toBe(true);
  });

  // ─── hidePassword ───────────────────────────────────────────────────────────

  it('should initialize hidePassword as true', () => {
    expect(component.hidePassword).toBe(true);
  });

  // ─── registrarse() ──────────────────────────────────────────────────────────

  it('should not call authApiService.register if the form is invalid', () => {
    jest.spyOn(authApiService, 'register');

    component.registrarse();

    expect(authApiService.register).not.toHaveBeenCalled();
  });

  it('should call authApiService.register with the form values', () => {
    jest.spyOn(authApiService, 'register').mockReturnValue(of({} as any));
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    component.registrarse();

    expect(authApiService.register).toHaveBeenCalledWith({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
  });

  it('should show a success snackbar when registration succeeds', () => {
    jest.spyOn(authApiService, 'register').mockReturnValue(of({} as any));
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    component.registrarse();

    expect(snackOpenSpy).toHaveBeenCalledWith(
      '¡Registro exitoso! Bienvenido 🎉',
      'OK',
      { duration: 3000, panelClass: 'snack-success' }
    );
  });

  it('should show an error snackbar with the server message when registration fails', () => {
    const errorResponse = { error: { message: 'El correo ya está registrado' } };
    jest.spyOn(authApiService, 'register').mockReturnValue(throwError(() => errorResponse));

    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    component.registrarse();

    expect(snackOpenSpy).toHaveBeenCalledWith(
      'El correo ya está registrado',
      'Cerrar',
      { duration: 4000, panelClass: 'snack-error' }
    );
  });

  it('should show a fallback error message when the server provides none', () => {
    jest.spyOn(authApiService, 'register').mockReturnValue(throwError(() => ({})));

    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    component.registrarse();

    expect(snackOpenSpy).toHaveBeenCalledWith(
      'Error al registrarse, intenta de nuevo',
      'Cerrar',
      { duration: 4000, panelClass: 'snack-error' }
    );
  });

  it('should not navigate when registration fails', () => {
    jest.spyOn(authApiService, 'register').mockReturnValue(throwError(() => ({})));
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.formRegistro.setValue({
      nombre: 'Juan',
      apellidos: 'Pérez',
      correo: 'juan@test.com',
      clave: '123456',
    });
    component.registrarse();

    expect(router.navigate).not.toHaveBeenCalled();
  });

  // ─── volver() ───────────────────────────────────────────────────────────────

  it('should navigate to root when volver() is called', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.volver();

    expect(router.navigate).toHaveBeenCalledWith(['']);
  });
});