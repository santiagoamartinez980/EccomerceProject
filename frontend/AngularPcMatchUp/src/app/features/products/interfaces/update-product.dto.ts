export interface UpdateProductDto {
  nombre?: string;
  descripcion?: string;
  precio?: number;
  stock?: number;
  imagenUrl?: string;
  activo?: boolean;
  idCategoria?: number;
}