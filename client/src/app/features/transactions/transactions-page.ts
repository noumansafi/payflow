import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TransactionsApiService } from '../../core/api/transactions-api.service';
import { problemMessage } from '../../core/http/problem-details';
import { TransactionItem } from '../../core/models/activity.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-transactions-page',
  imports: [FormsModule, RouterLink, MoneyPipe, DatePipe],
  templateUrl: './transactions-page.html',
})
export class TransactionsPage implements OnInit {
  private readonly api = inject(TransactionsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<TransactionItem[]>([]);
  readonly loading = signal(true);
  readonly selected = signal<TransactionItem | null>(null);
  direction = '';

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.api
      .list({
        direction: this.direction || undefined,
      })
      .subscribe({
        next: (page) => {
          this.items.set(page.items);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.toast.error(problemMessage(error, 'Could not load activity.'));
          this.loading.set(false);
        },
      });
  }

  open(item: TransactionItem): void {
    this.selected.set(item);
  }

  closeDetail(): void {
    this.selected.set(null);
  }
}
