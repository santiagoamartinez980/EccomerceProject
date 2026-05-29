import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Login } from './features/auth/pages/login/login';
import { Register } from './features/auth/pages/register/register';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'productos',
    pathMatch: 'full'
  },

  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  {
    path: 'productos',
    loadChildren: () =>
      import('./features/products/products.routes').then(
        m => m.PRODUCTS_ROUTES
      ),
  },

  {
  path: 'carrito',
  loadChildren: () =>
    import('./features/cart/cart.routes').then(m => m.CART_ROUTES),
  },

  {
  path: 'direcciones',
  loadChildren: () => import('./features/address/address.routes').then(m => m.ADDRESS_ROUTES)
  },

  {
  path: 'checkout',
  canActivate: [authGuard],
  loadComponent: () => import('./features/checkout/checkout/checkout').then(m => m.Checkout),
  title: 'Checkout'
  },

  {
    path: 'admin',
    loadChildren: () =>
      import('./features/admin/admin.routes').then(
        m => m.ADMIN_ROUTES
      ),
  },

  {
    path: '**',
    redirectTo: 'productos',
  },
];


