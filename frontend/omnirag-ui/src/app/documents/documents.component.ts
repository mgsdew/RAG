import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UploadApiService } from '../core/upload-api.service';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'documents-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Document Management</h2>
    <div *ngIf="!auth.isAuthenticated()">
      <button (click)="login()">Login</button>
    </div>

    <div *ngIf="auth.isAuthenticated()">
      <p>Logged in. Roles: {{ role }}</p>
      <div *ngIf="profile">
        <p>Welcome, {{ profile?.Profile?.Name ?? profile?.name ?? 'User' }}</p>
      </div>
      <input type="file" (change)="onFile($event)" />
      <button (click)="upload()" [disabled]="!canUpload">Upload</button>
    </div>
  `
})
export class DocumentsComponent {
  private uploadApi = inject(UploadApiService);
  auth = inject(AuthService);

  private selectedFile = signal<File|undefined>(undefined);
  role: string = this.auth.getRolesText();

  get canUpload(): boolean {
    return this.role === 'Document.Uploader';
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
