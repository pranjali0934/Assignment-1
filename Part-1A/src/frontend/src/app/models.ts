export interface Expense {
  id: number;
  amount: number;
  category: string;
  date: string; // ISO date (yyyy-MM-dd)
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ExpenseRequest {
  amount: number;
  category: string;
  date: string;
  description: string | null;
}

export interface ExpenseFilter {
  category: string | null;
  from: string | null;
  to: string | null;
}

export interface CategorySummary {
  category: string;
  total: number;
  budgetLimit: number | null;
  overBudget: boolean;
}

export interface MonthlySummary {
  year: number;
  month: number;
  total: number;
  byCategory: CategorySummary[];
}

export interface BudgetLimitRequest {
  category: string;
  year: number;
  month: number;
  limitAmount: number;
}

export interface BudgetLimit {
  id: number;
  category: string;
  year: number;
  month: number;
  limitAmount: number;
}

export interface ApiErrors {
  errors: Record<string, string[]>;
}
