import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Expense, ExpenseFilter, ExpenseRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/expenses`;

  create(request: ExpenseRequest): Observable<Expense> {
    return this.http.post<Expense>(this.baseUrl, request);
  }

  update(id: number, request: ExpenseRequest): Observable<Expense> {
    return this.http.put<Expense>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  list(filter?: ExpenseFilter): Observable<Expense[]> {
    let params = new HttpParams();
    if (filter?.category) {
      params = params.set('category', filter.category);
    }
    if (filter?.from) {
      params = params.set('from', filter.from);
    }
    if (filter?.to) {
      params = params.set('to', filter.to);
    }
    return this.http.get<Expense[]>(this.baseUrl, { params });
  }
}
