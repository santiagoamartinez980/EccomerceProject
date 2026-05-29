import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AddressApiService } from '../../services/address-api.service';
import { AddressDto } from '../../interfaces/address.dto';

@Component({
  selector: 'app-address-list',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSnackBarModule,
    RouterModule,
  ],
  templateUrl: './address-list.html',
  styleUrls: ['./address-list.css'],
})
export class AddressList implements OnInit {
  private readonly addressService = inject(AddressApiService);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  addresses = signal<AddressDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  deleting = signal<number | null>(null);

  ngOnInit(): void {
    this.loadAddresses();
  }

  loadAddresses(): void {
    this.loading.set(true);
    this.error.set(null);
    this.addressService.getAll().subscribe({
      next: (data) => {
        this.addresses.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar las direcciones');
        this.loading.set(false);
      },
    });
  }

  createNew(): void {
    this.router.navigate(['/direcciones/nueva']);
  }

  editAddress(id: number): void {
    this.router.navigate(['/direcciones/editar', id]);
  }

  deleteAddress(id: number): void {
    if (!confirm('¿Eliminar esta dirección?')) return;
    this.deleting.set(id);
    this.addressService.delete(id).subscribe({
      next: () => {
        this.addresses.update(addrs => addrs.filter(a => a.addressId !== id));
        this.deleting.set(null);
        this.snack.open('Dirección eliminada', 'OK', { duration: 2000 });
      },
      error: () => {
        this.snack.open('Error al eliminar', 'OK', { duration: 3000, panelClass: 'snack-error' });
        this.deleting.set(null);
      },
    });
  }

  setDefault(address: AddressDto): void {
    const request = { ...address, isDefault: true };
    this.addressService.update(address.addressId, request).subscribe({
      next: () => {
        this.loadAddresses();
        this.snack.open('Dirección predeterminada actualizada', 'OK', { duration: 2000 });
      },
      error: () => {
        this.snack.open('Error al actualizar', 'OK', { duration: 3000, panelClass: 'snack-error' });
      },
    });
  }

  isDeleting(id: number): boolean {
    return this.deleting() === id;
  }

  trackByAddress(_: number, addr: AddressDto): number {
    return addr.addressId;
  }
}
