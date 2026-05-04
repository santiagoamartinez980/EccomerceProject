// features/admin/pages/products/admin-product-form.component.ts
import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { ProductsAdminService } from '../../../products/services/products-admin.service';
import {ProductInterface} from '../../../products/interfaces/product.interface';
import { CategoryInterface } from '../../../categories/interfaces/category.interface';

@Component({
  selector: 'app-admin-product-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatButtonModule, MatInputModule,
    MatFormFieldModule, MatSelectModule, MatSlideToggleModule,
    MatIconModule, MatProgressSpinnerModule, MatSnackBarModule,
  ],
  template: `
    <div class="form-dialog">
      <h2 mat-dialog-title class="dialog-title">
        <mat-icon>{{ isEdit ? 'edit' : 'add_circle' }}</mat-icon>
        {{ isEdit ? 'Editar Producto' : 'Nuevo Producto' }}
      </h2>

      <mat-dialog-content>
        <form [formGroup]="form" class="product-form">

          <mat-form-field appearance="outline">
            <mat-label>Nombre</mat-label>
            <input matInput formControlName="nombre" maxlength="50" />
            @if (form.get('nombre')?.hasError('required') && form.get('nombre')?.touched) {
              <mat-error>El nombre es requerido</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Descripción</mat-label>
            <textarea matInput formControlName="descripcion" rows="3" maxlength="100"></textarea>
          </mat-form-field>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Precio (COP)</mat-label>
              <input matInput type="number" formControlName="precio" min="0.01" />
              @if (form.get('precio')?.hasError('required') && form.get('precio')?.touched) {
                <mat-error>El precio es requerido</mat-error>
              }
              @if (form.get('precio')?.hasError('min')) {
                <mat-error>Debe ser mayor a 0</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Stock</mat-label>
              <input matInput type="number" formControlName="stock" min="0" />
              @if (form.get('stock')?.hasError('required') && form.get('stock')?.touched) {
                <mat-error>El stock es requerido</mat-error>
              }
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>URL de imagen</mat-label>
            <input matInput formControlName="imagenUrl" />
            <mat-icon matSuffix>image</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Categoría</mat-label>
            <mat-select formControlName="idCategoria">
              @for (cat of data.categories; track cat.categoryId) {
                <mat-option [value]="cat.categoryId">{{ cat.name }}</mat-option>
              }
            </mat-select>
            @if (form.get('idCategoria')?.hasError('required') && form.get('idCategoria')?.touched) {
              <mat-error>Selecciona una categoría</mat-error>
            }
          </mat-form-field>

          <mat-slide-toggle formControlName="activo" color="primary">
            Producto activo
          </mat-slide-toggle>

        </form>
      </mat-dialog-content>

      <mat-dialog-actions class="dialog-actions">
        <button mat-button (click)="cancel()">Cancelar</button>
        <button mat-flat-button color="primary"
                [disabled]="form.invalid || saving()"
                (click)="save()">
          @if (saving()) { <mat-spinner diameter="18" /> }
          @else { {{ isEdit ? 'Guardar cambios' : 'Crear producto' }} }
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .form-dialog { min-width: 480px; }
    .dialog-title { display: flex; align-items: center; gap: 10px; font-size: 1.1rem; font-weight: 700; }
    .dialog-title mat-icon { color: #6366f1; }
    .product-form { display: flex; flex-direction: column; gap: 12px; padding: 4px 0; }
    .product-form mat-form-field { width: 100%; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .dialog-actions { padding: 16px 24px !important; justify-content: flex-end; gap: 8px; }

    @media (max-width: 540px) {
      .form-dialog { min-width: unset; }
      .form-row { grid-template-columns: 1fr; }
    }
  `],
})
export class AdminProductForm implements OnInit {
  readonly data: { product: ProductInterface | null; categories: CategoryInterface[] } =
    inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AdminProductForm>);
  private readonly service = inject(ProductsAdminService);
  private readonly snack = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  saving = signal(false);
  isEdit = !!this.data.product;

  form = this.fb.group({
    nombre:     ['', [Validators.required, Validators.maxLength(50)]],
    descripcion:['', Validators.maxLength(100)],
    precio:     [null as number | null, [Validators.required, Validators.min(0.01)]],
    stock:      [null as number | null, [Validators.required, Validators.min(0)]],
    imagenUrl:  [''],
    activo:     [true],
    idCategoria:[null as number | null, Validators.required],
  });

  ngOnInit(): void {
    if (this.data.product) {
      const p = this.data.product;
      this.form.patchValue({
        nombre: p.nombre,
        descripcion: p.descripcion ?? '',
        precio: p.precio,
        stock: p.stock,
        imagenUrl: p.imagenUrl ?? '',
        activo: p.activo,
        idCategoria: p.idCategoria,
      });
    }
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const payload = this.form.value as any;

    const obs = this.isEdit
      ? this.service.update(this.data.product!.idProducto, payload)
      : this.service.create(payload);

    obs.subscribe({
      next: () => {
        this.snack.open(
          this.isEdit ? 'Producto actualizado' : 'Producto creado',
          'OK', { duration: 2500 }
        );
        this.dialogRef.close(true);
      },
      error: () => {
        this.snack.open('Error al guardar el producto', 'OK', { duration: 3000 });
        this.saving.set(false);
      },
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}