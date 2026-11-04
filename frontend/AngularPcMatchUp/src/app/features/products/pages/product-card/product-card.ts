
import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProductInterface } from '../../interfaces/product.interface';
import { I18nService } from '../../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
@Component({
  selector: 'app-product-card',
  imports: [CommonModule,
    MatCardModule,
    MatIconModule,
    MatRippleModule,
    MatButtonModule,
    MatTooltipModule,
    TranslatePipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard {
  private readonly i18n = inject(I18nService);
  product = input.required<ProductInterface>();
    cardClick = output<ProductInterface>();
  
    onClick(): void {
      this.cardClick.emit(this.product());
    }
  
    formatPrice(price: number): string {
      return new Intl.NumberFormat('es-CO', {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0,
      }).format(price);
    }
  
    getStockLabel(stock: number): string {
      if (stock === 0) return this.i18n.t('stock.soldOut');
      if (stock <= 5) return this.i18n.t('stock.lastN', { n: stock });
      return this.i18n.t('stock.left', { n: stock });
    }
  
    getStockLevel(stock: number): 'ok' | 'low' | 'out' {
      if (stock === 0) return 'out';
      if (stock <= 5) return 'low';
      return 'ok';
    }
}
