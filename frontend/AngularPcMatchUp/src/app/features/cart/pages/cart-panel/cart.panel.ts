// features/cart/components/add-to-cart-panel/add-to-cart-panel.component.ts
import { Component, Input, input, output, signal,OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { trigger, transition, style, animate } from '@angular/animations';


export interface AddedProduct {
  nombre: string;
  imagenUrl?: string;
  quantity: number;
  precio: number;
}

@Component({
  selector: 'cart-panel',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(110%)', opacity: 0 }),
        animate('300ms cubic-bezier(0.4,0,0.2,1)',
          style({ transform: 'translateX(0)', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('250ms cubic-bezier(0.4,0,0.2,1)',
          style({ transform: 'translateX(110%)', opacity: 0 })),
      ]),
    ]),
  ],
  template: `
    @if (visible()) {
      <div class="panel-backdrop" (click)="close()" @slideIn></div>
      <div class="panel" @slideIn>

        <!-- Cerrar -->
        <button class="close-btn" (click)="close()">
          <mat-icon>close</mat-icon>
        </button>

        <!-- Icono éxito + imagen -->
        <div class="panel-hero">
          <div class="product-thumb">
            @if (product()?.imagenUrl) {
              <img [src]="product()!.imagenUrl" [alt]="product()!.nombre" />
            } @else {
              <mat-icon>inventory_2</mat-icon>
            }
            <div class="success-badge">
              <mat-icon>check</mat-icon>
            </div>
          </div>
        </div>

        <!-- Info -->
        <div class="panel-body">
          <p class="panel-title">¡Agregado al carrito!</p>
          <p class="panel-product">{{ product()?.nombre }}</p>
          <p class="panel-qty">{{ product()?.quantity }} {{ product()?.quantity === 1 ? 'unidad' : 'unidades' }}</p>
          <p class="panel-price">{{ formatPrice(product()?.precio ?? 0) }}</p>
        </div>

        <!-- Acciones -->
        <div class="panel-actions">
          <button class="btn-catalog" (click)="close()">
            Seguir comprando
          </button>
          <button class="btn-cart" (click)="goToCart()">
            <mat-icon>shopping_bag</mat-icon>
            Ir al carrito
          </button>
        </div>

      </div>
    }
  `,
  styles: [`
    .panel-backdrop {
      position: fixed; inset: 0;
      background: rgba(0,0,0,0.25);
      z-index: 999;
    }

    .panel {
      position: fixed;
      top: 80px;
      right: 20px;
      width: 300px;
      background: white;
      border-radius: var(--radius-md, 12px);
      box-shadow: 0 20px 60px rgba(0,0,0,0.18);
      z-index: 1000;
      padding: 24px 20px 20px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .close-btn {
      position: absolute;
      top: 12px; right: 12px;
      width: 28px; height: 28px;
      border-radius: 50%; border: none;
      background: #f1f5f9; cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      color: #64748b; transition: background 0.15s;
    }
    .close-btn:hover { background: #e2e8f0; }
    .close-btn mat-icon { font-size: 16px; width: 16px; height: 16px; }

    /* Hero */
    .panel-hero { display: flex; justify-content: center; }

    .product-thumb {
      position: relative;
      width: 90px; height: 90px;
      border-radius: var(--radius-sm, 8px);
      overflow: visible;
      background: var(--color-bg-secondary, #f7f5f2);
      display: flex; align-items: center; justify-content: center;
    }

    .product-thumb img {
      width: 90px; height: 90px;
      object-fit: cover;
      border-radius: var(--radius-sm, 8px);
    }

    .product-thumb mat-icon {
      font-size: 36px; width: 36px; height: 36px;
      color: #94a3b8;
    }

    .success-badge {
      position: absolute;
      bottom: -8px; right: -8px;
      width: 28px; height: 28px;
      border-radius: 50%;
      background: var(--color-accent, #69b46f);
      border: 2px solid white;
      display: flex; align-items: center; justify-content: center;
    }
    .success-badge mat-icon { font-size: 16px; width: 16px; height: 16px; color: white; }

    /* Body */
    .panel-body { text-align: center; display: flex; flex-direction: column; gap: 3px; }

    .panel-title {
      margin: 0; font-size: 1rem; font-weight: 700;
      color: var(--color-text-primary, #1a1a1a);
    }
    .panel-product {
      margin: 0; font-size: 0.85rem;
      color: var(--color-text-secondary, #888780);
      display: -webkit-box; -webkit-line-clamp: 2;
      -webkit-box-orient: vertical; overflow: hidden;
    }
    .panel-qty {
      margin: 0; font-size: 0.78rem;
      color: var(--color-text-secondary, #888780);
    }
    .panel-price {
      margin: 0; font-size: 1.1rem; font-weight: 800;
      color: var(--color-accent, #69b46f);
    }

    /* Actions */
    .panel-actions { display: flex; flex-direction: column; gap: 8px; }

    .btn-cart {
      display: flex; align-items: center; justify-content: center; gap: 8px;
      padding: 11px; border-radius: var(--radius-pill, 99px);
      border: none; background: var(--color-accent, #69b46f);
      color: white; font-size: 0.88rem; font-weight: 700;
      cursor: pointer; transition: background 0.2s;
    }
    .btn-cart:hover { background: var(--color-accent-hover, #1a8e52); }
    .btn-cart mat-icon { font-size: 18px; width: 18px; height: 18px; }

    .btn-catalog {
      padding: 10px; border-radius: var(--radius-pill, 99px);
      border: 1px solid var(--color-border, rgba(0,0,0,0.12));
      background: transparent; font-size: 0.85rem; font-weight: 600;
      color: var(--color-text-secondary, #888780);
      cursor: pointer; transition: border-color 0.2s, color 0.2s;
    }
    .btn-catalog:hover {
      border-color: var(--color-accent, #69b46f);
      color: var(--color-accent, #69b46f);
    }

    @media (max-width: 400px) {
      .panel { right: 10px; left: 10px; width: auto; }
    }
  `],
})
export class cartPanel {
  private readonly router = inject(Router);

  product = input<AddedProduct | null>(null);
  visible = input<boolean>(false);
  closed = output<void>();
  

  close(): void {
    this.closed.emit();
  }

  goToCart(): void {
    this.closed.emit();
    this.router.navigate(['/carrito']);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency', currency: 'COP', minimumFractionDigits: 0,
    }).format(price);
  }

  ngOnChanges(): void {
  if (this.visible()) {
    setTimeout(() => this.closed.emit(), 6000);
  }
  }
}

