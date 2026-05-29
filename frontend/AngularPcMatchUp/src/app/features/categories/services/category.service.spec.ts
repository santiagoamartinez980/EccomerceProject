import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';


import { environment } from '../../../../environments/environment';
import { CategoryInterface } from '../interfaces/category.interface';
import { AdminCategoriesService } from './category.service';

const mockCategory: CategoryInterface = { categoryId: 1, name: 'Periféricos' };
const mockCategory2: CategoryInterface = { categoryId: 2, name: 'Monitores' };

describe('AdminCategoriesService', () => {
  let service: AdminCategoriesService;
  let httpMock: HttpTestingController;

  const base = `${environment.apiUrl}/Category`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AdminCategoriesService);
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

  // ─── getAll() ───────────────────────────────────────────────────────────────

  it('should GET to /Category', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush({ value: [mockCategory, mockCategory2] });
  });

  it('should return the array of categories from the response value', () => {
    let result: CategoryInterface[] | undefined;
    service.getAll().subscribe(c => (result = c));
    httpMock.expectOne(base).flush({ value: [mockCategory, mockCategory2] });
    expect(result).toEqual([mockCategory, mockCategory2]);
  });

  it('should return an empty array when value is empty', () => {
    let result: CategoryInterface[] | undefined;
    service.getAll().subscribe(c => (result = c));
    httpMock.expectOne(base).flush({ value: [] });
    expect(result).toEqual([]);
  });

  it('should propagate HTTP errors from getAll', () => {
    let errorReceived = false;
    service.getAll().subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(base).flush('Error', { status: 500, statusText: 'Server Error' });
    expect(errorReceived).toBe(true);
  });

  // ─── create() ───────────────────────────────────────────────────────────────

  it('should POST to /Category with the given payload', () => {
    const payload = { name: 'Teclados' };
    service.create(payload).subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ value: { categoryId: 3, name: 'Teclados' } });
  });

  it('should return the created CategoryInterface from the response value', () => {
    const payload = { name: 'Teclados' };
    const created: CategoryInterface = { categoryId: 3, name: 'Teclados' };
    let result: CategoryInterface | undefined;
    service.create(payload).subscribe(c => (result = c));
    httpMock.expectOne(base).flush({ value: created });
    expect(result).toEqual(created);
  });

  it('should propagate HTTP errors from create', () => {
    let errorReceived = false;
    service.create({ name: 'Teclados' }).subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(base).flush('Error', { status: 400, statusText: 'Bad Request' });
    expect(errorReceived).toBe(true);
  });

  // ─── delete() ───────────────────────────────────────────────────────────────

  it('should DELETE to /Category/:id', () => {
    service.delete(1).subscribe();
    const req = httpMock.expectOne(`${base}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ mensaje: 'Categoría eliminada' });
  });

  it('should return the mensaje from the delete response', () => {
    let result: { mensaje: string } | undefined;
    service.delete(1).subscribe(r => (result = r));
    httpMock.expectOne(`${base}/1`).flush({ mensaje: 'Categoría eliminada' });
    expect(result).toEqual({ mensaje: 'Categoría eliminada' });
  });

  it('should propagate HTTP errors from delete', () => {
    let errorReceived = false;
    service.delete(1).subscribe({ error: () => (errorReceived = true) });
    httpMock.expectOne(`${base}/1`).flush('Error', { status: 404, statusText: 'Not Found' });
    expect(errorReceived).toBe(true);
  });
});