import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  template: `
    <span [class]="badgeClass()" [attr.aria-label]="'Status: ' + label()">
      {{ label() }}
    </span>
  `,
})
export class StatusBadge {
  readonly status = input.required<string>();

  readonly label = computed(() => this.status() || 'Unknown');

  readonly badgeClass = computed(() => {
    const key = this.status().toLowerCase();
    if (key === 'completed' || key === 'success' || key === 'active') {
      return 'chip-success';
    }
    if (key === 'pending' || key === 'processing') {
      return 'chip-warning';
    }
    if (key === 'failed' || key === 'cancelled' || key === 'frozen' || key === 'closed') {
      return 'chip-danger';
    }
    return 'chip-muted';
  });
}
