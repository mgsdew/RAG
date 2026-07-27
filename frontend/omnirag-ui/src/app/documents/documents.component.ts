import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadApiService } from '../core/upload-api.service';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'documents-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './documents.component.html'
})
export class DocumentsComponent {
  private uploadApi = inject(UploadApiService);
  auth = inject(AuthService);

  private selectedFile = signal<File|undefined>(undefined);
  role: string = this.auth.getSignedInRole();

  get canUpload(): boolean {
    return this.role === 'Document.Uploader';
  }

  get currentFile(): File | undefined {
    return this.selectedFile();
  }

  get currentFileName(): string {
    return this.currentFile?.name ?? 'No file selected';
  }

  get profile() {
    return this.auth.getProfile();
  }

  onFile(e: Event) {
    const input = e.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0]);
  }

  async upload() {
    const file = this.selectedFile();
    if (!file) return;
    const fd = new FormData();
    fd.append('file', file);
    try {
      await this.uploadApi.create(fd).toPromise();
      alert('Uploaded');
    } catch (e) {
      console.error(e);
      alert('Upload failed');
    }
  }

  login() {
    void this.auth.login();
  }
}
