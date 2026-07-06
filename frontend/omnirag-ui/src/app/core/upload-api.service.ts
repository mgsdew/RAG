import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { UploadRecord } from './upload-record.model';

@Injectable({ providedIn: 'root' })
export class UploadApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/uploads`;

  getAll(): Observable<UploadRecord[]> {
    return this.http.get<UploadRecord[]>(this.baseUrl);
  }

  create(formData: FormData): Observable<UploadRecord> {
    return this.http.post<UploadRecord>(this.baseUrl, formData);
  }

  update(id: string, formData: FormData): Observable<UploadRecord> {
    return this.http.put<UploadRecord>(`${this.baseUrl}/${id}`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
