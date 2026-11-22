import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ExtractResponse } from '../../models';

@Component({
  selector: 'app-extraction-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './extraction-result.html',
  styleUrl: './extraction-result.css',
})
export class ExtractionResultComponent {
  @Input() result: ExtractResponse | null = null;
}
