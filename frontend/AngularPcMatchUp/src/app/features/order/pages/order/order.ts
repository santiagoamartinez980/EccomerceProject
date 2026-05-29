import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';


import { OrderDto } from '../../interfaces/order.dto';
import { OrderService } from '../../services/order.services';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatExpansionModule,
    MatTooltipModule,
    MatDialogModule,
  ],
  templateUrl: './order.html',
  styleUrls: ['./order.css'],
})
export class MyOrders implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly router       = inject(Router);
  private readonly snack        = inject(MatSnackBar);

  orders   = signal<OrderDto[]>([]);
  loading  = signal(true);
  cancelling = signal<number | null>(null);

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading.set(true);
    this.orderService.getAll().subscribe({
      next: (data) => {
        // Ordenar por fecha más reciente primero
        this.orders.set(data.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        ));
        this.loading.set(false);
      },
      error: () => {
        this.snack.open('Error al cargar tus pedidos', 'OK', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  cancelOrder(orderId: number): void {
    if (!confirm('¿Estás seguro de que deseas cancelar este pedido?')) return;

    this.cancelling.set(orderId);
    this.orderService.cancel(orderId).subscribe({
      next: () => {
        this.snack.open('Pedido cancelado correctamente', 'OK', { duration: 3000 });
        this.cancelling.set(null);
        this.loadOrders();
      },
      error: () => {
        this.snack.open('No se pudo cancelar el pedido', 'OK', { duration: 3000 });
        this.cancelling.set(null);
      },
    });
  }

  goToProducts(): void {
    this.router.navigate(['/productos']);
  }

  canCancel(status: string): boolean {
    return status === 'Pending' || status === 'Confirmed';
  }

  // ── Status helpers ─────────────────────────────────────────────────────────

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Pending:   'Pendiente',
      Confirmed: 'Confirmado',
      Shipped:   'Enviado',
      Delivered: 'Entregado',
      Cancelled: 'Cancelado',
    };
    return map[status] ?? status;
  }

  getStatusColor(status: string): 'primary' | 'accent' | 'warn' | '' {
    const map: Record<string, 'primary' | 'accent' | 'warn' | ''> = {
      Pending:   'accent',
      Confirmed: 'primary',
      Shipped:   'primary',
      Delivered: '',
      Cancelled: 'warn',
    };
    return map[status] ?? '';
  }

  getStatusIcon(status: string): string {
    const map: Record<string, string> = {
      Pending:   'schedule',
      Confirmed: 'check_circle',
      Shipped:   'local_shipping',
      Delivered: 'done_all',
      Cancelled: 'cancel',
    };
    return map[status] ?? 'help_outline';
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('es-CO', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(dateStr));
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency', currency: 'COP', minimumFractionDigits: 0,
    }).format(price);
  }

  trackByOrder(_: number, order: OrderDto): number {
    return order.orderId;
  }
}