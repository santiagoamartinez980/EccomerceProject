import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { AdminCategoriesComponent } from './admin-categories';
import { AdminCategoriesService } from '../../../categories/services/category.service';
import { CategoryInterface } from '../../../categories/interfaces/category.interface';

const MOCK_CATEGORIES: CategoryInterface[] = [
  { categoryId: 1, name: 'Laptops' },
  { categoryId: 2, name: 'Monitores' },
];

describe('AdminCategoriesComponent', () => {
  let component: AdminCategoriesComponent;
  let fixture: ComponentFixture<AdminCategoriesComponent>;
  let service: jest.Mocked<AdminCategoriesService>;
  let snackBar: jest.Mocked<MatSnackBar>;

  beforeEach(async () => {
  const serviceMock: jest.Mocked<AdminCategoriesService> = {
    getAll: jest.fn().mockReturnValue(of(MOCK_CATEGORIES)),
    create: jest.fn().mockReturnValue(of({})),
    delete: jest.fn().mockReturnValue(of({})),
  } as any;

  const snackMock = { open: jest.fn() } as any;

  await TestBed.configureTestingModule({
    imports: [AdminCategoriesComponent],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter([]),
      provideAnimations(),
      { provide: AdminCategoriesService, useValue: serviceMock },
    ],
  })
  .overrideProvider(MatSnackBar, { useValue: snackMock })  // ← cambio clave
  .compileComponents();

  service = TestBed.inject(AdminCategoriesService) as jest.Mocked<AdminCategoriesService>;
  snackBar = TestBed.inject(MatSnackBar) as jest.Mocked<MatSnackBar>;

  fixture = TestBed.createComponent(AdminCategoriesComponent);
  component = fixture.componentInstance;
  fixture.detectChanges();
});
  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ────────────────────────────────────────────────────────────────

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have the correct displayed columns', () => {
    expect(component.displayedColumns).toEqual(['id', 'nombre', 'acciones']);
  });

  it('should have an invalid form initially', () => {
    expect(component.form.invalid).toBe(true);
  });

  // ─── load / ngOnInit ─────────────────────────────────────────────────────────

  it('should call getAll on init', () => {
    expect(service.getAll).toHaveBeenCalled();
  });

  it('should populate categories signal after load', () => {
    expect(component.categories()).toEqual(MOCK_CATEGORIES);
  });

  it('should set loading to false after successful load', () => {
    expect(component.loading()).toBe(false);
  });

  it('should show snackbar and stop loading on load error', () => {
    service.getAll.mockReturnValue(throwError(() => new Error('fail')));
    component.load();
    expect(snackBar.open).toHaveBeenCalledWith('Error al cargar categorías', 'OK', { duration: 3000 });
    expect(component.loading()).toBe(false);
  });

  // ─── create ──────────────────────────────────────────────────────────────────

  it('should not call service.create when form is invalid', () => {
    component.form.setValue({ name: '' });
    component.create();
    expect(service.create).not.toHaveBeenCalled();
  });

  it('should call service.create with form value when form is valid', () => {
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(service.create).toHaveBeenCalledWith({ name: 'Nueva categoría' });
  });

  it('should reset form after successful create', () => {
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(component.form.value.name).toBeFalsy();
  });

  it('should show success snackbar after create', () => {
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(snackBar.open).toHaveBeenCalledWith('Categoría creada', 'OK', { duration: 2500 });
  });

  it('should reload categories after successful create', () => {
    const callsBefore = service.getAll.mock.calls.length;
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(service.getAll.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  it('should set saving to false after successful create', () => {
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(component.saving()).toBe(false);
  });

  it('should show error snackbar when create fails', () => {
    service.create.mockReturnValue(throwError(() => new Error('fail')));
    component.form.setValue({ name: 'Nueva categoría' });
    component.create();
    expect(snackBar.open).toHaveBeenCalledWith('Error al crear categoría', 'OK', { duration: 3000 });
    expect(component.saving()).toBe(false);
  });

  // ─── confirmDelete ────────────────────────────────────────────────────────────

  it('should not call service.delete when user cancels confirm dialog', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(false);
    component.confirmDelete(MOCK_CATEGORIES[0]);
    expect(service.delete).not.toHaveBeenCalled();
  });

  it('should call service.delete with category id when user confirms', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    component.confirmDelete(MOCK_CATEGORIES[0]);
    expect(service.delete).toHaveBeenCalledWith(1);
  });

  it('should show success snackbar after delete', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    component.confirmDelete(MOCK_CATEGORIES[0]);
    expect(snackBar.open).toHaveBeenCalledWith('Categoría eliminada', 'OK', { duration: 2500 });
  });

  it('should reload categories after successful delete', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    const callsBefore = service.getAll.mock.calls.length;
    component.confirmDelete(MOCK_CATEGORIES[0]);
    expect(service.getAll.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  it('should show error snackbar when delete fails', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    service.delete.mockReturnValue(throwError(() => new Error('fail')));
    component.confirmDelete(MOCK_CATEGORIES[0]);
    expect(snackBar.open).toHaveBeenCalledWith(
      'No se puede eliminar: tiene productos asociados', 'OK', { duration: 4000 }
    );
  });
});