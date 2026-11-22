import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ExtractionService } from '../../services/extraction.service';
import { HistoryEntry } from '../../models';

@Component({
  selector: 'app-chat-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-input.html',
  styleUrl: './chat-input.css',
})
export class ChatInputComponent {
  private readonly extractionService = inject(ExtractionService);

  @Output() extracted = new EventEmitter<HistoryEntry>();

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  message = '';

  submit(): void {
    if (!this.message.trim()) {
      this.errorMessage.set('Please describe an expense.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    const submittedMessage = this.message;

    this.extractionService.extract(this.message).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.extracted.emit({
          message: submittedMessage,
          response,
          submittedAt: new Date().toISOString(),
        });
        this.message = '';
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err?.status === 502
            ? 'The AI service is currently unavailable. Please try again shortly.'
            : 'Something went wrong processing your message.',
        );
      },
    });
  }
}
