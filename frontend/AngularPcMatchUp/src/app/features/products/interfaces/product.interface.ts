import { CategoryInterface } from "../../categories/interfaces/category.interface";

export interface ProductInterface {
  idProducto: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  stock: number;
  imagenUrl?: string;
  activo: boolean;
  fechaCreacion: string;
  idCategoria : number;
  categoriaNombre?: string;
}