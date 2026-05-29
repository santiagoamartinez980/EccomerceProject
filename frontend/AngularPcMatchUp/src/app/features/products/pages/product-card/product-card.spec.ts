import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';

import { ProductCard } from './product-card';
import { ProductInterface } from '../../interfaces/product.interface';

const mockProduct: ProductInterface = {
  idProducto: 1,
  nombre: 'Mouse Gamer',
  descripcion: 'Mouse de alta precisión',
  precio: 150000,
  stock: 10,
  imagenUrl: 'https://example.com/mouse.jpg',
  activo: true,
  fechaCreacion: '2024-01-01T00:00:00Z',
  idCategoria: 2,
  categoriaNombre: 'Periféricos',
};

describe('ProductCard', () => {
  let component: ProductCard;
  let fixture: ComponentFixture<ProductCard>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ProductCard],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    });

    fixture = TestBed.createComponent(ProductCard);
    component = fixture.componentInstance;
    // Reemplazar la señal requerida con una señal mutable que apunte al mock
    (component as any).product = signal(mockProduct);
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── product input ──────────────────────────────────────────────────────────

  it('should expose the product from the input signal', () => {
    expect(component.product()).toEqual(mockProduct);
  });

  // ─── onClick() ──────────────────────────────────────────────────────────────

  it('should emit the product through cardClick when onClick() is called', () => {
    let emitted: ProductInterface | undefined;
    component.cardClick.subscribe(p => (emitted = p));

    component.onClick();

    expect(emitted).toEqual(mockProduct);
  });

  it('should emit the current product value at the time of the click', () => {
    const otherProduct: ProductInterface = { ...mockProduct, idProducto: 2, nombre: 'Teclado' };
    (component as any).product = signal(otherProduct);

    let emitted: ProductInterface | undefined;
    component.cardClick.subscribe(p => (emitted = p));
    component.onClick();

    expect(emitted).toEqual(otherProduct);
  });

  // ─── formatPrice() ──────────────────────────────────────────────────────────

  it('should format a price as COP currency', () => {
    const result = component.formatPrice(150000);
    expect(result).toContain('150');
    expect(result).toMatch(/COP|\$|150\.000/);
  });

  it('should format zero correctly', () => {
    const result = component.formatPrice(0);
    expect(result).toMatch(/0/);
  });

  // ─── getStockLabel() ────────────────────────────────────────────────────────

  it('should return "Agotado" when stock is 0', () => {
    expect(component.getStockLabel(0)).toBe('Agotado');
  });

  it('should return "Últimas N" when stock is between 1 and 5', () => {
    expect(component.getStockLabel(1)).toBe('Últimas 1');
    expect(component.getStockLabel(5)).toBe('Últimas 5');
  });

  it('should return "N disp." when stock is greater than 5', () => {
    expect(component.getStockLabel(6)).toBe('6 disp.');
    expect(component.getStockLabel(100)).toBe('100 disp.');
  });

  // ─── getStockLevel() ────────────────────────────────────────────────────────

  it('should return "out" when stock is 0', () => {
    expect(component.getStockLevel(0)).toBe('out');
  });

  it('should return "low" when stock is between 1 and 5', () => {
    expect(component.getStockLevel(1)).toBe('low');
    expect(component.getStockLevel(5)).toBe('low');
  });

  it('should return "ok" when stock is greater than 5', () => {
    expect(component.getStockLevel(6)).toBe('ok');
    expect(component.getStockLevel(50)).toBe('ok');
  });
});