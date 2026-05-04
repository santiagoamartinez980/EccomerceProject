// features/admin/layout/admin-layout.component.ts
import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { jwtDecode } from 'jwt-decode';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  active?: boolean;
}

interface JwtPayload {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  email?: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    RouterOutlet,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatDividerModule,
  ],
  templateUrl: './admin-layout.html',
  styleUrls: ['./admin-layout.css'],
})
export class AdminLayout {
  private readonly router = inject(Router);

  sidebarCollapsed = signal(false);

  adminEmail = signal('');

  navItems: NavItem[] = [
    { label: 'Dashboard',   icon: 'dashboard',       route: '/admin/dashboard' },
    { label: 'Productos',   icon: 'inventory_2',     route: '/admin/productos' },
    { label: 'Categorías',  icon: 'category',        route: '/admin/categorias' },
    // Agrega aquí sin tocar el template:
    // { label: 'Pedidos',  icon: 'receipt_long',    route: '/admin/pedidos' },
    // { label: 'Usuarios', icon: 'group',           route: '/admin/usuarios' },
    // { label: 'Reportes', icon: 'bar_chart',       route: '/admin/reportes' },
  ];

  constructor() {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode<JwtPayload>(token);
        const email =
          decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
          decoded['email'] ?? 'Admin';
        this.adminEmail.set(email);
      } catch {}
    }
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.set(!this.sidebarCollapsed());
  }

  isActive(route: string): boolean {
    return this.router.url.startsWith(route);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['']);
  }

  goToCatalog(): void {
    this.router.navigate(['/productos']);
  }
}