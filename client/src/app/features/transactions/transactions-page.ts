import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TransactionsApiService } from '../../core/api/transactions-api.service';
import { problemMessage } from '../../core/http/problem-details';
import { TransactionItem } from '../../core/models/activity.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';
import { Icon } from '../../shared/ui/icon';
import { StatusBadge } from '../../shared/ui/status-badge';
import {
  counterpartyLabel,
  formatTxTime,
  groupTransactionsByDate,
} from '../../shared/utils/transaction-groups';

type FilterTab = 'all' | 'sent' | 'received' | 'pending';

@Component({
  selector: 'app-transactions-page',
  imports: [RouterLink, MoneyPipe, DatePipe, Icon, StatusBadge],
  templateUrl: './transactions-page.html',
})
export class TransactionsPage implements OnInit {
  private readonly api = inject(TransactionsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<TransactionItem[]>([]);
  readonly loading = signal(true);
  readonly selected = signal<TransactionItem | null>(null);
  readonly filter = signal<FilterTab>('all');

  readonly tabs: { id: FilterTab; label: string }[] = [
    { id: 'all', label: 'All' },
    { id: 'sent', label: 'Sent' },
    { id: 'received', label: 'Received' },
    { id: 'pending', label: 'Pending' },
  ];

  readonly groups = computed(() => groupTransactionsByDate(this.items()));

  ngOnInit(): void {
    this.reload();
  }

  setFilter(tab: FilterTab): void {
    if (this.filter() === tab) {
      return;
    }
    this.filter.set(tab);
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    const tab = this.filter();
    this.api
      .list({
        direction: tab === 'sent' ? 'Sent' : tab === 'received' ? 'Received' : undefined,
        status: tab === 'pending' ? 'Pending' : undefined,
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

  txLabel(tx: TransactionItem): string {
    return counterpartyLabel(tx);
  }

  txTime(tx: TransactionItem): string {
    return formatTxTime(tx.createdAtUtc);
  }
}
