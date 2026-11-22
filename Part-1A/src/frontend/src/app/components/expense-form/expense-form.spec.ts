import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ExpenseFormComponent } from './expense-form';

describe('ExpenseFormComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function createComponent() {
    const fixture = TestBed.createComponent(ExpenseFormComponent);
    fixture.detectChanges();
    httpMock.expectOne(`${environment.apiBaseUrl}/categories`).flush(['Food', 'Transport']);
    return fixture;
  }

  it('rejects a decimal amount client-side without calling the API', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.amount = 500.5;
    component.category = 'Food';
    component.date = new Date().toISOString().slice(0, 10);

    component.submit();

    expect(component.errorMessage()).toContain('whole number');
    httpMock.expectNone(`${environment.apiBaseUrl}/expenses`);
  });

  it('rejects a future date client-side without calling the API', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.amount = 500;
    component.category = 'Food';
    const future = new Date();
    future.setDate(future.getDate() + 1);
    component.date = future.toISOString().slice(0, 10);

    component.submit();

    expect(component.errorMessage()).toContain('future');
    httpMock.expectNone(`${environment.apiBaseUrl}/expenses`);
  });

  it('submits a valid expense to the API', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.amount = 500;
    component.category = 'Food';
    component.date = new Date().toISOString().slice(0, 10);

    component.submit();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/expenses`);
    expect(req.request.method).toBe('POST');
    req.flush({ id: 1, amount: 500, category: 'Food', date: component.date, description: null });
  });
});
