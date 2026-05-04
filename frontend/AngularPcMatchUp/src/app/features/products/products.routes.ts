// features/products/products.routes.ts
 
import { Routes } from '@angular/router';
 
export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/product-catalog/product-catalog').then(
        (m) => m.ProductCatalog
      ),
    title: 'Catálogo de Productos',
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/product-detail/product-detail').then(
        (m) => m.ProductDetail
      ),
    title: 'Detalle del Producto',
  },
];
 