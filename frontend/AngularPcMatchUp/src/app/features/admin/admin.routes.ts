import { Routes } from '@angular/router';
import { adminGuard } from '../../core/guards/admin-guard';
import { AdminLayout} from './layout/admin-layout/admin-layout';
 
export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayout,
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard').then(
            (m) => m.Dashboard
          ),
        title: 'Dashboard',
      },
      {
        path: 'productos',
        loadComponent: () =>
          import('./pages/admin-products/admin-products').then(
            (m) => m.AdminProductsComponent
          ),
        title: 'Gestión de Productos',
      },
      {
        path: 'categorias',
        loadComponent: () =>
          import('./pages/admin-categories/admin-categories').then(
            (m) => m.AdminCategoriesComponent
          ),
        title: 'Gestión de Categorías',
      },
      // Slots listos para agregar sin tocar el layout:
      // { path: 'pedidos',    loadComponent: () => import('./pages/orders/...') },
      // { path: 'usuarios',   loadComponent: () => import('./pages/users/...') },
      // { path: 'reportes',   loadComponent: () => import('./pages/reports/...') },
    ],
  },
];