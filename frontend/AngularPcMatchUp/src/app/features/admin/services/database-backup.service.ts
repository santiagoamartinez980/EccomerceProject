import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';

export interface BackupFile {
  name: string;
  bytes: number;
  createdUtc: string;
}

@Injectable({ providedIn: 'root' })
export class DatabaseBackupService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin/backups`;

  list() { return this.http.get<BackupFile[]>(this.base); }
  create() { return this.http.post<BackupFile>(this.base, {}); }
  download(name: string) { return this.http.get(`${this.base}/${encodeURIComponent(name)}`, { responseType: 'blob' }); }
  restore(name: string) { return this.http.post<{ message: string }>(`${this.base}/${encodeURIComponent(name)}/restore`, {}); }
}
