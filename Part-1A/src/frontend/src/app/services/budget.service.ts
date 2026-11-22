import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BudgetLimit, BudgetLimitRequest, MonthlySummary } from '../models';

@Injectable({ providedIn: 'root' })
export class BudgetService {
  private readonly http = inject(HttpClient);

  setLimit(request: BudgetLimitRequest): Observable<BudgetLimit> {
    return this.http.post<BudgetLimit>(`${environment.apiBaseUrl}/budgets`, request);
  }

  listForMonth(year: number, month: number): Observable<BudgetLimit[]> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<BudgetLimit[]>(`${environment.apiBaseUrl}/budgets`, { params });
  }

  getMonthlySummary(year: number, month: number): Observable<MonthlySummary> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<MonthlySummary>(`${environment.apiBaseUrl}/summary`, { params });
  }
}
