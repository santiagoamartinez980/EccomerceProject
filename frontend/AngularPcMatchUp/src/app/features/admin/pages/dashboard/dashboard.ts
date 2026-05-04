// features/admin/pages/dashboard/dashboard.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatCardModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
})
export class Dashboard {
  private readonly router = inject(Router);

  shortcuts = [
    { label: 'Gestionar Productos', icon: 'inventory_2', route: '/admin/productos', color: 'blue' },
    { label: 'Gestionar Categorías', icon: 'category',   route: '/admin/categorias', color: 'purple' },
  ];

  go(route: string): void {
    this.router.navigate([route]);
  }
}