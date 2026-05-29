import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { CartService } from './cart.service';
import { environment } from '../../../../environments/environment';
import { CartDto } from '../interfaces/cart.dto';

const mockCartDto: CartDto = {
  id: 1,
  userId: 1,
  isActive: true,
  items: [],
  total: 0,
};

const mockCartResponse = { value: mockCartDto };

describe('CartService', () => {
  let service: CartService;
  let httpMock: HttpTestingController;

  const base = `${environment.apiUrl}/Cart`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(CartService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── getCart() ──────────────────────────────────────────────────────────────

  it('should GET to /Cart', () => {
    service.getCart().subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush(mockCartResponse);
  });

  it('should return the CartDto from the response value', () => {
    let result: CartDto | undefined;
    service.getCart().subscribe(c => (result = c));
    httpMock.expectOne(base).flush(mockCartResponse);
    expect(result).toEqual(mockCartDto);
  });

  it('should propagate HTTP errors from getCart', () => {
    let errorReceived = false;
    service.getCart().subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(base).flush('Error', { status: 500, statusText: 'Server Error' });
    expect(errorReceived).toBe(true);
  });

  // ─── addOrUpdateItem() ──────────────────────────────────────────────────────

  it('should POST to /Cart/items with the given request', () => {
    const request = { productId: 5, quantity: 3 };
    service.addOrUpdateItem(request).subscribe();
    const req = httpMock.expectOne(`${base}/items`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(mockCartResponse);
  });

  it('should return the CartDto from addOrUpdateItem response', () => {
    let result: CartDto | undefined;
    service.addOrUpdateItem({ productId: 5, quantity: 3 }).subscribe(c => (result = c));
    httpMock.expectOne(`${base}/items`).flush(mockCartResponse);
    expect(result).toEqual(mockCartDto);
  });

  it('should propagate HTTP errors from addOrUpdateItem', () => {
    let errorReceived = false;
    service.addOrUpdateItem({ productId: 5, quantity: 3 }).subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(`${base}/items`).flush('Error', { status: 400, statusText: 'Bad Request' });
    expect(errorReceived).toBe(true);
  });

  // ─── removeItem() ───────────────────────────────────────────────────────────

  it('should DELETE to /Cart/items/:productId', () => {
    service.removeItem(5).subscribe();
    const req = httpMock.expectOne(`${base}/items/5`);
    expect(req.request.method).toBe('DELETE');
    req.flush(mockCartResponse);
  });

  it('should return the CartDto from removeItem response', () => {
    let result: CartDto | undefined;
    service.removeItem(5).subscribe(c => (result = c));
    httpMock.expectOne(`${base}/items/5`).flush(mockCartResponse);
    expect(result).toEqual(mockCartDto);
  });

  it('should propagate HTTP errors from removeItem', () => {
    let errorReceived = false;
    service.removeItem(5).subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(`${base}/items/5`).flush('Error', { status: 404, statusText: 'Not Found' });
    expect(errorReceived).toBe(true);
  });

  // ─── clearCart() ────────────────────────────────────────────────────────────

  it('should DELETE to /Cart', () => {
    service.clearCart().subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should complete when clearCart succeeds', () => {
    let completed = false;
    service.clearCart().subscribe({ complete: () => (completed = true) });
    httpMock.expectOne(base).flush(null);
    expect(completed).toBe(true);
  });

  it('should propagate HTTP errors from clearCart', () => {
    let errorReceived = false;
    service.clearCart().subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(base).flush('Error', { status: 500, statusText: 'Server Error' });
    expect(errorReceived).toBe(true);
  });
});