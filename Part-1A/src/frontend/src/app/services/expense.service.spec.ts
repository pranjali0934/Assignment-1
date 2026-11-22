import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { ExpenseService } from './expense.service';
import { ExpenseRequest } from '../models';

describe('ExpenseService', () => {
  let service: ExpenseService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ExpenseService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('create() posts to /expenses', () => {
    const request: ExpenseRequest = { amount: 500, category: 'Food', date: '2026-06-01', description: null };

    service.create(request).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/expenses`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('update() puts to /expenses/{id}', () => {
    const request: ExpenseRequest = { amount: 750, category: 'Food', date: '2026-06-01', description: null };

    service.update(42, request).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/expenses/42`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('delete() sends DELETE to /expenses/{id}', () => {
    service.delete(42).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/expenses/42`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('list() includes category/from/to query params when provided', () => {
    service.list({ category: 'Food', from: '2026-06-01', to: '2026-06-30' }).subscribe();

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiBaseUrl}/expenses` && r.params.get('category') === 'Food',
    );
    expect(req.request.params.get('from')).toBe('2026-06-01');
    expect(req.request.params.get('to')).toBe('2026-06-30');
    req.flush([]);
  });
});
