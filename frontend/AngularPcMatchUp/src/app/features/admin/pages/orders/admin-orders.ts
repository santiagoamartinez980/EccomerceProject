import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { OrderDto } from '../../../order/interfaces/order.dto';
import { AdminOrderStatus, AdminOrdersService } from '../../services/admin-orders.service';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-orders.html',
  styleUrls: ['./admin-orders.css'],
})
export class AdminOrders implements OnInit {
  private readonly service = inject(AdminOrdersService);
  orders = signal<OrderDto[]>([]);
  loading = signal(false);
  saving = signal<number | null>(null);
  error = signal('');
  filter = signal('all');

  ngOnInit(): void { this.refresh(); }

  refresh(): void {
    this.loading.set(true);
    this.error.set('');
    this.service.list().subscribe({
      next: orders => { this.orders.set(orders); this.loading.set(false); },
      error: err => { this.error.set(err.error?.message || 'No se pudieron cargar los pedidos.'); this.loading.set(false); },
    });
  }

  visibleOrders(): OrderDto[] {
    return this.filter() === 'all' ? this.orders() : this.orders().filter(o => o.status === this.filter());
  }

  change(order: OrderDto, status: AdminOrderStatus): void {
    if (status === 4 && !window.confirm(`¿Cancelar el pedido #${order.orderId}? Esta acción no se puede deshacer.`)) return;
    this.saving.set(order.orderId);
    this.error.set('');
    this.service.updateStatus(order.orderId, status).subscribe({
      next: () => {
        this.saving.set(null);
        this.refresh();
      },
      error: err => {
        this.error.set(err.error?.message || 'No se pudo cambiar el estado del pedido.');
        this.saving.set(null);
      },
    });
  }

  label(status: string): string {
    return ({ Pending: 'Pendiente de pago', Paid: 'Pendiente de despacho', Shipped: 'Despachado', Delivered: 'Entregado', Cancelled: 'Cancelado' } as Record<string, string>)[status] || status;
  }
}
