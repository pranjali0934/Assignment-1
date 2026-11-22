import { Component, signal } from '@angular/core';
import { ExpenseListComponent } from './components/expense-list/expense-list';
import { SummaryViewComponent } from './components/summary-view/summary-view';
import { BudgetFormComponent } from './components/budget-form/budget-form';

@Component({
  selector: 'app-root',
  imports: [ExpenseListComponent, SummaryViewComponent, BudgetFormComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('expense-tracker-web');
}
