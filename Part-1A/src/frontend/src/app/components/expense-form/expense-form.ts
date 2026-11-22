import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { ExpenseService } from '../../services/expense.service';
import { Expense, ExpenseRequest } from '../../models';

@Component({
  selector: 'app-expense-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './expense-form.html',
  styleUrl: './expense-form.css',
})
export class ExpenseFormComponent implements OnInit, OnChanges {
  private readonly expenseService = inject(ExpenseService);
  private readonly categoryService = inject(CategoryService);

  @Input() editingExpense: Expense | null = null;
  @Output() saved = new EventEmitter<void>();

  readonly categories = signal<string[]>([]);
  readonly errorMessage = signal<string | null>(null);

  amount = 0;
  category = '';
  date = '';
  description = '';

  ngOnInit(): void {
    this.categoryService.list().subscribe((categories) => this.categories.set(categories));
    this.resetFromEditingExpense();
  }

  ngOnChanges(): void {
    this.resetFromEditingExpense();
  }

  private resetFromEditingExpense(): void {
    if (this.editingExpense) {
      this.amount = this.editingExpense.amount;
      this.category = this.editingExpense.category;
      this.date = this.editingExpense.date;
      this.description = this.editingExpense.description ?? '';
    }
  }

  /** Client-side pre-validation mirroring backend rules, for UX only (server remains authoritative). */
  get clientValidationError(): string | null {
    if (!Number.isInteger(this.amount)) {
      return 'Amount must be a whole number (no decimals).';
    }
    if (this.amount < 1 || this.amount > 100_000) {
      return 'Amount must be between 1 and 100,000.';
    }
    if (!this.category) {
      return 'Category is required.';
    }
    if (!this.date) {
      return 'Date is required.';
    }
    if (this.date > this.todayIso()) {
      return 'Date cannot be in the future.';
    }
    return null;
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  submit(): void {
    const clientError = this.clientValidationError;
    if (clientError) {
      this.errorMessage.set(clientError);
      return;
    }

    const request: ExpenseRequest = {
      amount: this.amount,
      category: this.category,
      date: this.date,
      description: this.description || null,
    };

    const result = this.editingExpense
      ? this.expenseService.update(this.editingExpense.id, request)
      : this.expenseService.create(request);

    result.subscribe({
      next: () => {
        this.errorMessage.set(null);
        this.saved.emit();
      },
      error: (err) => {
        const serverErrors = err?.error?.errors;
        this.errorMessage.set(
          serverErrors ? Object.values(serverErrors).flat().join(' ') : 'Failed to save expense.',
        );
      },
    });
  }
}
