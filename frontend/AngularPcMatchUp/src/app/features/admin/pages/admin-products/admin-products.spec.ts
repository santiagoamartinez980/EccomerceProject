import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { AdminProductsComponent } from './admin-products';
import { ProductsAdminService } from '../../../products/services/products-admin.service';
import { ProductsApiService } from '../../../products/services/products-api.service';
import { AdminCategoriesService } from '../../../categories/services/category.service';
import { ProductInterface } from '../../../products/interfaces/product.interface';
import { CategoryInterface } from '../../../categories/interfaces/category.interface';

const MOCK_PRODUCTS: ProductInterface[] = [
  { idProducto: 1, nombre: 'Laptop Gamer', categoriaNombre: 'Laptops', precio: 3000000, stock: 5, activo: true, fechaCreacion: '2024-01-01', idCategoria: 1 },
  { idProducto: 2, nombre: 'Monitor 4K',   categoriaNombre: 'Monitores', precio: 1500000, stock: 10, activo: true, fechaCreacion: '2024-01-02', idCategoria: 2 },
];

const MOCK_CATEGORIES: CategoryInterface[] = [
  { categoryId: 1, name: 'Laptops' },
  { categoryId: 2, name: 'Monitores' },
];

describe('AdminProductsComponent', () => {
  let component: AdminProductsComponent;
  let fixture: ComponentFixture<AdminProductsComponent>;
  let adminService: jest.Mocked<ProductsAdminService>;
  let categoriesService: jest.Mocked<AdminCategoriesService>;
  let snackBar: jest.Mocked<MatSnackBar>;
  let dialog: jest.Mocked<MatDialog>;

  beforeEach(async () => {
    const adminServiceMock: jest.Mocked<ProductsAdminService> = {
      getAlladmin: jest.fn().mockReturnValue(of(MOCK_PRODUCTS)),
      delete: jest.fn().mockReturnValue(of({})),
    } as any;

    const categoriesServiceMock: jest.Mocked<AdminCategoriesService> = {
      getAll: jest.fn().mockReturnValue(of(MOCK_CATEGORIES)),
    } as any;

    const snackMock = { open: jest.fn() } as any;

    const dialogMock: jest.Mocked<MatDialog> = {
      open: jest.fn().mockReturnValue({
        afterClosed: jest.fn().mockReturnValue(of(null)),
      } as any),
    } as any;

    await TestBed.configureTestingModule({
      imports: [AdminProductsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
        { provide: ProductsAdminService, useValue: adminServiceMock },
        { provide: ProductsApiService, useValue: {} },
        { provide: AdminCategoriesService, useValue: categoriesServiceMock },
        
      ],
    })
    .overrideProvider(MatSnackBar, { useValue: snackMock })
    .overrideProvider(MatDialog, { useValue: dialogMock })
    .compileComponents();

    adminService = TestBed.inject(ProductsAdminService) as jest.Mocked<ProductsAdminService>;
    categoriesService = TestBed.inject(AdminCategoriesService) as jest.Mocked<AdminCategoriesService>;
    snackBar = TestBed.inject(MatSnackBar) as jest.Mocked<MatSnackBar>;
    dialog = TestBed.inject(MatDialog) as jest.Mocked<MatDialog>;

    fixture = TestBed.createComponent(AdminProductsComponent);
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
    expect(component.displayedColumns).toEqual([
      'imagen', 'nombre', 'categoria', 'precio', 'stock', 'activo', 'acciones'
    ]);
  });

  it('should start with empty searchTerm', () => {
    expect(component.searchTerm()).toBe('');
  });

  // ─── loadAll / ngOnInit ───────────────────────────────────────────────────────

  it('should call getAlladmin on init', () => {
    expect(adminService.getAlladmin).toHaveBeenCalled();
  });

  it('should call getAll categories on init', () => {
    expect(categoriesService.getAll).toHaveBeenCalled();
  });

  it('should populate products signal after load', () => {
    expect(component.products()).toEqual(MOCK_PRODUCTS);
  });

  it('should populate categories signal after load', () => {
    expect(component.categories()).toEqual(MOCK_CATEGORIES);
  });

  it('should set loading to false after successful load', () => {
    expect(component.loading()).toBe(false);
  });

  it('should show snackbar and stop loading on products load error', () => {
    adminService.getAlladmin.mockReturnValue(throwError(() => new Error('fail')));
    component.loadAll();
    expect(snackBar.open).toHaveBeenCalledWith('Error al cargar productos', 'OK', { duration: 3000 });
    expect(component.loading()).toBe(false);
  });

  // ─── filteredProducts ────────────────────────────────────────────────────────

  it('should return all products when searchTerm is empty', () => {
    component.searchTerm.set('');
    expect(component.filteredProducts()).toEqual(MOCK_PRODUCTS);
  });

  it('should filter products by nombre', () => {
    component.searchTerm.set('laptop');
    const result = component.filteredProducts();
    expect(result.length).toBe(1);
    expect(result[0].nombre).toBe('Laptop Gamer');
  });

  it('should filter products by categoriaNombre', () => {
    component.searchTerm.set('monitores');
    const result = component.filteredProducts();
    expect(result.length).toBe(1);
    expect(result[0].nombre).toBe('Monitor 4K');
  });

  it('should return empty array when no product matches searchTerm', () => {
    component.searchTerm.set('xbox');
    expect(component.filteredProducts()).toEqual([]);
  });

  it('should be case-insensitive when filtering', () => {
    component.searchTerm.set('LAPTOP');
    expect(component.filteredProducts().length).toBe(1);
  });

  // ─── openCreate ──────────────────────────────────────────────────────────────

  it('should open dialog when openCreate is called', () => {
    component.openCreate();
    expect(dialog.open).toHaveBeenCalled();
  });

  it('should reload products when dialog closes with result', () => {
    (dialog.open as jest.Mock).mockReturnValue({
      afterClosed: jest.fn().mockReturnValue(of(true)),
    });
    const callsBefore = adminService.getAlladmin.mock.calls.length;
    component.openCreate();
    expect(adminService.getAlladmin.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  it('should not reload products when dialog closes without result', () => {
    (dialog.open as jest.Mock).mockReturnValue({
      afterClosed: jest.fn().mockReturnValue(of(null)),
    });
    const callsBefore = adminService.getAlladmin.mock.calls.length;
    component.openCreate();
    expect(adminService.getAlladmin.mock.calls.length).toBe(callsBefore);
  });

  // ─── openEdit ────────────────────────────────────────────────────────────────

  it('should open dialog with product data when openEdit is called', () => {
    component.openEdit(MOCK_PRODUCTS[0]);
    expect(dialog.open).toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ data: expect.objectContaining({ product: MOCK_PRODUCTS[0] }) })
    );
  });

  it('should reload products when edit dialog closes with result', () => {
    (dialog.open as jest.Mock).mockReturnValue({
      afterClosed: jest.fn().mockReturnValue(of(true)),
    });
    const callsBefore = adminService.getAlladmin.mock.calls.length;
    component.openEdit(MOCK_PRODUCTS[0]);
    expect(adminService.getAlladmin.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  // ─── confirmDelete ────────────────────────────────────────────────────────────

  it('should not call delete when user cancels confirm dialog', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(false);
    component.confirmDelete(MOCK_PRODUCTS[0]);
    expect(adminService.delete).not.toHaveBeenCalled();
  });

  it('should call delete with product id when user confirms', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    component.confirmDelete(MOCK_PRODUCTS[0]);
    expect(adminService.delete).toHaveBeenCalledWith(1);
  });

  it('should show success snackbar after delete', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    component.confirmDelete(MOCK_PRODUCTS[0]);
    expect(snackBar.open).toHaveBeenCalledWith('Producto eliminado', 'OK', { duration: 2500 });
  });

  it('should reload after successful delete', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    const callsBefore = adminService.getAlladmin.mock.calls.length;
    component.confirmDelete(MOCK_PRODUCTS[0]);
    expect(adminService.getAlladmin.mock.calls.length).toBeGreaterThan(callsBefore);
  });

  it('should show error snackbar when delete fails', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    adminService.delete.mockReturnValue(throwError(() => new Error('fail')));
    component.confirmDelete(MOCK_PRODUCTS[0]);
    expect(snackBar.open).toHaveBeenCalledWith('Error al eliminar', 'OK', { duration: 3000 });
  });

  // ─── formatPrice ─────────────────────────────────────────────────────────────

  it('should format price in COP currency', () => {
    const formatted = component.formatPrice(3000000);
    expect(formatted).toContain('3.000.000');
  });

  it('should format price without decimal places', () => {
    const formatted = component.formatPrice(1500000);
    expect(formatted).not.toContain(',00');
  });
});