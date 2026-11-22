import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { BudgetFormComponent } from './budget-form';

describe('BudgetFormComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BudgetFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function createComponent() {
    const fixture = TestBed.createComponent(BudgetFormComponent);
    fixture.detectChanges();
    httpMock.expectOne(`${environment.apiBaseUrl}/categories`).flush(['Food', 'Transport']);
    return fixture;
  }

  it('rejects an out-of-range limit amount without calling the API', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.category = 'Food';
    component.limitAmount = 200_000;

    component.submit();

    expect(component.errorMessage()).toContain('between 1 and 100,000');
    httpMock.expectNone(`${environment.apiBaseUrl}/budgets`);
  });

  it('submits a valid budget limit to the API', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.category = 'Food';
    component.year = 2026;
    component.month = 6;
    component.limitAmount = 1000;

    component.submit();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/budgets`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ category: 'Food', year: 2026, month: 6, limitAmount: 1000 });
    req.flush({ id: 1, category: 'Food', year: 2026, month: 6, limitAmount: 1000 });
  });
});
