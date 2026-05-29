import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';

import { ProductsApiService } from './products-api.service';

import { environment } from '../../../../environments/environment';

import { ProductInterface } from '../interfaces/product.interface';

describe('ProductsApiService', () => {
  let service: ProductsApiService;
  let httpMock: HttpTestingController;

  const baseUrl = `${environment.apiUrl}/Producto`;

  const mockProduct: ProductInterface = {
    idProducto: 1,
    nombre: 'Laptop Gamer',
    descripcion: 'RTX 4070',
    precio: 5000000,
    stock: 10,
    imagenUrl: 'https://img.com/laptop.jpg',
    activo: true,
    fechaCreacion: '2026-01-01',
    idCategoria: 1,
    categoriaNombre: 'Gaming',
  };

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

  // ─────────────────────────────
  // Creación
  // ─────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─────────────────────────────
  // getAll
  // ─────────────────────────────

  it('should get all products', () => {
    const mockResponse = {
      value: [mockProduct],
    };

    service.getAll().subscribe(products => {
      expect(products.length).toBe(1);
      expect(products[0]).toEqual(mockProduct);
    });

    const req = httpMock.expectOne(baseUrl);

    expect(req.request.method).toBe('GET');

    req.flush(mockResponse);
  });

  it('should return empty array when no products exist', () => {
    service.getAll().subscribe(products => {
      expect(products).toEqual([]);
    });

    const req = httpMock.expectOne(baseUrl);

    req.flush({
      value: [],
    });
  });

  // ─────────────────────────────
  // getById
  // ─────────────────────────────

  it('should get product by id', () => {
    service.getById(1).subscribe(product => {
      expect(product).toEqual(mockProduct);
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);

    expect(req.request.method).toBe('GET');

    req.flush({
      value: mockProduct,
    });
  });

  // ─────────────────────────────
  // getByIdPublic
  // ─────────────────────────────

  it('should get public product by id', () => {
    service.getByIdPublic(1).subscribe(product => {
      expect(product).toEqual(mockProduct);
    });

    const req = httpMock.expectOne(`${baseUrl}/public/1`);

    expect(req.request.method).toBe('GET');

    req.flush({
      value: mockProduct,
    });
  });

  // ─────────────────────────────
  // Error cases
  // ─────────────────────────────

  it('should handle getAll error', () => {
    service.getAll().subscribe({
      next: () => fail('should fail'),
      error: error => {
        expect(error.status).toBe(500);
      },
    });

    const req = httpMock.expectOne(baseUrl);

    req.flush(
      { message: 'Internal Server Error' },
      {
        status: 500,
        statusText: 'Server Error',
      }
    );
  });

  it('should handle getById error', () => {
    service.getById(999).subscribe({
      next: () => fail('should fail'),
      error: error => {
        expect(error.status).toBe(404);
      },
    });

    const req = httpMock.expectOne(`${baseUrl}/999`);

    req.flush(
      { message: 'Not Found' },
      {
        status: 404,
        statusText: 'Not Found',
      }
    );
  });

  it('should handle getByIdPublic error', () => {
    service.getByIdPublic(999).subscribe({
      next: () => fail('should fail'),
      error: error => {
        expect(error.status).toBe(404);
      },
    });

    const req = httpMock.expectOne(`${baseUrl}/public/999`);

    req.flush(
      { message: 'Not Found' },
      {
        status: 404,
        statusText: 'Not Found',
      }
    );
  });
});