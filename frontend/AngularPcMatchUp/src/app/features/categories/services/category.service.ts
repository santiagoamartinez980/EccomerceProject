// features/admin/services/admin-categories.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { CreateCategoryDto } from '../interfaces/create-category.dto';
import { CategoryInterface } from '../interfaces/category.interface';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminCategoriesService {
  private http=inject(HttpClient);
  private apiUrl=environment.apiUrl;

  getAll(): Observable<CategoryInterface[]> {
    return this.http
      .get<{ value: CategoryInterface[] }>(`${this.apiUrl}/Category`)
      .pipe(map((r) => r.value));
  }

  create(payload: CreateCategoryDto): Observable<CategoryInterface> {
    return this.http
      .post<{ value: CategoryInterface }>(`${this.apiUrl}/Category`, payload)
      .pipe(map((r) => r.value));
  }

  delete(id: number): Observable<{ mensaje: string }> {
    return this.http.delete<{ mensaje: string }>(`${this.apiUrl}/Category/${id}`);
  }
}