import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { SummaryViewComponent } from './summary-view';
import { MonthlySummary } from '../../models';

describe('SummaryViewComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SummaryViewComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('loads and renders totals, per-category breakdown, and overspending indicator', () => {
    const fixture = TestBed.createComponent(SummaryViewComponent);
    fixture.detectChanges();

    const req = httpMock.expectOne((r) => r.url === `${environment.apiBaseUrl}/summary`);
    const summary: MonthlySummary = {
      year: 2026,
      month: 6,
      total: 800,
      byCategory: [{ category: 'Food', total: 800, budgetLimit: 500, overBudget: true }],
    };
    req.flush(summary);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('800');
    expect(text).toContain('Food');
    expect(text).toContain('Over budget');
  });
});
