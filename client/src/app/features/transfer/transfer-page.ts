import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BeneficiariesApiService } from '../../core/api/beneficiaries-api.service';
import { TransfersApiService } from '../../core/api/transfers-api.service';
import { WalletApiService } from '../../core/api/wallet-api.service';
import { problemMessage } from '../../core/http/problem-details';
import {
  Beneficiary,
  BeneficiaryCandidate,
  TransferResult,
} from '../../core/models/transfer.models';
import { ToastService } from '../../core/ui/toast.service';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

type Step = 'who' | 'amount' | 'confirm' | 'done';

@Component({
  selector: 'app-transfer-page',
  imports: [ReactiveFormsModule, RouterLink, MoneyPipe],
  templateUrl: './transfer-page.html',
})
export class TransferPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly beneficiariesApi = inject(BeneficiariesApiService);
  private readonly transfersApi = inject(TransfersApiService);
  private readonly walletsApi = inject(WalletApiService);
  private readonly toast = inject(ToastService);

  readonly step = signal<Step>('who');
  readonly busy = signal(false);
  readonly walletFrozen = signal(false);
  readonly currency = signal('USD');
  readonly beneficiaries = signal<Beneficiary[]>([]);
  readonly candidate = signal<BeneficiaryCandidate | null>(null);
  readonly result = signal<TransferResult | null>(null);

  readonly lookupForm = this.fb.nonNullable.group({
    userId: ['', [Validators.required, Validators.minLength(32)]],
  });

  readonly amountForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    note: [''],
  });

  readonly recipientName = computed(() => {
    const c = this.candidate();
    return c ? `${c.firstName} ${c.lastName}` : '';
  });

  ngOnInit(): void {
    this.walletsApi.getBalance().subscribe({
      next: (balance) => {
        this.currency.set(balance.currency);
        this.walletFrozen.set(balance.status === 'Frozen');
      },
    });

    this.beneficiariesApi.list().subscribe({
      next: (page) => this.beneficiaries.set(page.items),
      error: () => undefined,
    });
  }

  pickBeneficiary(item: Beneficiary): void {
    this.lookupForm.controls.userId.setValue(item.beneficiaryUserId);
    this.lookup();
  }

  lookup(): void {
    if (this.lookupForm.invalid) {
      this.lookupForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.beneficiariesApi.lookup(this.lookupForm.controls.userId.value.trim()).subscribe({
      next: (candidate) => {
        this.candidate.set(candidate);
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.candidate.set(null);
        this.toast.error(problemMessage(error, 'Recipient not found.'));
        this.busy.set(false);
      },
    });
  }

  confirmRecipient(): void {
    if (!this.candidate()) {
      return;
    }
    this.step.set('amount');
  }

  continueToConfirm(): void {
    if (this.amountForm.invalid) {
      this.amountForm.markAllAsTouched();
      return;
    }
    this.step.set('confirm');
  }

  backToWho(): void {
    this.step.set('who');
  }

  backToAmount(): void {
    this.step.set('amount');
  }

  send(): void {
    const recipient = this.candidate();
    if (!recipient || this.amountForm.invalid || this.busy()) {
      return;
    }

    this.busy.set(true);
    const { amount, note } = this.amountForm.getRawValue();
    this.transfersApi.send(recipient.userId, amount, note || undefined).subscribe({
      next: (result) => {
        this.result.set(result);
        this.step.set('done');
        this.toast.success('Transfer sent');
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Transfer failed.'));
        this.busy.set(false);
      },
    });
  }

  saveRecipient(): void {
    const recipient = this.candidate();
    if (!recipient || recipient.alreadySaved || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.beneficiariesApi.add(recipient.userId).subscribe({
      next: () => {
        this.candidate.update((c) => (c ? { ...c, alreadySaved: true } : c));
        this.toast.success('Saved to People');
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not save recipient.'));
        this.busy.set(false);
      },
    });
  }
}
