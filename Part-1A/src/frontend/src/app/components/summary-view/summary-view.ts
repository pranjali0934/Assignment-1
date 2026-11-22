import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BudgetService } from '../../services/budget.service';
import { MonthlySummary } from '../../models';

@Component({
  selector: 'app-summary-view',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './summary-view.html',
  styleUrl: './summary-view.css',
})
export class SummaryViewComponent implements OnInit {
  private readonly budgetService = inject(BudgetService);

  readonly summary = signal<MonthlySummary | null>(null);

  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.budgetService.getMonthlySummary(this.year, this.month).subscribe((summary) => this.summary.set(summary));
  }
}
