import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BeneficiariesApiService } from '../../core/api/beneficiaries-api.service';
import { NotificationsApiService } from '../../core/api/notifications-api.service';
import { TransactionsApiService } from '../../core/api/transactions-api.service';
import { WalletApiService } from '../../core/api/wallet-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { TransactionItem } from '../../core/models/activity.models';
import { Beneficiary } from '../../core/models/transfer.models';
import { WalletBalance } from '../../core/models/wallet.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';
import { Avatar } from '../../shared/ui/avatar';
import { Icon } from '../../shared/ui/icon';
import { StatusBadge } from '../../shared/ui/status-badge';
import {
  counterpartyLabel,
  formatTxTime,
  groupTransactionsByDate,
} from '../../shared/utils/transaction-groups';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, MoneyPipe, Icon, Avatar, StatusBadge],
  templateUrl: './home-page.html',
})
export class HomePage implements OnInit {
  readonly auth = inject(AuthService);
  private readonly wallets = inject(WalletApiService);
  private readonly transactions = inject(TransactionsApiService);
  private readonly beneficiariesApi = inject(BeneficiariesApiService);
  private readonly notificationsApi = inject(NotificationsApiService);
  private readonly toast = inject(ToastService);

  readonly balance = signal<WalletBalance | null>(null);
  readonly recent = signal<TransactionItem[]>([]);
  readonly favorites = signal<Beneficiary[]>([]);
  readonly unreadCount = signal(0);
  readonly loading = signal(true);
  readonly recentLoading = signal(true);
  readonly hideBalance = signal(false);

  readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 12) {
      return 'Good morning';
    }
    if (hour < 17) {
      return 'Good afternoon';
    }
    return 'Good evening';
  });

  readonly displayName = computed(() => {
    const user = this.auth.user();
    return user ? `${user.firstName} ${user.lastName}` : 'PayFlow';
  });

  readonly firstName = computed(() => this.auth.user()?.firstName ?? 'there');

  readonly groups = computed(() => groupTransactionsByDate(this.recent()));

  readonly maskedBalance = '••••••';

  ngOnInit(): void {
    this.wallets.getBalance().subscribe({
      next: (balance) => {
        this.balance.set(balance);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not load balance.'));
        this.loading.set(false);
      },
    });

    this.transactions.list({ page: 1, pageSize: 8 }).subscribe({
      next: (page) => {
        this.recent.set(page.items);
        this.recentLoading.set(false);
      },
      error: () => this.recentLoading.set(false),
    });

    this.beneficiariesApi.list(1, 12).subscribe({
      next: (page) => this.favorites.set(page.items),
      error: () => undefined,
    });

    this.notificationsApi.list(1, 1, false).subscribe({
      next: (page) => this.unreadCount.set(page.totalCount),
      error: () => undefined,
    });
  }

  toggleHideBalance(): void {
    this.hideBalance.update((v) => !v);
  }

  personName(person: Beneficiary): string {
    return person.displayName || `${person.firstName} ${person.lastName}`;
  }

  txLabel(tx: TransactionItem): string {
    return counterpartyLabel(tx);
  }

  txTime(tx: TransactionItem): string {
    return formatTxTime(tx.createdAtUtc);
  }
}
