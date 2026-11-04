import { Injectable, PLATFORM_ID, afterNextRender, effect, inject, signal } from '@angular/core';
import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { Lang, TRANSLATIONS } from './translations';

const STORAGE_KEY = 'lang';
const LOCALES: Record<Lang, string> = { es: 'es-CO', en: 'en-US' };

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly document = inject(DOCUMENT);

  /** Idioma activo. Siempre arranca en 'es' para que SSR e hidratación coincidan. */
  readonly lang = signal<Lang>('es');

  constructor() {
    // Una vez renderizado en el navegador, recupera el idioma guardado.
    afterNextRender(() => {
      const saved = this.readSaved();
      if (saved) this.lang.set(saved);
    });

    effect(() => {
      this.document.documentElement.lang = this.lang();
    });
  }

  /** Locale para Intl (fechas, números): es-CO o en-US. */
  get locale(): string {
    return LOCALES[this.lang()];
  }

  setLang(lang: Lang): void {
    this.lang.set(lang);
    if (isPlatformBrowser(this.platformId)) {
      try {
        localStorage.setItem(STORAGE_KEY, lang);
      } catch {
        /* almacenamiento no disponible */
      }
    }
  }

  toggle(): void {
    this.setLang(this.lang() === 'es' ? 'en' : 'es');
  }

  /** Traduce una clave. Si no existe en el idioma actual cae a español, y si no, devuelve la clave. */
  t(key: string, params?: Record<string, string | number>): string {
    let text = TRANSLATIONS[this.lang()][key] ?? TRANSLATIONS['es'][key] ?? key;
    if (params) {
      for (const [name, value] of Object.entries(params)) {
        text = text.split(`{${name}}`).join(String(value));
      }
    }
    return text;
  }

  private readSaved(): Lang | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return saved === 'es' || saved === 'en' ? saved : null;
    } catch {
      return null;
    }
  }
}
