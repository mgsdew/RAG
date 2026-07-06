import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { UploadApiService } from './core/upload-api.service';
import { UploadRecord } from './core/upload-record.model';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly uploadApi = inject(UploadApiService);

  protected readonly title = signal('OmniRAG');
  protected readonly uploads = signal<UploadRecord[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly error = signal('');
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly selectedUploadId = signal('');
  protected readonly fileName = signal('');
  protected readonly uploadDate = signal(this.toDateInputValue(new Date()));

  protected readonly modeLabel = computed(() => this.selectedUploadId() ? 'Update upload' : 'Create upload');

  constructor() {
    void this.loadUploads();
  }

  protected async loadUploads(): Promise<void> {
    this.isLoading.set(true);
    this.error.set('');

    try {
      const records = await firstValueFrom(this.uploadApi.getAll());
      this.uploads.set(records);
    } catch {
      this.error.set('Unable to load uploads from the backend API.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  protected beginEdit(upload: UploadRecord): void {
    this.selectedUploadId.set(upload.id);
    this.fileName.set(upload.fileName);
    this.uploadDate.set(upload.uploadedAt.slice(0, 10));
  }

  protected cancelEdit(): void {
    this.selectedUploadId.set('');
    this.fileName.set('');
    this.selectedFile.set(null);
    this.uploadDate.set(this.toDateInputValue(new Date()));
  }

  protected async saveUpload(): Promise<void> {
    const file = this.selectedFile();
    const existingFileName = this.fileName().trim();

    if (!file && !existingFileName) {
      this.error.set('Select a CSV or Excel file before saving.');
      return;
    }

    const formData = new FormData();
    if (file) {
      formData.append('File', file);
    }
    if (existingFileName) {
      formData.append('FileName', existingFileName);
    }
    formData.append('UploadedAt', this.uploadDate());

    this.isSaving.set(true);
    this.error.set('');

    try {
      if (this.selectedUploadId()) {
        await firstValueFrom(this.uploadApi.update(this.selectedUploadId(), formData));
      } else {
        await firstValueFrom(this.uploadApi.create(formData));
      }

      this.cancelEdit();
      await this.loadUploads();
    } catch {
      this.error.set('Save failed. Check the backend API and file type.');
    } finally {
      this.isSaving.set(false);
    }
  }

  protected async removeUpload(id: string): Promise<void> {
    this.error.set('');

    try {
      await firstValueFrom(this.uploadApi.delete(id));
      await this.loadUploads();
    } catch {
      this.error.set('Delete failed.');
    }
  }

  private toDateInputValue(date: Date): string {
    return date.toISOString().slice(0, 10);
  }
}
