import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TransactionsApiService } from '../../core/api/transactions-api.service';
import { WalletApiService } from '../../core/api/wallet-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { TransactionItem } from '../../core/models/activity.models';
import { WalletBalance } from '../../core/models/wallet.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, MoneyPipe, DatePipe],
  templateUrl: './home-page.html',
})
export class HomePage implements OnInit {
  readonly auth = inject(AuthService);
  private readonly wallets = inject(WalletApiService);
  private readonly transactions = inject(TransactionsApiService);
  private readonly toast = inject(ToastService);

  readonly balance = signal<WalletBalance | null>(null);
  readonly recent = signal<TransactionItem[]>([]);
  readonly loading = signal(true);

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

    this.transactions.list({ page: 1, pageSize: 5 }).subscribe({
      next: (page) => this.recent.set(page.items),
      error: () => undefined,
    });
  }
}
