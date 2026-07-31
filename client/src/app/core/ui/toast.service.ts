import { Injectable, signal } from '@angular/core';

export type ToastTone = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  message: string;
  tone: ToastTone;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly toastsSignal = signal<Toast[]>([]);

  readonly toasts = this.toastsSignal.asReadonly();

  success(message: string): void {
    this.push(message, 'success');
  }

  error(message: string): void {
    this.push(message, 'error');
  }

  info(message: string): void {
    this.push(message, 'info');
  }

  dismiss(id: number): void {
    this.toastsSignal.update((items) => items.filter((t) => t.id !== id));
  }

  private push(message: string, tone: ToastTone): void {
    const id = this.nextId++;
    this.toastsSignal.update((items) => [...items, { id, message, tone }]);
    window.setTimeout(() => this.dismiss(id), 4200);
  }
}
