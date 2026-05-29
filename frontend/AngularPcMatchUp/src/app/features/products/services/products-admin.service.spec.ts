import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProductsAdminService } from './products-admin.service';
import { environment } from '../../../../environments/environment';
import { ProductInterface } from '../interfaces/product.interface';
import { CreateProductDto } from '../interfaces/create-product.dto';
import { UpdateProductDto } from '../interfaces/update-product.dto';
import { ApiResponse } from '../interfaces/api-response.interface';

const mockProduct = (id: number = 1): ProductInterface => ({
  idProducto: id,
  nombre: `Producto ${id}`,
  descripcion: 'Descripción del producto',
  precio: 100000,
  stock: 10,
  imagenUrl: 'https://test.com/image.jpg',
  activo: true,
  fechaCreacion: '2024-01-15T00:00:00Z',
  idCategoria: 5,
  categoriaNombre: 'Electrónicos',
});

const mockCreateDto = (overrides?: Partial<CreateProductDto>): CreateProductDto => ({
  nombre: 'Nuevo Producto',
  descripcion: 'Descripción del nuevo producto',
  precio: 150000,
  stock: 20,
  imagenUrl: 'https://test.com/new-image.jpg',
  activo: true,
  idCategoria: 3,
  ...overrides,
});

const mockUpdateDto = (overrides?: Partial<UpdateProductDto>): UpdateProductDto => ({
  ...overrides,
});

