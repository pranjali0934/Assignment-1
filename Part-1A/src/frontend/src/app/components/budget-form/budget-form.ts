import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BudgetService } from '../../services/budget.service';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-budget-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './budget-form.html',
  styleUrl: './budget-form.css',
})
export class BudgetFormComponent implements OnInit {
  private readonly budgetService = inject(BudgetService);
  private readonly categoryService = inject(CategoryService);

  @Output() saved = new EventEmitter<void>();

  readonly categories = signal<string[]>([]);
  readonly errorMessage = signal<string | null>(null);

  category = '';
  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;
  limitAmount = 0;

  ngOnInit(): void {
    this.categoryService.list().subscribe((categories) => this.categories.set(categories));
  }

  submit(): void {
    if (!Number.isInteger(this.limitAmount) || this.limitAmount < 1 || this.limitAmount > 100_000) {
      this.errorMessage.set('Limit amount must be a whole number between 1 and 100,000.');
      return;
    }
    if (!this.category) {
      this.errorMessage.set('Category is required.');
      return;
    }

    this.budgetService
      .setLimit({ category: this.category, year: this.year, month: this.month, limitAmount: this.limitAmount })
      .subscribe({
        next: () => {
          this.errorMessage.set(null);
          this.saved.emit();
        },
        error: (err) => {
          const serverErrors = err?.error?.errors;
          this.errorMessage.set(
            serverErrors ? Object.values(serverErrors).flat().join(' ') : 'Failed to set budget limit.',
          );
        },
      });
  }
}
