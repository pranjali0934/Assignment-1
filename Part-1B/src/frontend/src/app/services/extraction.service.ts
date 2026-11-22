import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ExtractRequest, ExtractResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class ExtractionService {
  private readonly http = inject(HttpClient);

  extract(message: string): Observable<ExtractResponse> {
    const request: ExtractRequest = { message };
    return this.http.post<ExtractResponse>(`${environment.apiBaseUrl}/extract`, request);
  }
}
