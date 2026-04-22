import { Component, inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Producto } from '../../interfaces/Producto';
import { Catalogo } from '../../services/catalogo';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { CurrencyPipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-inicio',
  imports: [ReactiveFormsModule,
    CurrencyPipe,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Iniciocomponent implements OnInit {
  private catalogoService = inject(Catalogo);

  productos: Producto[] = [];
  categorias: string[] = [];
  categoriaActiva = '';
  cargando = true;
  skeletons = Array(8);

  searchControl = new FormControl('');

  ngOnInit(): void {
    this.cargarTodos();

    this.searchControl.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        switchMap((nombre) =>
          nombre?.trim()
            ? this.catalogoService.buscarPorNombre(nombre)
            : this.catalogoService.lista()
        )
      )
      .subscribe((productos) => {
        this.productos = productos;
        this.cargando = false;
      });
  }

  cargarTodos(): void {
    this.cargando = true;
    this.categoriaActiva = '';
    this.catalogoService.lista().subscribe((productos) => {
      this.productos = productos;
      this.categorias = [...new Set(productos.map((p) => p.categoria))];
      this.cargando = false;
    });
  }

  filtrarCategoria(categoria: string): void {
    this.categoriaActiva = categoria;
    this.cargando = true;

    const obs$ = categoria
      ? this.catalogoService.porCategoria(categoria)
      : this.catalogoService.lista();

    obs$.subscribe((productos) => {
      this.productos = productos;
      this.cargando = false;
    });
  }

  verDetalle(id: number): void {
    
    console.log('Ver detalle:', id);
  }

  agregarAlCarrito(event: Event, producto: Producto): void {
    event.stopPropagation(); // Evita abrir el detalle
    console.log('Agregar al carrito:', producto);
  }
}
