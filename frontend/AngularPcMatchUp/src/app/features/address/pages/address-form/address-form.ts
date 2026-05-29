import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';

import { AddressApiService } from '../../services/address-api.service';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatSelectModule,
  ],
  templateUrl: './address-form.html',
  styleUrls: ['./address-form.css'],
})
export class AddressForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly addressService = inject(AddressApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  form!: FormGroup;
  saving = signal(false);
  editingId = signal<number | null>(null);
  loadingDetail = signal(true);

  departments = [
    'Antioquia', 'Arauca', 'Atlántico', 'Bolívar', 'Boyacá', 'Caldas',
    'Caquetá', 'Cauca', 'Cesar', 'Chocó', 'Córdoba', 'Cundinamarca',
    'Guainía', 'Guaviare', 'Huila', 'La Guajira', 'Magdalena', 'Meta',
    'Nariño', 'Norte de Santander', 'Putumayo', 'Quindío', 'Risaralda',
    'Santander', 'Sucre', 'Tolima', 'Valle del Cauca', 'Vaupés', 'Vichada'
  ];

  ngOnInit(): void {
    this.initForm();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadAddress(parseInt(id));
    } else {
      this.loadingDetail.set(false);
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      addressLine: ['', [Validators.required, Validators.minLength(5)]],
      city: ['', Validators.required],
      department: ['', Validators.required],
      postalCode: ['', Validators.required],
      phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      notes: [''],
      isDefault: [false],
    });
  }

  private loadAddress(id: number): void {
    this.addressService.getAll().subscribe({
      next: (addresses) => {
        const addr = addresses.find(a => a.addressId === id);
        if (addr) {
          this.editingId.set(id);
          this.form.patchValue(addr);
        }
        this.loadingDetail.set(false);
      },
      error: () => {
        this.snack.open('Error al cargar la dirección', 'OK', { 
          duration: 3000, 
          panelClass: 'snack-error' 
        });
        this.loadingDetail.set(false);
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const id = this.editingId();
    const request = this.form.value;

    const operation = id
      ? this.addressService.update(id, request)
      : this.addressService.create(request);

    operation.subscribe({
      next: () => {
        this.saving.set(false);
        this.snack.open(
          id ? 'Dirección actualizada' : 'Dirección creada',
          'OK',
          { duration: 2000 }
        );
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigate([returnUrl || '/direcciones']);
      },
      error: () => {
        this.saving.set(false);
        this.snack.open('Error al guardar', 'OK', { 
          duration: 3000, 
          panelClass: 'snack-error' 
        });
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/direcciones']);
  }

  trackByDepartment(_: number, dept: string): string {
    return dept;
  }
}
