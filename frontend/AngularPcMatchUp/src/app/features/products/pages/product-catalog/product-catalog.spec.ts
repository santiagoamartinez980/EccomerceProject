import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { ProductCatalog } from './product-catalog';
import { ProductsApiService } from '../../services/products-api.service';
import { ProductsQueryService } from '../../services/products-query.service';
import { ProductInterface } from '../../interfaces/product.interface';

const makeProduct = (id: number, cat?: string): ProductInterface => ({
  idProducto: id,
  nombre: `Producto ${id}`,
  precio: 100000,
  stock: 10,
  activo: true,
  fechaCreacion: '2024-01-01T00:00:00Z',
  idCategoria: 1,
  categoriaNombre: cat ?? 'Periféricos',
});

const p1 = makeProduct(1, 'Periféricos');
const p2 = makeProduct(2, 'Monitores');
const p3 = makeProduct(3, 'Periféricos');

describe('ProductCatalog', () => {
  let component: ProductCatalog;
  let fixture: ComponentFixture<ProductCatalog>;
  let productsApiService: ProductsApiService;
  let productsQueryService: ProductsQueryService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ProductCatalog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    });

    fixture = TestBed.createComponent(ProductCatalog);
    component = fixture.componentInstance;
    productsApiService = TestBed.inject(ProductsApiService);
    productsQueryService = TestBed.inject(ProductsQueryService);
    router = TestBed.inject(Router);

    jest.spyOn(productsApiService, 'getAll').mockReturnValue(of([p1, p2, p3]));
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── ngOnInit / loadProducts() ──────────────────────────────────────────────

  it('should call productsApiService.getAll on init', () => {
    expect(productsApiService.getAll).toHaveBeenCalled();
  });

  it('should populate products signal after loading', () => {
    expect(component.products()).toEqual([p1, p2, p3]);
    expect(component.loading()).toBe(false);
  });

  it('should set error signal when loadProducts fails', () => {
    jest.spyOn(productsApiService, 'getAll').mockReturnValue(throwError(() => new Error()));
    component.loadProducts();
    expect(component.error()).toBe('No se pudieron cargar los productos.');
    expect(component.loading()).toBe(false);
  });

  // ─── computed: categories ────────────────────────────────────────────────────

  it('should compute unique categories from products', () => {
    expect(component.categories()).toEqual(['Periféricos', 'Monitores']);
  });

  it('should return empty categories when products list is empty', () => {
    component.products.set([]);
    expect(component.categories()).toEqual([]);
  });

  // ─── computed: filteredProducts ──────────────────────────────────────────────

  it('should return all products when no category is selected', () => {
    expect(component.filteredProducts()).toEqual([p1, p2, p3]);
  });

  it('should filter products by selected category', () => {
    component.selectedCategory.set('Periféricos');
    expect(component.filteredProducts()).toEqual([p1, p3]);
  });

  it('should return all products when selected category is cleared', () => {
    component.selectedCategory.set('Periféricos');
    component.selectedCategory.set('');
    expect(component.filteredProducts()).toEqual([p1, p2, p3]);
  });

  // ─── filterByCategory() ─────────────────────────────────────────────────────

  it('should set selectedCategory when a new category is selected', () => {
    component.filterByCategory('Monitores');
    expect(component.selectedCategory()).toBe('Monitores');
  });

  it('should clear selectedCategory when the same category is selected again', () => {
    component.selectedCategory.set('Monitores');
    component.filterByCategory('Monitores');
    expect(component.selectedCategory()).toBe('');
  });

  // ─── onSearchInput() ────────────────────────────────────────────────────────

  it('should update searchTerm and clear selectedCategory on input', () => {
    component.selectedCategory.set('Monitores');
    component.onSearchInput('mouse');
    expect(component.searchTerm()).toBe('mouse');
    expect(component.selectedCategory()).toBe('');
  });

  it('should call productsQueryService.search after debounce', fakeAsync(() => {
    jest.spyOn(productsQueryService, 'search').mockReturnValue(of([p1]));
    component.onSearchInput('mouse');
    tick(400);
    expect(productsQueryService.search).toHaveBeenCalledWith('mouse');
  }));

  it('should call loadProducts when search term is cleared after debounce', fakeAsync(() => {
    jest.spyOn(productsApiService, 'getAll').mockReturnValue(of([p1, p2, p3]));
    component.onSearchInput('');
    tick(400);
    expect(productsApiService.getAll).toHaveBeenCalled();
  }));

  it('should set error signal when search fails', fakeAsync(() => {
    jest.spyOn(productsQueryService, 'search').mockReturnValue(throwError(() => new Error()));
    component.onSearchInput('mouse');
    tick(400);
    expect(component.error()).toBe('Error al buscar productos.');
    expect(component.loading()).toBe(false);
  }));

  // ─── clearFilters() ─────────────────────────────────────────────────────────

  it('should reset searchTerm, selectedCategory and reload products', () => {
    component.searchTerm.set('mouse');
    component.selectedCategory.set('Monitores');
    jest.spyOn(productsApiService, 'getAll').mockReturnValue(of([p1, p2, p3]));

    component.clearFilters();

    expect(component.searchTerm()).toBe('');
    expect(component.selectedCategory()).toBe('');
    expect(productsApiService.getAll).toHaveBeenCalled();
  });

  // ─── onCardClick() ──────────────────────────────────────────────────────────

  it('should navigate to /productos/:id when a card is clicked', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);
    component.onCardClick(p1);
    expect(router.navigate).toHaveBeenCalledWith(['/productos', 1]);
  });

  // ─── toggleViewMode() ───────────────────────────────────────────────────────

  it('should toggle viewMode from grid to list', () => {
    expect(component.viewMode()).toBe('grid');
    component.toggleViewMode();
    expect(component.viewMode()).toBe('list');
  });

  it('should toggle viewMode from list back to grid', () => {
    component.viewMode.set('list');
    component.toggleViewMode();
    expect(component.viewMode()).toBe('grid');
  });

  // ─── trackById() ────────────────────────────────────────────────────────────

  it('should return idProducto as the tracking key', () => {
    expect(component.trackById(0, p1)).toBe(1);
  });

  // ─── ngOnDestroy() ──────────────────────────────────────────────────────────

  it('should complete destroy$ on ngOnDestroy', () => {
    const destroySpy = jest.spyOn((component as any).destroy$, 'next');
    component.ngOnDestroy();
    expect(destroySpy).toHaveBeenCalled();
  });
});