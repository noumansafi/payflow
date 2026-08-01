import { Component, computed, input } from '@angular/core';

const PALETTE = [
  'bg-indigo-100 text-indigo-700',
  'bg-emerald-100 text-emerald-700',
  'bg-sky-100 text-sky-700',
  'bg-violet-100 text-violet-700',
  'bg-teal-100 text-teal-700',
  'bg-rose-100 text-rose-700',
  'bg-amber-100 text-amber-800',
  'bg-slate-200 text-slate-700',
];

@Component({
  selector: 'app-avatar',
  template: `
    <span
      class="inline-flex shrink-0 items-center justify-center rounded-full font-semibold uppercase select-none"
      [class]="tone() + ' ' + sizeClass()"
      [attr.aria-hidden]="ariaLabel() ? null : true"
      [attr.aria-label]="ariaLabel() || null"
      role="img"
    >
      {{ initials() }}
    </span>
  `,
})
export class Avatar {
  readonly name = input('');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly ariaLabel = input('');

  readonly initials = computed(() => {
    const parts = this.name().trim().split(/\s+/).filter(Boolean);
    if (!parts.length) {
      return '?';
    }
    if (parts.length === 1) {
      return parts[0].slice(0, 2).toUpperCase();
    }
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });

  readonly tone = computed(() => {
    const key = this.name().trim().toLowerCase() || '?';
    let hash = 0;
    for (let i = 0; i < key.length; i++) {
      hash = (hash * 31 + key.charCodeAt(i)) >>> 0;
    }
    return PALETTE[hash % PALETTE.length];
  });

  sizeClass(): string {
    switch (this.size()) {
      case 'sm':
        return 'h-9 w-9 text-xs';
      case 'lg':
        return 'h-14 w-14 text-base';
      default:
        return 'h-11 w-11 text-sm';
    }
  }
}
