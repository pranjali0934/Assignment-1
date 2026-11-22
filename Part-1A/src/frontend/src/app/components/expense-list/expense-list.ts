import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ExpenseFilterComponent } from '../expense-filter/expense-filter';
import { ExpenseFormComponent } from '../expense-form/expense-form';
import { ExpenseService } from '../../services/expense.service';
import { Expense, ExpenseFilter } from '../../models';

@Component({
  selector: 'app-expense-list',
  standalone: true,
  imports: [CommonModule, ExpenseFilterComponent, ExpenseFormComponent],
  templateUrl: './expense-list.html',
  styleUrl: './expense-list.css',
})
export class ExpenseListComponent implements OnInit {
  private readonly expenseService = inject(ExpenseService);

  readonly expenses = signal<Expense[]>([]);
  readonly editingExpense = signal<Expense | null>(null);
  private currentFilter: ExpenseFilter = { category: null, from: null, to: null };

  ngOnInit(): void {
    this.reload();
  }

  onFilterChanged(filter: ExpenseFilter): void {
    this.currentFilter = filter;
    this.reload();
  }

  private reload(): void {
    this.expenseService.list(this.currentFilter).subscribe((expenses) => this.expenses.set(expenses));
  }

  edit(expense: Expense): void {
    this.editingExpense.set(expense);
  }

  cancelEdit(): void {
    this.editingExpense.set(null);
  }

  onSaved(): void {
    this.editingExpense.set(null);
    this.reload();
  }

  delete(expense: Expense): void {
    this.expenseService.delete(expense.id).subscribe(() => this.reload());
  }
}
