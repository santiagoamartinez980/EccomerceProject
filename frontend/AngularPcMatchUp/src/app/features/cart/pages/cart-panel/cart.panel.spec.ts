import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

import { AddedProduct, cartPanel } from './cart.panel';

const mockProduct: AddedProduct = {
  nombre: 'Mouse Gamer',
  imagenUrl: 'https://example.com/mouse.jpg',
  quantity: 2,
  precio: 150000,
};

describe('cartPanel', () => {
  let component: cartPanel;
  let fixture: ComponentFixture<cartPanel>;
  let router: Router;

  beforeEach(() => {
    jest.useFakeTimers();

    TestBed.configureTestingModule({
      imports: [cartPanel],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    });

    fixture = TestBed.createComponent(cartPanel);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.useRealTimers();
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── Inputs por defecto ─────────────────────────────────────────────────────

  it('should have visible as false by default', () => {
    expect(component.visible()).toBe(false);
  });

  it('should have product as null by default', () => {
    expect(component.product()).toBeNull();
  });

  // ─── close() ────────────────────────────────────────────────────────────────

  it('should emit closed when close() is called', () => {
    let emitted = false;
    component.closed.subscribe(() => (emitted = true));

    component.close();

    expect(emitted).toBe(true);
  });

  // ─── goToCart() ─────────────────────────────────────────────────────────────

  it('should emit closed and navigate to /carrito when goToCart() is called', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);
    let emitted = false;
    component.closed.subscribe(() => (emitted = true));

    component.goToCart();

    expect(emitted).toBe(true);
    expect(router.navigate).toHaveBeenCalledWith(['/carrito']);
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

  // ─── ngOnChanges() / auto-close ─────────────────────────────────────────────

  it('should emit closed after 6 seconds when visible becomes true', () => {
    let emitted = false;
    component.closed.subscribe(() => (emitted = true));

    // Simular que visible() retorna true y disparar ngOnChanges manualmente
    jest.spyOn(component, 'visible').mockReturnValue(true);
    component.ngOnChanges();

    expect(emitted).toBe(false);

    jest.advanceTimersByTime(6000);

    expect(emitted).toBe(true);
  });

  it('should not emit closed before 6 seconds have passed', () => {
    let emitted = false;
    component.closed.subscribe(() => (emitted = true));

    jest.spyOn(component, 'visible').mockReturnValue(true);
    component.ngOnChanges();

    jest.advanceTimersByTime(5999);

    expect(emitted).toBe(false);
  });

  it('should not auto-emit closed when visible is false', () => {
    let emitted = false;
    component.closed.subscribe(() => (emitted = true));

    jest.spyOn(component, 'visible').mockReturnValue(false);
    component.ngOnChanges();

    jest.advanceTimersByTime(6000);

    expect(emitted).toBe(false);
  });
});