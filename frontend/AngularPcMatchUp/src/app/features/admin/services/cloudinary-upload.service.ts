import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CloudinaryUploadService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Producto/upload-image`;

  upload(file: File): Observable<string> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(this.base, form).pipe(
      map((res: { url: string }) => res.url)
    );
  }
}