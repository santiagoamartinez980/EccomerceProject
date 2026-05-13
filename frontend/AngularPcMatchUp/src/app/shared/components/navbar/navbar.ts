import { Component, inject, input, signal, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatTooltipModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})


export class Navbar {

  brandName = input<string>('Consumir nombre de la api');
  brandIcon = input<string>('storefron');

  private tokenService = inject(TokenService);
  isLoggedIn = this.tokenService.isLoggedIn$;
}