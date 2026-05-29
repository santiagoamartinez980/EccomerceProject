import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { Cart } from './cart';
import { CartService } from '../../services/cart.service';
import { CartDto } from '../../interfaces/cart.dto';
import { CartItemDto } from '../../interfaces/cart-item.dto';

const mockItem = (productId: number, overrides: Partial<CartItemDto> = {}): CartItemDto => ({
  cartItemId: productId * 10,
  productId,
  productName: `Producto ${productId}`,
  unitPrice: 100000,
  quantity: 2,
  subtotal: 200000,
  stock: 10,
  imagenUrl: '',
  ...overrides,
});

const mockCart = (items: CartItemDto[] = []): CartDto => ({
  id: 1,
  userId: 1,
  isActive: true,
  items,
  total: items.reduce((s, i) => s + i.subtotal, 0),
});

describe('Cart', () => {
  let component: Cart;
  let fixture: ComponentFixture<Cart>;
  let cartService: CartService;
  let router: Router;
  let snackOpenSpy: jest.SpyInstance;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Cart],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    fixture = TestBed.createComponent(Cart);
    component = fixture.componentInstance;
    cartService = TestBed.inject(CartService);
    router = TestBed.inject(Router);

    const snack = (component as any)['snack'] as MatSnackBar;
    snackOpenSpy = jest.spyOn(snack, 'open').mockReturnValue({} as any);

    // Default: carrito con dos items
    jest.spyOn(cartService, 'getCart').mockReturnValue(
      of(mockCart([mockItem(1), mockItem(2)]))
    );

    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── loadCart() ─────────────────────────────────────────────────────────────

  it('should load the cart on init', () => {
    expect(cartService.getCart).toHaveBeenCalled();
    expect(component.cart()).not.toBeNull();
    expect(component.loading()).toBe(false);
  });

  it('should select all items by default after loading', () => {
    expect(component.selectedIds().has(1)).toBe(true);
    expect(component.selectedIds().has(2)).toBe(true);
  });

  it('should set error signal when loadCart fails', () => {
    jest.spyOn(cartService, 'getCart').mockReturnValue(throwError(() => new Error()));

    component.loadCart();

    expect(component.error()).toBe('No se pudo cargar el carrito.');
    expect(component.loading()).toBe(false);
  });

  // ─── Computed signals ───────────────────────────────────────────────────────

  it('should compute itemCount from cart items', () => {
    expect(component.itemCount()).toBe(2);
  });

  it('should compute isEmpty as false when there are items', () => {
    expect(component.isEmpty()).toBe(false);
  });

  it('should compute isEmpty as true when cart is empty', () => {
    jest.spyOn(cartService, 'getCart').mockReturnValue(of(mockCart([])));
    component.loadCart();
    expect(component.isEmpty()).toBe(true);
  });

  it('should compute selectedTotal from selected items subtotals', () => {
    // ambos items seleccionados, cada uno subtotal 200000
    expect(component.selectedTotal()).toBe(400000);
  });

  it('should compute allSelected as true when all items are selected', () => {
    expect(component.allSelected()).toBe(true);
  });

  // ─── toggleSelect() ─────────────────────────────────────────────────────────

  it('should deselect an item when it is already selected', () => {
    component.toggleSelect(1);
    expect(component.selectedIds().has(1)).toBe(false);
  });

  it('should select an item when it is not selected', () => {
    component.toggleSelect(1);           // deselect
    component.toggleSelect(1);           // select again
    expect(component.selectedIds().has(1)).toBe(true);
  });

  // ─── toggleSelectAll() ──────────────────────────────────────────────────────

  it('should deselect all items when all are selected', () => {
    component.toggleSelectAll();
    expect(component.selectedIds().size).toBe(0);
  });

  it('should select all items when none are selected', () => {
    component.selectedIds.set(new Set());
    component.toggleSelectAll();
    expect(component.selectedIds().size).toBe(2);
  });

  // ─── isSelected() ───────────────────────────────────────────────────────────

  it('should return true for a selected productId', () => {
    expect(component.isSelected(1)).toBe(true);
  });

  it('should return false for a non-selected productId', () => {
    component.toggleSelect(1);
    expect(component.isSelected(1)).toBe(false);
  });

  // ─── updateQuantity() ───────────────────────────────────────────────────────

  it('should not call cartService when new quantity is less than 1', () => {
    jest.spyOn(cartService, 'addOrUpdateItem');
    component.updateQuantity(mockItem(1), 0);
    expect(cartService.addOrUpdateItem).not.toHaveBeenCalled();
  });

  it('should not call cartService when new quantity exceeds stock', () => {
    jest.spyOn(cartService, 'addOrUpdateItem');
    component.updateQuantity(mockItem(1, { stock: 5 }), 6);
    expect(cartService.addOrUpdateItem).not.toHaveBeenCalled();
  });

  it('should call cartService.addOrUpdateItem with correct payload', () => {
    jest.spyOn(cartService, 'addOrUpdateItem').mockReturnValue(
      of(mockCart([mockItem(1, { quantity: 3, subtotal: 300000 }), mockItem(2)]))
    );
    component.updateQuantity(mockItem(1), 3);
    expect(cartService.addOrUpdateItem).toHaveBeenCalledWith({ productId: 1, quantity: 3 });
  });

  it('should update cart signal after successful updateQuantity', () => {
    const updatedCart = mockCart([mockItem(1, { quantity: 3, subtotal: 300000 }), mockItem(2)]);
    jest.spyOn(cartService, 'addOrUpdateItem').mockReturnValue(of(updatedCart));
    component.updateQuantity(mockItem(1), 3);
    expect(component.cart()).toEqual(updatedCart);
    expect(component.updatingItemId()).toBeNull();
  });

  it('should show error snackbar when updateQuantity fails', () => {
    jest.spyOn(cartService, 'addOrUpdateItem').mockReturnValue(throwError(() => new Error()));
    component.updateQuantity(mockItem(1), 3);
    expect(snackOpenSpy).toHaveBeenCalledWith(
      'Error al actualizar cantidad', 'OK', { duration: 3000, panelClass: 'snack-error' }
    );
    expect(component.updatingItemId()).toBeNull();
  });

  // ─── removeItem() ───────────────────────────────────────────────────────────

  it('should call cartService.removeItem with the correct productId', () => {
    jest.spyOn(cartService, 'removeItem').mockReturnValue(of(mockCart([mockItem(2)])));
    component.removeItem(mockItem(1));
    expect(cartService.removeItem).toHaveBeenCalledWith(1);
  });

  it('should remove the productId from selectedIds after removeItem succeeds', () => {
    jest.spyOn(cartService, 'removeItem').mockReturnValue(of(mockCart([mockItem(2)])));
    component.removeItem(mockItem(1));
    expect(component.selectedIds().has(1)).toBe(false);
  });

  it('should show success snackbar after removeItem succeeds', () => {
    jest.spyOn(cartService, 'removeItem').mockReturnValue(of(mockCart([mockItem(2)])));
    component.removeItem(mockItem(1));
    expect(snackOpenSpy).toHaveBeenCalledWith('Producto eliminado del carrito', 'OK', { duration: 2000 });
  });

  it('should show error snackbar when removeItem fails', () => {
    jest.spyOn(cartService, 'removeItem').mockReturnValue(throwError(() => new Error()));
    component.removeItem(mockItem(1));
    expect(snackOpenSpy).toHaveBeenCalledWith(
      'Error al eliminar producto', 'OK', { duration: 3000, panelClass: 'snack-error' }
    );
    expect(component.updatingItemId()).toBeNull();
  });

  // ─── clearCart() ────────────────────────────────────────────────────────────

  it('should not call cartService.clearCart when confirm is cancelled', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(false);
    jest.spyOn(cartService, 'clearCart');
    component.clearCart();
    expect(cartService.clearCart).not.toHaveBeenCalled();
  });

  it('should call cartService.clearCart when confirm is accepted', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    jest.spyOn(cartService, 'clearCart').mockReturnValue(of(void 0 as any));
    component.clearCart();
    expect(cartService.clearCart).toHaveBeenCalled();
  });

  it('should clear selectedIds and show snackbar after clearCart succeeds', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    jest.spyOn(cartService, 'clearCart').mockReturnValue(of(void 0 as any));
    jest.spyOn(cartService, 'getCart').mockReturnValue(of(mockCart([])));
    component.clearCart();
    expect(component.selectedIds().size).toBe(0);
    expect(snackOpenSpy).toHaveBeenCalledWith('Carrito vaciado', 'OK', { duration: 2000 });
  });

  it('should show error snackbar when clearCart fails', () => {
    jest.spyOn(window, 'confirm').mockReturnValue(true);
    jest.spyOn(cartService, 'clearCart').mockReturnValue(throwError(() => new Error()));
    component.clearCart();
    expect(snackOpenSpy).toHaveBeenCalledWith(
      'Error al vaciar el carrito', 'OK', { duration: 3000, panelClass: 'snack-error' }
    );
    expect(component.clearingCart()).toBe(false);
  });

  // ─── goToProducts() ─────────────────────────────────────────────────────────

  it('should navigate to /productos', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);
    component.goToProducts();
    expect(router.navigate).toHaveBeenCalledWith(['/productos']);
  });

  // ─── formatPrice() ──────────────────────────────────────────────────────────

  it('should format a price in COP currency', () => {
    const result = component.formatPrice(150000);
    expect(result).toContain('150');
    expect(result).toMatch(/COP|\$|150\.000/);
  });

  // ─── isUpdating() ───────────────────────────────────────────────────────────

  it('should return true when updatingItemId matches the given productId', () => {
    component.updatingItemId.set(1);
    expect(component.isUpdating(1)).toBe(true);
  });

  it('should return false when updatingItemId does not match', () => {
    component.updatingItemId.set(2);
    expect(component.isUpdating(1)).toBe(false);
  });

  // ─── trackByItem() ──────────────────────────────────────────────────────────

  it('should return cartItemId as the tracking key', () => {
    expect(component.trackByItem(0, mockItem(1))).toBe(10);
  });
});