import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-host',
  template: `
    <div class="pointer-events-none fixed inset-x-0 top-3 z-50 flex flex-col items-center gap-2 px-4">
      @for (toast of toasts(); track toast.id) {
        <div
          class="pointer-events-auto w-full max-w-md rounded-xl px-4 py-3 text-sm font-medium shadow-sm"
          [class]="toneClass(toast.tone)"
          role="status"
        >
          {{ toast.message }}
        </div>
      }
    </div>
  `,
})
export class ToastHost {
  private readonly toastService = inject(ToastService);
  readonly toasts = this.toastService.toasts;

  toneClass(tone: 'success' | 'error' | 'info'): string {
    switch (tone) {
      case 'success':
        return 'bg-primary text-white';
      case 'error':
        return 'bg-danger text-white';
      default:
        return 'bg-ink text-white';
    }
  }
}
