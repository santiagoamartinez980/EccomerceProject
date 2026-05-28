// features/cart/pages/cart/cart.component.ts
// features/cart/pages/cart/cart.component.ts
import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { CartService } from '../../services/cart.service';
import { CartDto } from '../../interfaces/cart.dto';
import { CartItemDto } from '../../interfaces/cart-item.dto';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './cart.html',
  styleUrls: ['./cart.css'],
})
export class Cart implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  cart = signal<CartDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  updatingItemId = signal<number | null>(null);
  clearingCart = signal(false);

  // Set de productIds seleccionados
  selectedIds = signal<Set<number>>(new Set());

  itemCount = computed(() => this.cart()?.items.length ?? 0);
  isEmpty = computed(() => this.itemCount() === 0);

  selectedItems = computed(() =>
    (this.cart()?.items ?? []).filter(i => this.selectedIds().has(i.productId))
  );

  allSelected = computed(() => {
    const items = this.cart()?.items ?? [];
    return items.length > 0 && items.every(i => this.selectedIds().has(i.productId));
  });

  selectedTotal = computed(() =>
    this.selectedItems().reduce((sum, i) => sum + i.subtotal, 0)
  );

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading.set(true);
    this.error.set(null);
    this.cartService.getCart().subscribe({
      next: (data) => {
        this.cart.set(data);
        // Seleccionar todos por defecto
        this.selectedIds.set(new Set(data.items.map(i => i.productId)));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el carrito.');
        this.loading.set(false);
      },
    });
  }

  // ── Selección ─────────────────────────────────────────────────────────────
  toggleSelect(productId: number): void {
    const current = new Set(this.selectedIds());
    if (current.has(productId)) {
      current.delete(productId);
    } else {
      current.add(productId);
    }
    this.selectedIds.set(current);
  }

  toggleSelectAll(): void {
    const items = this.cart()?.items ?? [];
    if (this.allSelected()) {
      this.selectedIds.set(new Set());
    } else {
      this.selectedIds.set(new Set(items.map(i => i.productId)));
    }
  }

  isSelected(productId: number): boolean {
    return this.selectedIds().has(productId);
  }

  // ── Cantidad ──────────────────────────────────────────────────────────────
  updateQuantity(item: CartItemDto, newQty: number): void {
    if (newQty < 1 || newQty > item.stock) return;
    this.updatingItemId.set(item.productId);
    this.cartService.addOrUpdateItem({ productId: item.productId, quantity: newQty }).subscribe({
      next: (data) => {
        this.cart.set(data);
        // mantener selección
        const current = new Set(this.selectedIds());
        this.selectedIds.set(current);
        this.updatingItemId.set(null);
      },
      error: () => {
        this.snack.open('Error al actualizar cantidad', 'OK', { duration: 3000, panelClass: 'snack-error' });
        this.updatingItemId.set(null);
      },
    });
  }

  // ── Eliminar ──────────────────────────────────────────────────────────────
  removeItem(item: CartItemDto): void {
    this.updatingItemId.set(item.productId);
    this.cartService.removeItem(item.productId).subscribe({
      next: (data) => {
        // quitar de seleccionados
        const current = new Set(this.selectedIds());
        current.delete(item.productId);
        this.selectedIds.set(current);
        this.cart.set(data);
        this.updatingItemId.set(null);
        this.snack.open('Producto eliminado del carrito', 'OK', { duration: 2000 });
      },
      error: () => {
        this.snack.open('Error al eliminar producto', 'OK', { duration: 3000, panelClass: 'snack-error' });
        this.updatingItemId.set(null);
      },
    });
  }

  clearCart(): void {
    if (!confirm('¿Vaciar todo el carrito?')) return;
    this.clearingCart.set(true);
    this.cartService.clearCart().subscribe({
      next: () => {
        this.selectedIds.set(new Set());
        this.loadCart();
        this.clearingCart.set(false);
        this.snack.open('Carrito vaciado', 'OK', { duration: 2000 });
      },
      error: () => {
        this.snack.open('Error al vaciar el carrito', 'OK', { duration: 3000, panelClass: 'snack-error' });
        this.clearingCart.set(false);
      },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  goToProducts(): void {
    this.router.navigate(['/productos']);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency', currency: 'COP', minimumFractionDigits: 0,
    }).format(price);
  }

  isUpdating(productId: number): boolean {
    return this.updatingItemId() === productId;
  }

  trackByItem(_: number, item: CartItemDto): number {
    return item.cartItemId;
  }
}