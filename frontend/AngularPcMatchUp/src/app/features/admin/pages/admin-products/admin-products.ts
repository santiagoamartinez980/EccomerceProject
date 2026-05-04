// features/admin/pages/products/admin-products.component.ts
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';

import { ProductsAdminService } from '../../../products/services/products-admin.service';
import { ProductsApiService } from '../../../products/services/products-api.service';
import { AdminCategoriesService } from '../../../categories/services/category.service';
import {ProductInterface} from '../../../products/interfaces/product.interface';
import { CategoryInterface } from '../../../categories/interfaces/category.interface';
import { AdminProductForm } from './admin-product.form';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatDialogModule,
    MatSnackBarModule, MatProgressSpinnerModule, MatSelectModule,
    MatSlideToggleModule, MatTooltipModule, MatChipsModule,
  ],
  templateUrl: './admin-products.html',
  styleUrls: ['./admin-products.css'],
})
export class AdminProductsComponent implements OnInit {
  private readonly productsService = inject(ProductsApiService);
  private readonly adminProductsService = inject(ProductsAdminService);
  private readonly categoriesService = inject(AdminCategoriesService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  products = signal<ProductInterface[]>([]);
  categories = signal<CategoryInterface[]>([]);
  loading = signal(false);
  searchTerm = signal('');

  displayedColumns = ['imagen', 'nombre', 'categoria', 'precio', 'stock', 'activo', 'acciones'];

  filteredProducts() {
    const term = this.searchTerm().toLowerCase();
    return term
      ? this.products().filter(
          (p) =>
            p.nombre.toLowerCase().includes(term) ||
            (p.categoriaNombre ?? '').toLowerCase().includes(term)
        )
      : this.products();
  }

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading.set(true);
    this.productsService.getAll().subscribe({
      next: (data) => { this.products.set(data); this.loading.set(false); },
      error: () => { this.snack.open('Error al cargar productos', 'OK', { duration: 3000 }); this.loading.set(false); },
    });
    this.categoriesService.getAll().subscribe({
      next: (data) => this.categories.set(data),
    });
  }

  openCreate(): void {
    const ref = this.dialog.open(AdminProductForm, {
      width: '560px',
      data: { product: null, categories: this.categories() },
    });
    ref.afterClosed().subscribe((result) => {
      if (result) this.loadAll();
    });
  }

  openEdit(product: ProductInterface): void {
    const ref = this.dialog.open(AdminProductForm, {
      width: '560px',
      data: { product, categories: this.categories() },
    });
    ref.afterClosed().subscribe((result) => {
      if (result) this.loadAll();
    });
  }

  confirmDelete(product: ProductInterface): void {
    if (!confirm(`¿Eliminar "${product.nombre}"? Esta acción no se puede deshacer.`)) return;
    this.adminProductsService.delete(product.idProducto).subscribe({
      next: () => {
        this.snack.open('Producto eliminado', 'OK', { duration: 2500 });
        this.loadAll();
      },
      error: () => this.snack.open('Error al eliminar', 'OK', { duration: 3000 }),
    });
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency', currency: 'COP', minimumFractionDigits: 0,
    }).format(price);
  }
}