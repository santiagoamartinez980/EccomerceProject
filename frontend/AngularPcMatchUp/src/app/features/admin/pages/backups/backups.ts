import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { BackupFile, DatabaseBackupService } from '../../services/database-backup.service';

@Component({
  selector: 'app-backups',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './backups.html',
  styleUrls: ['./backups.css'],
})
export class Backups implements OnInit {
  private readonly service = inject(DatabaseBackupService);
  files = signal<BackupFile[]>([]);
  busy = signal(false);
  error = signal('');
  info = signal('');

  ngOnInit() { this.refresh(); }

  refresh() {
    this.error.set('');
    this.service.list().subscribe({
      next: files => this.files.set(files),
      error: err => this.error.set(err.error?.message || 'No se pudieron cargar las copias.'),
    });
  }

  create() {
    this.busy.set(true);
    this.error.set('');
    this.info.set('');
    this.service.create().subscribe({
      next: file => {
        this.busy.set(false);
        this.info.set(`Copia ${file.name} creada. Descárguela y guárdela fuera del servidor.`);
        this.refresh();
      },
      error: err => {
        this.busy.set(false);
        this.error.set(err.error?.message || 'No se pudo crear la copia.');
      },
    });
  }

  download(file: BackupFile) {
    this.service.download(file.name).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = file.name;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.error.set('No se pudo descargar la copia.'),
    });
  }

  restore(file: BackupFile) {
    if (!window.confirm(`¿Restaurar ${file.name}? Se reemplazarán todos los datos actuales de la base y se cerrarán las conexiones activas.`)) return;
    this.busy.set(true);
    this.error.set('');
    this.info.set('');
    this.service.restore(file.name).subscribe({
      next: result => {
        this.busy.set(false);
        localStorage.removeItem('token');
        this.info.set(result.message);
      },
      error: err => {
        this.busy.set(false);
        this.error.set(err.error?.message || 'Falló la restauración.');
      },
    });
  }
}
