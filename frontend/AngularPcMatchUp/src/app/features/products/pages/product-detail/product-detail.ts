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
@Component({
  selector: 'app-product-detail',
  imports: [CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatTooltipModule,
    MatSnackBarModule,],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
[x: string]: any;
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsApiService = inject(ProductsApiService);
  private readonly snackBar = inject(MatSnackBar);
  
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
        this.error.set('No se encontró el producto o ocurrió un error.');
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
    return new Intl.DateTimeFormat('es-CO', {
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
    if (stock === 0) return 'Agotado';
    if (stock <= 5) return `Solo quedan ${stock} unidades`;
    return `${stock} unidades disponibles`;
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
  // pendiente
}
}
