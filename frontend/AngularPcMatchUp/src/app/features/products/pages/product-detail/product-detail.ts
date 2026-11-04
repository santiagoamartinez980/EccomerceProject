import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
 

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
 
import { ProductInterface } from '../../interfaces/product.interface';
import { ProductsApiService } from '../../services/products-api.service';
import { CartService } from '../../../cart/services/cart.service';
import { TokenService } from '../../../../core/services/token.service';
import { AddedProduct, cartPanel } from '../../../cart/pages/cart-panel/cart.panel';
import { I18nService } from '../../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatTooltipModule,
    MatSnackBarModule,
    cartPanel,
    TranslatePipe],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
[x: string]: any;
  private readonly cartService = inject(CartService);
  private readonly tokenService = inject(TokenService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsApiService = inject(ProductsApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly i18n = inject(I18nService);

  addingToCart = signal(false);
  addedProduct = signal<AddedProduct | null>(null);
  showPanel = signal(false);
  isZoomed = signal(false);
  zoomOrigin = signal('center center');
 
  product = signal<ProductInterface | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  imageError = signal(false);
 
  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id || isNaN(id)) {
      this.goBack();
      return;
    }
    this.loadProduct(id);
  }
 
  private loadProduct(id: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.productsApiService.getByIdPublic(id).subscribe({
      next: (data) => {
        this.product.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(this.i18n.t('detail.errorLoad'));
        this.loading.set(false);
      },
    });
  }
 
  goBack(): void {
    this.router.navigate(['/productos']);
  }
 
  onImageError(): void {
    this.imageError.set(true);
  }
 
  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(price);
  }
 
  formatDate(date: string): string {
    return new Intl.DateTimeFormat(this.i18n.locale, {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
    }).format(new Date(date));
  }
 
  getStockLevel(stock: number): 'ok' | 'low' | 'out' {
    if (stock === 0) return 'out';
    if (stock <= 5) return 'low';
    return 'ok';
  }
 
  getStockText(stock: number): string {
    if (stock === 0) return this.i18n.t('stock.soldOut');
    if (stock <= 5) return this.i18n.t('detail.stockLow', { n: stock });
    return this.i18n.t('detail.stockOk', { n: stock });
  }

  onMouseMove(event: MouseEvent) {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
  const x = ((event.clientX - rect.left) / rect.width) * 100;
  const y = ((event.clientY - rect.top) / rect.height) * 100;
  this.zoomOrigin.set(`${x}% ${y}%`);
  this.isZoomed.set(true);
}

onMouseLeave() {
  this.isZoomed.set(false);
}

// Cantidad
quantity = signal(1);

increaseQty() {
  if (this.quantity() < this.product()!.stock) {
    this.quantity.update(q => q + 1);
  }
}

decreaseQty() {
  if (this.quantity() > 1) {
    this.quantity.update(q => q - 1);
  }
}

addToCart() {
 if (!this.tokenService.isLoggedIn()) {
    this.router.navigate(['/auth/login']);
    return;
  }
 
  const p = this.product();
  if (!p) return;
 
  this.addingToCart.set(true);
  this.cartService.addOrUpdateItem({
    productId: p.idProducto,
    quantity: this.quantity(),
  }).subscribe({
    next: () => {
      this.addedProduct.set({
        nombre: p.nombre,
        imagenUrl: p.imagenUrl,
        quantity: this.quantity(),
        precio: p.precio * this.quantity(),
      });
      this.showPanel.set(true);
      this.addingToCart.set(false);
    },
    error: () => {
      this.snackBar.open(this.i18n.t('detail.errorAdd'), this.i18n.t('common.ok'), {
        duration: 3000,
        panelClass: 'snack-error',
      });
      this.addingToCart.set(false);
    },
  });
}
}
