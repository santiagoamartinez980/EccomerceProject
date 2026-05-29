import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth-guard';

export const ADDRESS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/address-list/address-list').then(m => m.AddressList),
    title: 'Mis Direcciones',
  },
  {
    path: 'nueva',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/address-form/address-form').then(m => m.AddressForm),
    title: 'Nueva Dirección',
  },
  {
    path: 'editar/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/address-form/address-form').then(m => m.AddressForm),
    title: 'Editar Dirección',
  },
];
