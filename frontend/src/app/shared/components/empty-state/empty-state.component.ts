import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `
    <div class="empty-state">
      <span class="empty-state__icon" aria-hidden="true">{{ icon() }}</span>
      <h3 class="empty-state__title">{{ title() }}</h3>
      @if (message()) {
        <p class="empty-state__message">{{ message() }}</p>
      }
    </div>
  `,
  styles: [
    `
      .empty-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        gap: var(--space-sm);
        padding: var(--space-xxl) var(--space-lg);
      }

      .empty-state__icon {
        font-size: 48px;
        line-height: 1;
      }

      .empty-state__title {
        color: var(--color-ink);
      }

      .empty-state__message {
        color: var(--color-muted);
        max-width: 420px;
      }
    `
  ]
})
export class EmptyStateComponent {
  readonly icon = input<string>('\u2708');
  readonly title = input.required<string>();
  readonly message = input<string>('');
}
