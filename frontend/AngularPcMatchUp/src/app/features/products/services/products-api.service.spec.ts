import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProductsApiService } from './products-api.service';
import { environment } from '../../../../environments/environment';
import { ProductInterface } from '../interfaces/product.interface';
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

describe('ProductsApiService', () => {
  let service: ProductsApiService;
  let httpMock: HttpTestingController;
  const apiUrl = environment.apiUrl + '/Producto';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ProductsApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    jest.restoreAllMocks();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── getAll() ──────────────────────────────────────────────────────────────

  describe('getAll', () => {
    it('should fetch all products', () => {
      const mockResponse: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(1), mockProduct(2), mockProduct(3)],
      };
      let result: ProductInterface[] | undefined;

      service.getAll().subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(apiUrl);
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

      service.getAll().subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(apiUrl);
      req.flush(mockResponse);

      expect(result).toEqual([]);
    });

    it('should handle HTTP error', () => {
      const errorMessage = 'Error del servidor';
      let error: any;

      service.getAll().subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(apiUrl);
      req.flush(errorMessage, { status: 500, statusText: 'Server Error' });

      expect(error.status).toBe(500);
      expect(error.statusText).toBe('Server Error');
    });

    it('should handle network error', () => {
      let error: any;

      service.getAll().subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(apiUrl);
      req.error(new ErrorEvent('Network error'));

      expect(error.error).toBeDefined();
    });
  });

  // ─── getById() ─────────────────────────────────────────────────────────────

  describe('getById', () => {
    it('should fetch product by id', () => {
      const productId = 1;
      const mockResponse: ApiResponse<ProductInterface> = {
        value: mockProduct(productId),
      };
      let result: ProductInterface | undefined;

      service.getById(productId).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);

      expect(result).toEqual(mockProduct(productId));
      expect(result?.idProducto).toBe(productId);
    });

    it('should handle 404 when product not found', () => {
      const productId = 999;
      let error: any;

      service.getById(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      req.flush('Producto no encontrado', { status: 404, statusText: 'Not Found' });

      expect(error.status).toBe(404);
    });

    it('should handle invalid id', () => {
      const productId = -1;
      let error: any;

      service.getById(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/${productId}`);
      req.flush('ID inválido', { status: 400, statusText: 'Bad Request' });

      expect(error.status).toBe(400);
    });
  });

  // ─── getByIdPublic() ───────────────────────────────────────────────────────

  describe('getByIdPublic', () => {
    it('should fetch product by id from public endpoint', () => {
      const productId = 1;
      const mockResponse: ApiResponse<ProductInterface> = {
        value: mockProduct(productId),
      };
      let result: ProductInterface | undefined;

      service.getByIdPublic(productId).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/public/${productId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);

      expect(result).toEqual(mockProduct(productId));
      expect(result?.idProducto).toBe(productId);
    });

    it('should handle 404 when product not found in public endpoint', () => {
      const productId = 999;
      let error: any;

      service.getByIdPublic(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/public/${productId}`);
      req.flush('Producto no encontrado', { status: 404, statusText: 'Not Found' });

      expect(error.status).toBe(404);
    });

    it('should handle inactive product (not visible to public)', () => {
      const productId = 2;
      let error: any;

      service.getByIdPublic(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/public/${productId}`);
      req.flush('Producto no disponible', { status: 403, statusText: 'Forbidden' });

      expect(error.status).toBe(403);
    });

    it('should handle server error', () => {
      const productId = 1;
      let error: any;

      service.getByIdPublic(productId).subscribe({
        error: (err) => {
          error = err;
        },
      });

      const req = httpMock.expectOne(`${apiUrl}/public/${productId}`);
      req.flush('Error interno', { status: 500, statusText: 'Internal Server Error' });

      expect(error.status).toBe(500);
    });
  });

  // ─── Edge cases ────────────────────────────────────────────────────────────

  describe('edge cases', () => {
    it('should handle concurrent getAll requests', () => {
      const mockResponse1: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(1)],
      };
      const mockResponse2: ApiResponse<ProductInterface[]> = {
        value: [mockProduct(2)],
      };
      let result1: ProductInterface[] | undefined;
      let result2: ProductInterface[] | undefined;

      service.getAll().subscribe(res => {
        result1 = res;
      });
      service.getAll().subscribe(res => {
        result2 = res;
      });

      const requests = httpMock.match(apiUrl);
      expect(requests.length).toBe(2);
      requests[0].flush(mockResponse1);
      requests[1].flush(mockResponse2);

      expect(result1).toEqual([mockProduct(1)]);
      expect(result2).toEqual([mockProduct(2)]);
    });

    it('should handle concurrent getById requests', () => {
      const mockResponse1: ApiResponse<ProductInterface> = {
        value: mockProduct(1),
      };
      const mockResponse2: ApiResponse<ProductInterface> = {
        value: mockProduct(2),
      };
      let result1: ProductInterface | undefined;
      let result2: ProductInterface | undefined;

      service.getById(1).subscribe(res => {
        result1 = res;
      });
      service.getById(2).subscribe(res => {
        result2 = res;
      });

      const req1 = httpMock.expectOne(`${apiUrl}/1`);
      const req2 = httpMock.expectOne(`${apiUrl}/2`);
      
      req1.flush(mockResponse1);
      req2.flush(mockResponse2);

      expect(result1?.idProducto).toBe(1);
      expect(result2?.idProducto).toBe(2);
    });

    it('should handle product with missing optional fields', () => {
      const productWithMissingFields: ProductInterface = {
        idProducto: 10,
        nombre: 'Producto Mínimo',
        precio: 50000,
        stock: 5,
        activo: true,
        fechaCreacion: '2024-01-15T00:00:00Z',
        idCategoria: 1,
      };
      const mockResponse: ApiResponse<ProductInterface> = {
        value: productWithMissingFields,
      };
      let result: ProductInterface | undefined;

      service.getByIdPublic(10).subscribe(res => {
        result = res;
      });

      const req = httpMock.expectOne(`${apiUrl}/public/10`);
      req.flush(mockResponse);

      expect(result?.descripcion).toBeUndefined();
      expect(result?.imagenUrl).toBeUndefined();
      expect(result?.categoriaNombre).toBeUndefined();
    });
  });
});