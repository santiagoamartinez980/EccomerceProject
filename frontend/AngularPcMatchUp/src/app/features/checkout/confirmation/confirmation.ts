import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { OrderService } from '../../order/services/order.services';
import { OrderDto } from '../../order/interfaces/order.dto';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './confirmation.html',
  styleUrls: ['./confirmation.css'],
})
export class Confirmation implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  order = signal<OrderDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadOrder();
  }

  loadOrder(): void {
    this.loading.set(true);
    const orderId = parseInt(this.route.snapshot.paramMap.get('id') || '0', 10);

    if (!orderId) {
      this.error.set('Orden no encontrada');
      this.loading.set(false);
      return;
    }

    this.orderService.getById(orderId).subscribe({
      next: (data: OrderDto) => {
        this.order.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No pudimos cargar los detalles de tu orden');
        this.loading.set(false);
      },
    });
  }

  goToOrders(): void {
    this.router.navigate(['/pedidos']);
  }

  goToProducts(): void {
    this.router.navigate(['/productos']);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(price);
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('es-CO', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(dateStr));
  }
}
