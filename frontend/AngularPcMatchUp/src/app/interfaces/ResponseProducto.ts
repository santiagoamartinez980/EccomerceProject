import { Producto } from "./Producto";

export interface responseProducto {
    value: Producto[];
}
export interface ResponseProductoSingle {
    value: Producto;
}