describe('ProductsAdminService', () => {
  let service: ProductsAdminService;
  let httpMock: HttpTestingController;
  const apiUrl = environment.apiUrl + '/Producto';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ProductsAdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    jest.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── getAlladmin() ─────────────────────────────────────────────────────────

  describe('getAlladmin', () => {
    it('should fetch all products for admin', () => {
      const mockResponse: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(1), mockProduct(2), mockProduct(3)],
      };
      let result: ProductInterface[] | undefined;

      service.getAlladmin().subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/Admin`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);

      expect(result).toEqual(mockResponse.value);
      expect(result?.length).toBe(3);
    });

    it('should handle empty response', () => {
      const mockResponse: ApiResponse<ProductInterface[]> = {
        value: [],
      };
      let result: ProductInterface[] | undefined;

      service.getAlladmin().subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/Admin`);
      req.flush(mockResponse);

      expect(result).toEqual([]);
    });

    it('should handle HTTP error', () => {
      const errorMessage = 'Error del servidor';
      let error: any;

      service.getAlladmin().subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/Admin`);
      req.flush(errorMessage, { status: 500, statusText: 'Server Error' });

      expect(error.status).toBe(500);
      expect(error.statusText).toBe('Server Error');
    });
  });

  // ─── create() ──────────────────────────────────────────────────────────────

  describe('create', () => {
    it('should create a new product with complete data', () => {
      const createDto = mockCreateDto();
      const createdProduct: ProductInterface = {
        idProducto: 10,
        nombre: createDto.nombre,
        descripcion: createDto.descripcion,
        precio: createDto.precio,
        stock: createDto.stock,
        imagenUrl: createDto.imagenUrl,
        activo: createDto.activo,
        idCategoria: createDto.idCategoria,
        fechaCreacion: new Date().toISOString(),
        categoriaNombre: 'Electrónicos',
      };
      const mockResponse = { value: createdProduct };
      let result: ProductInterface | undefined;

      service.create(createDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);

      expect(result).toEqual(createdProduct);
      expect(result?.idProducto).toBe(10);
    });

    it('should create product with activo = false', () => {
      const createDto = mockCreateDto({ activo: false });
      const createdProduct: ProductInterface = {
        idProducto: 11,
        nombre: createDto.nombre,
        descripcion: createDto.descripcion,
        precio: createDto.precio,
        stock: createDto.stock,
        imagenUrl: createDto.imagenUrl,
        activo: false,
        idCategoria: createDto.idCategoria,
        fechaCreacion: new Date().toISOString(),
      };
      const mockResponse = { value: createdProduct };
      let result: ProductInterface | undefined;

      service.create(createDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.body.activo).toBe(false);
      req.flush(mockResponse);

      expect(result?.activo).toBe(false);
    });

    it('should create product with empty imagenUrl', () => {
      const createDto = mockCreateDto({ imagenUrl: '' });
      const createdProduct: ProductInterface = {
        idProducto: 12,
        nombre: createDto.nombre,
        descripcion: createDto.descripcion,
        precio: createDto.precio,
        stock: createDto.stock,
        imagenUrl: '',
        activo: createDto.activo,
        idCategoria: createDto.idCategoria,
        fechaCreacion: new Date().toISOString(),
      };
      const mockResponse = { value: createdProduct };
      let result: ProductInterface | undefined;

      service.create(createDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.body.imagenUrl).toBe('');
      req.flush(mockResponse);

      expect(result?.imagenUrl).toBe('');
    });

    it('should handle validation error on create', () => {
      const createDto = mockCreateDto({
        nombre: '',
        precio: -100,
        stock: -1,
        idCategoria: 0,
      });
      const errorResponse = { message: 'Datos inválidos' };
      let error: any;

      service.create(createDto).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(apiUrl);
      req.flush(errorResponse, { status: 400, statusText: 'Bad Request' });

      expect(error.status).toBe(400);
      expect(error.error).toEqual(errorResponse);
    });
  });

  // ─── update() ──────────────────────────────────────────────────────────────

  describe('update', () => {
    it('should update all fields of an existing product', () => {
      const productId = 1;
      const updateDto = mockUpdateDto({
        nombre: 'Producto Actualizado',
        descripcion: 'Nueva descripción',
        precio: 200000,
        stock: 15,
        imagenUrl: 'https://test.com/updated-image.jpg',
        activo: false,
        idCategoria: 10,
      });
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
        ...updateDto,
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(mockResponse);

      expect(result?.nombre).toBe('Producto Actualizado');
      expect(result?.descripcion).toBe('Nueva descripción');
      expect(result?.precio).toBe(200000);
      expect(result?.stock).toBe(15);
      expect(result?.imagenUrl).toBe('https://test.com/updated-image.jpg');
      expect(result?.activo).toBe(false);
      expect(result?.idCategoria).toBe(10);
    });

    it('should update only single field (nombre)', () => {
      const productId = 5;
      const updateDto = mockUpdateDto({ nombre: 'Solo nombre actualizado' });
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
        nombre: 'Solo nombre actualizado',
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.body).toEqual(updateDto);
      expect(req.request.body.nombre).toBe('Solo nombre actualizado');
      expect(req.request.body.precio).toBeUndefined();
      expect(req.request.body.stock).toBeUndefined();
      req.flush(mockResponse);

      expect(result?.nombre).toBe('Solo nombre actualizado');
      expect(result?.precio).toBe(mockProduct(productId).precio);
    });

    it('should update only stock', () => {
      const productId = 5;
      const updateDto = mockUpdateDto({ stock: 0 });
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
        stock: 0,
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.body).toEqual(updateDto);
      expect(req.request.body.stock).toBe(0);
      expect(req.request.body.nombre).toBeUndefined();
      req.flush(mockResponse);

      expect(result?.stock).toBe(0);
    });

    it('should update only activo to false', () => {
      const productId = 5;
      const updateDto = mockUpdateDto({ activo: false });
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
        activo: false,
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.body).toEqual(updateDto);
      expect(req.request.body.activo).toBe(false);
      req.flush(mockResponse);

      expect(result?.activo).toBe(false);
    });

    it('should update multiple fields (partial update)', () => {
      const productId = 5;
      const updateDto = mockUpdateDto({
        precio: 250000,
        stock: 25,
        activo: true,
      });
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
        precio: updateDto.precio as number,
        stock: updateDto.stock as number,
        activo: updateDto.activo as boolean,
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.body).toEqual(updateDto);
      expect(req.request.body.precio).toBe(250000);
      expect(req.request.body.stock).toBe(25);
      expect(req.request.body.activo).toBe(true);
      expect(req.request.body.nombre).toBeUndefined();
      req.flush(mockResponse);

      expect(result?.precio).toBe(250000);
      expect(result?.stock).toBe(25);
      expect(result?.activo).toBe(true);
    });

    it('should update with empty object (no fields changed)', () => {
      const productId = 5;
      const updateDto = mockUpdateDto({});
      const updatedProduct: ProductInterface = {
        ...mockProduct(productId),
      };
      const mockResponse = { value: updatedProduct };
      let result: ProductInterface | undefined;

      service.update(productId, updateDto).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);

      expect(result).toEqual(mockProduct(productId));
    });

    it('should handle 404 when product not found', () => {
      const productId = 999;
      const updateDto = mockUpdateDto({ nombre: 'No Existe' });
      let error: any;

      service.update(productId, updateDto).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      req.flush('Producto no encontrado', { status: 404, statusText: 'Not Found' });

      expect(error.status).toBe(404);
    });
  });

  // ─── delete() ──────────────────────────────────────────────────────────────

  describe('delete', () => {
    it('should delete a product', () => {
      const productId = 1;
      const mockResponse = { mensaje: 'Producto eliminado correctamente' };
      let result: { mensaje: string } | undefined;

      service.delete(productId).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);

      expect(result).toEqual(mockResponse);
      expect(result?.mensaje).toBe('Producto eliminado correctamente');
    });

    it('should handle deletion of non-existent product', () => {
      const productId = 999;
      let error: any;

      service.delete(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      req.flush('Producto no encontrado', { status: 404, statusText: 'Not Found' });

      expect(error.status).toBe(404);
    });

    it('should handle server error on deletion', () => {
      const productId = 1;
      let error: any;

      service.delete(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      req.flush('Error interno del servidor', { status: 500, statusText: 'Internal Server Error' });

      expect(error.status).toBe(500);
    });
  });

  // ─── Edge cases ────────────────────────────────────────────────────────────

  describe('edge cases', () => {
    it('should handle concurrent requests', () => {
      const mockResponse1: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(1)],
      };
      const mockResponse2: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(2)],
      };
      let result1: ProductInterface[] | undefined;
      let result2: ProductInterface[] | undefined;

      service.getAlladmin().subscribe(res => {
        result1 = res;
      });
      service.getAlladmin().subscribe(res => {
        result2 = res;
      });

      const requests = httpMock.match(`${apiUrl}/Admin`);
      expect(requests.length).toBe(2);
      requests[0].flush(mockResponse1);
      requests[1].flush(mockResponse2);

      expect(result1).toEqual([mockProduct(1)]);
      expect(result2).toEqual([mockProduct(2)]);
    });
  });
});