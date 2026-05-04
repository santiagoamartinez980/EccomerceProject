// features/admin/pages/categories/admin-categories.component.ts
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';

import { CategoryInterface } from '../../../categories/interfaces/category.interface';
import { AdminCategoriesService } from '../../../categories/services/category.service';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSnackBarModule,
    MatProgressSpinnerModule, MatTooltipModule, MatCardModule,
  ],
  templateUrl: './admin-categories.html',
  styleUrls: ['./admin-categories.css'],
})
export class AdminCategoriesComponent implements OnInit {
  private readonly service = inject(AdminCategoriesService);
  private readonly snack = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  categories = signal<CategoryInterface[]>([]);
  loading = signal(false);
  saving = signal(false);

  displayedColumns = ['id', 'nombre', 'acciones'];

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (data) => { this.categories.set(data); this.loading.set(false); },
      error: () => { this.snack.open('Error al cargar categorías', 'OK', { duration: 3000 }); this.loading.set(false); },
    });
  }

  create(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.service.create({ name: this.form.value.name! }).subscribe({
      next: () => {
        this.snack.open('Categoría creada', 'OK', { duration: 2500 });
        this.form.reset();
        this.saving.set(false);
        this.load();
      },
      error: () => {
        this.snack.open('Error al crear categoría', 'OK', { duration: 3000 });
        this.saving.set(false);
      },
    });
  }

  confirmDelete(category: CategoryInterface): void {
    if (!confirm(`¿Eliminar la categoría "${category.name}"?`)) return;
    this.service.delete(category.categoryId).subscribe({
      next: () => {
        this.snack.open('Categoría eliminada', 'OK', { duration: 2500 });
        this.load();
      },
      error: () => this.snack.open('No se puede eliminar: tiene productos asociados', 'OK', { duration: 4000 }),
    });
  }
}