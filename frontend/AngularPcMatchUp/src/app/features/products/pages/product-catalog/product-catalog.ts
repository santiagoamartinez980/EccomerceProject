
import {Component,OnInit,OnDestroy,signal,computed,inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

import { ProductInterface } from '../../interfaces/product.interface';
import {ProductsApiService} from '../../services/products-api.service';
import {ProductsQueryService} from '../../services/products-query.service';
import { ProductCard } from '../product-card/product-card';
@Component({
  selector: 'app-product-catalog',
  imports: [CommonModule,
    FormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule, ProductCard],
  templateUrl: './product-catalog.html',
  styleUrl: './product-catalog.css',
  
})
export class ProductCatalog implements OnInit ,OnDestroy{
  private readonly productsApiService = inject(ProductsApiService);
  private readonly productsQueryService = inject(ProductsQueryService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();
  private readonly searchSubject = new Subject<string>();
 
  products = signal<ProductInterface[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  searchTerm = signal('');
  selectedCategory = signal<string>('');
  viewMode = signal<'grid' | 'list'>('grid');
 
  categories = computed(() => {
    const productos = this.products();
    console.log('Productos completos:', JSON.parse(JSON.stringify(productos))); // Debug profundo
    const cats = this.products().map((p) => p.categoriaNombre?? '').filter(Boolean);
    
    return [...new Set(cats)];
  });
 
  filteredProducts = computed(() => {
    const cat = this.selectedCategory();
    return cat ? this.products().filter((p) => p.categoriaNombre === cat) : this.products();
  });
 
  ngOnInit(): void {
    this.loadProducts();
    this.searchSubject
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => (term.trim() ? this.searchProducts(term) : this.loadProducts()));
  }
 
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
 
  loadProducts(): void {
    this.loading.set(true);
    this.error.set(null);
    this.productsApiService.getAll().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.products.set(data); this.loading.set(false); },
      error: () => { this.error.set('No se pudieron cargar los productos.'); this.loading.set(false); },
    });
  }
 
  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.selectedCategory.set('');
    this.searchSubject.next(value);
  }
 
  private searchProducts(name: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.productsQueryService.search(name).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.products.set(data); this.loading.set(false); },
      error: () => { this.error.set('Error al buscar productos.'); this.loading.set(false); },
    });
  }
 
  filterByCategory(category: string): void {
    this.selectedCategory.set(category === this.selectedCategory() ? '' : category);
  }
 
  clearFilters(): void {
    this.searchTerm.set('');
    this.selectedCategory.set('');
    this.loadProducts();
  }
 
  onCardClick(product: ProductInterface): void {
    this.router.navigate(['/productos', product.idProducto]);
  }
 
  toggleViewMode(): void {
    this.viewMode.set(this.viewMode() === 'grid' ? 'list' : 'grid');
  }
 
  trackById(_: number, p: ProductInterface): number {
    return p.idProducto;
  }

}
