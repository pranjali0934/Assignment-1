import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);

  list(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiBaseUrl}/categories`);
  }
}
