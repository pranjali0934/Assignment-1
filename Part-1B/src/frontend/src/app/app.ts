import { Component, computed, signal } from '@angular/core';
import { ChatInputComponent } from './components/chat-input/chat-input';
import { ExtractedExpenseResult, HistoryEntry } from './models';

@Component({
  selector: 'app-root',
  imports: [ChatInputComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('expense-assistant-web');

  /** In-session only (not persisted across reloads) — Part 1B is a standalone extraction
   * assistant, not a tracker; this list exists purely so a demo/manual test session can see
   * more than just the latest result at a glance. */
  protected readonly history = signal<HistoryEntry[]>([]);

  /** Every extracted expense across all submissions, flattened into one continuous list, newest first. */
  protected readonly allExpenses = computed<ExtractedExpenseResult[]>(() =>
    [...this.history()].reverse().flatMap((entry) => entry.response.expenses),
  );

  /** Submissions that needed clarification (no expense extracted), newest first. */
  protected readonly clarifications = computed(() =>
    [...this.history()]
      .reverse()
      .filter((entry) => entry.response.clarificationNeeded),
  );

  protected readonly sessionTotal = computed(() =>
    this.allExpenses().reduce((sum, e) => sum + e.amount, 0),
  );

  onExtracted(entry: HistoryEntry): void {
    this.history.update((current) => [...current, entry]);
  }
}
