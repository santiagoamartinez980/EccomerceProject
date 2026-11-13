import { Component, OnInit, OnDestroy, signal, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';

import { CartService } from '../../cart/services/cart.service';
import { AddressApiService } from '../../address/services/address-api.service';

import { CartDto } from '../../cart/interfaces/cart.dto';
import { AddressDto } from '../../address/interfaces/address.dto';
import { PaymentService } from '../services/payment.services';
import { OrderService } from '../../order/services/order.services';
import { OrderDto } from '../../order/interfaces/order.dto';
import { PaymentIntentDto } from './interfaces/payment-interface.dto';
import { I18nService } from '../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

interface WompiResult { transaction?: { id: string; status: string } }
interface WompiCheckout { open(callback: (result: WompiResult) => void): void }
interface WompiWindow extends Window {
  WidgetCheckout?: new (options: {
    currency: string;
    amountInCents: number;
    reference: string;
    publicKey: string;
    signature: { integrity: string };
  }) => WompiCheckout;
}


@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatRadioModule,
    FormsModule,
    TranslatePipe,
  ],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css'],
})
export class Checkout implements OnInit, OnDestroy {
  private readonly cartService    = inject(CartService);
  private readonly addressService = inject(AddressApiService);
  private readonly orderService   = inject(OrderService);
  private readonly paymentService = inject(PaymentService);
  private readonly router         = inject(Router);
  private readonly snack          = inject(MatSnackBar);
  private readonly platformId     = inject(PLATFORM_ID);
  private readonly i18n           = inject(I18nService);

  cart                = signal<CartDto | null>(null);
  addresses           = signal<AddressDto[]>([]);
  selectedAddressId   = signal<number | null>(null);
  createdOrder        = signal<OrderDto | null>(null);
  paymentIntent       = signal<PaymentIntentDto | null>(null);

  loadingCart         = signal(true);
  loadingAddresses    = signal(true);
  creatingOrder       = signal(false);
  loadingPayment      = signal(false);

  step = signal<'summary' | 'payment' | 'confirming'>('summary');

  ngOnInit(): void {
    this.loadCart();
    this.loadAddresses();
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const container = document.getElementById('wompi-widget-container');
    if (container) container.innerHTML = '';
  }

  loadCart(): void {
    this.cartService.getCart().subscribe({
      next: (data) => { this.cart.set(data); this.loadingCart.set(false); },
      error: () => {
        this.snack.open(this.i18n.t('checkout.errorCart'), this.i18n.t('common.ok'), { duration: 3000 });
        this.loadingCart.set(false);
      },
    });
  }

  loadAddresses(): void {
    this.addressService.getAll().subscribe({
      next: (data) => {
        this.addresses.set(data);
        const def = data.find(a => a.isDefault);
        if (def) this.selectedAddressId.set(def.addressId);
        this.loadingAddresses.set(false);
      },
      error: () => {
        this.snack.open(this.i18n.t('checkout.errorAddresses'), this.i18n.t('common.ok'), { duration: 3000 });
        this.loadingAddresses.set(false);
      },
    });
  }

  createOrder(): void {
    if (!this.selectedAddressId()) {
      this.snack.open(this.i18n.t('checkout.selectAddress'), this.i18n.t('common.ok'), { duration: 3000 });
      return;
    }
    if (!this.cart()?.items.length) {
      this.snack.open(this.i18n.t('checkout.emptyCart'), this.i18n.t('common.ok'), { duration: 3000 });
      return;
    }

    this.creatingOrder.set(true);
    console.log('Creando orden...');

    const payload: any = {
      addressId: this.selectedAddressId()!,
      items: this.cart()?.items.map(item => ({
        productId: item.productId,
        quantity: item.quantity
      })) ?? []
    };

    this.orderService.create(payload).subscribe({
      next: (order) => {
        console.log('Orden creada:', order.orderId);
        this.createdOrder.set(order);
        this.loadPaymentIntent(order.orderId);
      },
      error: (err) => {
        console.error('Error al crear orden:', err);
        this.snack.open(this.i18n.t('checkout.errorOrder'), this.i18n.t('common.ok'), { duration: 3000 });
        this.creatingOrder.set(false);
      },
    });
  }

  private loadPaymentIntent(orderId: number): void {
    this.loadingPayment.set(true);
    console.log('Creando intención de pago para orden:', orderId);

    this.paymentService.createIntent(orderId).subscribe({
      next: (intent) => {
        console.log('Wompi intent recibido:', intent.reference);
        this.paymentIntent.set(intent);
        this.loadingPayment.set(false);
        this.step.set('payment');
      },
      error: (error) => {
        console.error('Error al crear intención:', error);
        this.snack.open(
          this.i18n.t('checkout.errorPayment', { status: error.statusText }),
          this.i18n.t('common.ok'),
          { duration: 5000 }
        );
        this.loadingPayment.set(false);
        this.creatingOrder.set(false);
      },
    });
  }

  async openWompi(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const intent = this.paymentIntent();
    const orderId = this.createdOrder()?.orderId;
    if (!intent || !orderId) return;
    this.step.set('confirming');
    try {
      await this.loadWompiScript();
      const WidgetCheckout = (window as WompiWindow).WidgetCheckout;
      if (!WidgetCheckout) throw new Error('El Widget de Wompi no está disponible.');
      new WidgetCheckout({
        currency: intent.currency,
        amountInCents: intent.amountInCents,
        reference: intent.reference,
        publicKey: intent.publicKey,
        signature: { integrity: intent.signature },
      }).open(result => {
        if (result.transaction?.status === 'APPROVED') {
          this.cartService.clearCart().subscribe({ error: () => {} });
        }
        this.router.navigate([`/pedido/${orderId}/confirmacion`]);
      });
      this.step.set('payment');
    } catch {
      this.snack.open('No se pudo abrir Wompi Sandbox.', this.i18n.t('common.ok'), { duration: 5000 });
      this.step.set('payment');
    }
  }

  private loadWompiScript(): Promise<void> {
    if ((window as WompiWindow).WidgetCheckout) return Promise.resolve();
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = 'https://checkout.wompi.co/widget.js';
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('No se pudo cargar Wompi.'));
      document.head.appendChild(script);
    });
  }

  goToAddresses(): void {
    this.router.navigate(['/direcciones/nueva'], { queryParams: { returnUrl: '/checkout' } });
  }

  goBackToCart(): void {
    this.router.navigate(['/carrito']);
  }

  get isLoading(): boolean {
    return this.loadingCart() || this.loadingAddresses();
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency', currency: 'COP', minimumFractionDigits: 0,
    }).format(price);
  }

  trackByAddress(_: number, addr: AddressDto): number { return addr.addressId; }
}
