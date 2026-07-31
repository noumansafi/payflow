import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { WalletApiService } from '../../core/api/wallet-api.service';
import { environment } from '../../../environments/environment';
import { problemMessage } from '../../core/http/problem-details';
import { Wallet } from '../../core/models/wallet.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-wallet-page',
  imports: [ReactiveFormsModule, RouterLink, MoneyPipe],
  templateUrl: './wallet-page.html',
})
export class WalletPage implements OnInit {
  private readonly api = inject(WalletApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly wallet = signal<Wallet | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly isDev = !environment.production;

  readonly creditForm = this.fb.nonNullable.group({
    amount: [100, [Validators.required, Validators.min(0.01)]],
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.api.getMine().subscribe({
      next: (wallet) => {
        this.wallet.set(wallet);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not load wallet.'));
        this.loading.set(false);
      },
    });
  }

  toggleFreeze(): void {
    const current = this.wallet();
    if (!current || this.busy()) {
      return;
    }

    const next = current.status === 'Frozen' ? 'Active' : 'Frozen';
    this.busy.set(true);
    this.api.changeStatus(next).subscribe({
      next: (wallet) => {
        this.wallet.set(wallet);
        this.toast.success(next === 'Frozen' ? 'Wallet frozen' : 'Wallet activated');
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not update wallet status.'));
        this.busy.set(false);
      },
    });
  }

  credit(): void {
    if (!this.isDev || this.creditForm.invalid || this.busy()) {
      this.creditForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.api.credit(this.creditForm.controls.amount.value).subscribe({
      next: (wallet) => {
        this.wallet.set(wallet);
        this.toast.success('Demo funds added');
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Credit failed (Development only).'));
        this.busy.set(false);
      },
    });
  }
}
