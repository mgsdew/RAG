import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { UploadRecord } from './upload-record.model';

@Injectable({ providedIn: 'root' })
export class UploadApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Documents`;

  getAll(): Observable<UploadRecord[]> {
    return this.http.get<UploadRecord[]>(this.baseUrl);
  }

  create(formData: FormData): Observable<any> {
    // The backend accepts a single file parameter named 'file' on POST api/Documents/upload
    return this.http.post<any>(`${this.baseUrl}/upload`, formData);
  }

  update(id: string, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
