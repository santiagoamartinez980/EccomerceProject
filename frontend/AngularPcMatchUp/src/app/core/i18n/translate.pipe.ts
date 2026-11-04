import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nService } from './i18n.service';

/**
 * Uso en plantillas:  {{ 'nav.home' | t }}   o   {{ 'cart.onlyN' | t: { n: 3 } }}
 * Es impuro a propósito: así se vuelve a evaluar al cambiar el idioma.
 */
@Pipe({ name: 't', standalone: true, pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  transform(key: string, params?: Record<string, string | number>): string {
    return this.i18n.t(key, params);
  }
}
