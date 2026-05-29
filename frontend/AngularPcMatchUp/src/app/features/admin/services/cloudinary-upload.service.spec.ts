import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { CloudinaryUploadService } from './cloudinary-upload.service';
import { environment } from '../../../../environments/environment';

describe('CloudinaryUploadService', () => {
  let service: CloudinaryUploadService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(CloudinaryUploadService);
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

  // ─── upload() ───────────────────────────────────────────────────────────────

  it('should POST to the correct endpoint', () => {
    const file = new File(['contenido'], 'imagen.png', { type: 'image/png' });

    service.upload(file).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Producto/upload-image`);
    expect(req.request.method).toBe('POST');
    req.flush({ url: 'https://res.cloudinary.com/test/imagen.png' });
  });

  it('should send the file inside a FormData with key "file"', () => {
    const file = new File(['contenido'], 'imagen.png', { type: 'image/png' });

    service.upload(file).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Producto/upload-image`);
    expect(req.request.body).toBeInstanceOf(FormData);
    expect(req.request.body.get('file')).toBe(file);
    req.flush({ url: 'https://res.cloudinary.com/test/imagen.png' });
  });

  it('should return the url string from the response', () => {
    const file = new File(['contenido'], 'imagen.png', { type: 'image/png' });
    const mockUrl = 'https://res.cloudinary.com/test/imagen.png';
    let result: string | undefined;

    service.upload(file).subscribe(url => (result = url));

    const req = httpMock.expectOne(`${environment.apiUrl}/Producto/upload-image`);
    req.flush({ url: mockUrl });

    expect(result).toBe(mockUrl);
  });

  it('should propagate HTTP errors', () => {
    const file = new File(['contenido'], 'imagen.png', { type: 'image/png' });
    let errorReceived = false;

    service.upload(file).subscribe({
      next: () => fail('debería haber fallado'),
      error: () => (errorReceived = true),
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Producto/upload-image`);
    req.flush('Error del servidor', { status: 500, statusText: 'Internal Server Error' });

    expect(errorReceived).toBe(true);
  });
});