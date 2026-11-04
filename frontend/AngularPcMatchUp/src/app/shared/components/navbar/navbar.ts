import { Component, inject, input, signal, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TokenService } from '../../../core/services/token.service';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { I18nService } from '../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink,
  RouterLinkActive,
  MatIconModule,
  MatTooltipModule,
  MatMenuModule,
  MatButtonModule,
  TranslatePipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})


export class Navbar {
  private router = inject(Router);
  readonly i18n = inject(I18nService);
  brandName = input<string>('Eccomerce');
  brandIcon = input<string>('storefront');

  private tokenService = inject(TokenService);
  isLoggedIn = this.tokenService.isLoggedIn$;

  logout(): void {
  localStorage.removeItem('token'); // o el método que uses
  this.router.navigate(['/auth/login']);
}
}
