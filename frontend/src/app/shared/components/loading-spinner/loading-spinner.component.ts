import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `
    <div class="spinner-wrap" role="status" aria-live="polite">
      <span class="spinner" aria-hidden="true"></span>
      @if (label()) {
        <span class="spinner-label">{{ label() }}</span>
      }
    </div>
  `,
  styles: [
    `
      .spinner-wrap {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-md);
        padding: var(--space-xxl) 0;
      }

      .spinner {
        width: 40px;
        height: 40px;
        border: 3px solid var(--color-primary-disabled);
        border-top-color: var(--color-primary);
        border-radius: var(--rounded-pill);
        animation: spinner-rotate 0.8s linear infinite;
      }

      .spinner-label {
        color: var(--color-muted);
        font-size: 14px;
      }

      @keyframes spinner-rotate {
        to {
          transform: rotate(360deg);
        }
      }
    `
  ]
})
export class LoadingSpinnerComponent {
  readonly label = input<string>('');
}
