import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ExpenseFilterComponent } from './expense-filter';

describe('ExpenseFilterComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseFilterComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function createComponent() {
    const fixture = TestBed.createComponent(ExpenseFilterComponent);
    fixture.detectChanges();
    httpMock.expectOne(`${environment.apiBaseUrl}/categories`).flush(['Food', 'Transport']);
    return fixture;
  }

  it('emits the selected category/from/to as filterChanged', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.category = 'Food';
    component.from = '2026-06-01';
    component.to = '2026-06-30';

    let emitted: unknown;
    component.filterChanged.subscribe((value) => (emitted = value));
    component.apply();

    expect(emitted).toEqual({ category: 'Food', from: '2026-06-01', to: '2026-06-30' });
  });

  it('rejects from-after-to without emitting', () => {
    const fixture = createComponent();
    const component = fixture.componentInstance;
    component.from = '2026-06-30';
    component.to = '2026-06-01';

    let emitted = false;
    component.filterChanged.subscribe(() => (emitted = true));
    component.apply();

    expect(emitted).toBe(false);
    expect(component.rangeError()).toContain('must not be after');
  });
});
