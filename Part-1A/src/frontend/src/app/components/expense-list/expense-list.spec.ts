import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ExpenseListComponent } from './expense-list';
import { Expense } from '../../models';

describe('ExpenseListComponent', () => {
  let httpMock: HttpTestingController;

  const sampleExpense: Expense = {
    id: 1,
    amount: 500,
    category: 'Food',
    date: '2026-06-01',
    description: 'Lunch',
    createdAt: '2026-06-01T00:00:00Z',
    updatedAt: '2026-06-01T00:00:00Z',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseListComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function createComponent() {
    const fixture = TestBed.createComponent(ExpenseListComponent);
    fixture.detectChanges();
    httpMock.expectOne((r) => r.url === `${environment.apiBaseUrl}/expenses`).flush([sampleExpense]);
    // Both the filter and form child components request the category list independently.
    httpMock.match(`${environment.apiBaseUrl}/categories`).forEach((req) => req.flush(['Food', 'Transport']));
    return fixture;
  }

  it('loads and displays expenses on init', () => {
    const fixture = createComponent();

    expect(fixture.componentInstance.expenses()).toEqual([sampleExpense]);
  });

  it('edit() sets the expense being edited', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.edit(sampleExpense);

    expect(component.editingExpense()).toEqual(sampleExpense);
  });

  it('delete() calls the API and reloads the list', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;

    component.delete(sampleExpense);

    const deleteReq = httpMock.expectOne(`${environment.apiBaseUrl}/expenses/1`);
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null);

    httpMock.expectOne((r) => r.url === `${environment.apiBaseUrl}/expenses`).flush([]);
    expect(component.expenses()).toEqual([]);
  });
});
