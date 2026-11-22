import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { ExpenseFilter } from '../../models';

@Component({
  selector: 'app-expense-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './expense-filter.html',
  styleUrl: './expense-filter.css',
})
export class ExpenseFilterComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);

  @Output() filterChanged = new EventEmitter<ExpenseFilter>();

  readonly categories = signal<string[]>([]);
  readonly rangeError = signal<string | null>(null);

  category = '';
  from = '';
  to = '';

  ngOnInit(): void {
    this.categoryService.list().subscribe((categories) => this.categories.set(categories));
  }

  apply(): void {
    if (this.from && this.to && this.from > this.to) {
      this.rangeError.set("'From' date must not be after 'To' date.");
      return;
    }
    this.rangeError.set(null);

    this.filterChanged.emit({
      category: this.category || null,
      from: this.from || null,
      to: this.to || null,
    });
  }

  clear(): void {
    this.category = '';
    this.from = '';
    this.to = '';
    this.rangeError.set(null);
    this.filterChanged.emit({ category: null, from: null, to: null });
  }
}
