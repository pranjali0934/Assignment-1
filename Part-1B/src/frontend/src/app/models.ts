export interface ExtractedExpenseResult {
  amount: number;
  category: string;
  date: string;
  description: string | null;
}

export interface ExtractResponse {
  expenses: ExtractedExpenseResult[];
  clarificationNeeded: boolean;
  clarificationQuestion: string | null;
}

export interface ExtractRequest {
  message: string;
}

export interface HistoryEntry {
  message: string;
  response: ExtractResponse;
  submittedAt: string;
}
