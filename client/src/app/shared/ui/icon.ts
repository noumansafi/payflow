import { Component, input } from '@angular/core';

export type IconName =
  | 'home'
  | 'send'
  | 'receive'
  | 'activity'
  | 'people'
  | 'user'
  | 'user-plus'
  | 'bell'
  | 'eye'
  | 'eye-off'
  | 'plus'
  | 'wallet'
  | 'statement'
  | 'search'
  | 'arrow-up'
  | 'arrow-down'
  | 'arrow-left'
  | 'check'
  | 'x'
  | 'backspace'
  | 'chevron-right'
  | 'download';

@Component({
  selector: 'app-icon',
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.75"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
      class="shrink-0"
    >
      @switch (name()) {
        @case ('home') {
          <path d="M3 10.5 12 3l9 7.5" />
          <path d="M5 10v10h5v-6h4v6h5V10" />
        }
        @case ('send') {
          <path d="M12 19V5" />
          <path d="m6 11 6-6 6 6" />
        }
        @case ('receive') {
          <path d="M12 5v14" />
          <path d="m6 13 6 6 6-6" />
        }
        @case ('activity') {
          <path d="M8 6h13" />
          <path d="M8 12h13" />
          <path d="M8 18h13" />
          <path d="M3 6h.01" />
          <path d="M3 12h.01" />
          <path d="M3 18h.01" />
        }
        @case ('people') {
          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
          <circle cx="9" cy="7" r="4" />
          <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
          <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        }
        @case ('user') {
          <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" />
          <circle cx="12" cy="7" r="4" />
        }
        @case ('user-plus') {
          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
          <circle cx="9" cy="7" r="4" />
          <path d="M19 8v6" />
          <path d="M22 11h-6" />
        }
        @case ('bell') {
          <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
          <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
        }
        @case ('eye') {
          <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
          <circle cx="12" cy="12" r="3" />
        }
        @case ('eye-off') {
          <path d="M10.6 10.6a2 2 0 1 0 2.8 2.8" />
          <path d="M16.1 16.1A10.9 10.9 0 0 1 12 19c-6.5 0-10-7-10-7a18.5 18.5 0 0 1 5.1-5.3" />
          <path d="M9.9 4.2A10.9 10.9 0 0 1 12 5c6.5 0 10 7 10 7a18.4 18.4 0 0 1-2.2 3.2" />
          <path d="m2 2 20 20" />
        }
        @case ('plus') {
          <path d="M12 5v14" />
          <path d="M5 12h14" />
        }
        @case ('wallet') {
          <path d="M19 7V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-1" />
          <path d="M3 11h18a1 1 0 0 1 1 1v4a1 1 0 0 1-1 1H3" />
          <circle cx="17" cy="14" r="1" />
        }
        @case ('statement') {
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z" />
          <path d="M14 2v6h6" />
          <path d="M8 13h8" />
          <path d="M8 17h6" />
        }
        @case ('download') {
          <path d="M12 3v12" />
          <path d="m7 10 5 5 5-5" />
          <path d="M5 21h14" />
        }
        @case ('search') {
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3.5-3.5" />
        }
        @case ('arrow-up') {
          <path d="M12 19V5" />
          <path d="m6 11 6-6 6 6" />
        }
        @case ('arrow-down') {
          <path d="M12 5v14" />
          <path d="m6 13 6 6 6-6" />
        }
        @case ('arrow-left') {
          <path d="M19 12H5" />
          <path d="m12 19-7-7 7-7" />
        }
        @case ('check') {
          <path d="M20 6 9 17l-5-5" />
        }
        @case ('x') {
          <path d="M18 6 6 18" />
          <path d="m6 6 12 12" />
        }
        @case ('backspace') {
          <path d="M22 5H9l-7 7 7 7h13a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2Z" />
          <path d="m14 10 4 4" />
          <path d="m18 10-4 4" />
        }
        @case ('chevron-right') {
          <path d="m9 18 6-6-6-6" />
        }
      }
    </svg>
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly size = input(20);
}
