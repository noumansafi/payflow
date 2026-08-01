import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
import { Avatar } from '../../shared/ui/avatar';
import { Icon } from '../../shared/ui/icon';

type Step = 'who' | 'amount' | 'confirm' | 'done';

@Component({
  selector: 'app-transfer-page',
  imports: [ReactiveFormsModule, RouterLink, MoneyPipe, Icon, Avatar],
  templateUrl: './transfer-page.html',
})
export class TransferPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
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
  readonly searchQuery = signal('');
  readonly amountDigits = signal('0');

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

  readonly filteredBeneficiaries = computed(() => {
    const q = this.searchQuery().trim().toLowerCase();
    const list = this.beneficiaries();
    if (!q) {
      return list;
    }
    return list.filter((p) => {
      const name = (p.displayName || `${p.firstName} ${p.lastName}`).toLowerCase();
      return (
        name.includes(q) ||
        p.email.toLowerCase().includes(q) ||
        p.beneficiaryUserId.toLowerCase().includes(q)
      );
    });
  });

  readonly displayAmount = computed(() => {
    const raw = this.amountDigits();
    const n = Number(raw);
    if (!raw || Number.isNaN(n)) {
      return '0.00';
    }
    const [whole, frac = ''] = raw.split('.');
    const fracPadded = (frac + '00').slice(0, 2);
    return `${Number(whole || '0').toLocaleString()}.${fracPadded}`;
  });

  readonly fee = 0;

  readonly keypadKeys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '0', 'back'] as const;

  readonly stepRank = computed(() => {
    switch (this.step()) {
      case 'who':
        return 1;
      case 'amount':
        return 2;
      case 'confirm':
        return 3;
      default:
        return 0;
    }
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

    const to = this.route.snapshot.queryParamMap.get('to');
    if (to) {
      this.lookupForm.controls.userId.setValue(to);
      this.lookup();
    }
  }

  onSearch(value: string): void {
    this.searchQuery.set(value);
    this.lookupForm.controls.userId.setValue(value.trim());
  }

  personName(person: Beneficiary): string {
    return person.displayName || `${person.firstName} ${person.lastName}`;
  }

  pickBeneficiary(item: Beneficiary): void {
    this.lookupForm.controls.userId.setValue(item.beneficiaryUserId);
    this.searchQuery.set(item.email);
    this.lookup();
  }

  lookup(): void {
    if (this.lookupForm.invalid) {
      this.lookupForm.markAllAsTouched();
      if (this.searchQuery().trim() && this.lookupForm.controls.userId.invalid) {
        this.toast.error('Enter a valid PayFlow user id to look up.');
      }
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
    this.amountDigits.set('0');
    this.amountForm.controls.amount.setValue(0);
    this.step.set('amount');
  }

  pressKey(key: (typeof this.keypadKeys)[number]): void {
    if (key === 'back') {
      this.backspace();
      return;
    }

    let next = this.amountDigits();
    if (next === '0' && key !== '.') {
      next = key;
    } else if (key === '.') {
      if (next.includes('.')) {
        return;
      }
      next = `${next}.`;
    } else {
      const [, frac] = next.split('.');
      if (frac !== undefined && frac.length >= 2) {
        return;
      }
      if (!next.includes('.') && next.replace(/^0+/, '').length >= 7) {
        return;
      }
      next = `${next}${key}`;
    }

    this.amountDigits.set(next);
    this.syncAmountForm();
  }

  backspace(): void {
    const current = this.amountDigits();
    if (current.length <= 1) {
      this.amountDigits.set('0');
    } else {
      this.amountDigits.set(current.slice(0, -1));
    }
    this.syncAmountForm();
  }

  private syncAmountForm(): void {
    const value = Number(this.amountDigits());
    this.amountForm.controls.amount.setValue(Number.isFinite(value) ? value : 0);
  }

  continueToConfirm(): void {
    this.syncAmountForm();
    if (this.amountForm.invalid || this.amountForm.controls.amount.value < 0.01) {
      this.toast.error('Enter an amount of at least 0.01.');
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

  resetFlow(): void {
    this.step.set('who');
    this.candidate.set(null);
    this.result.set(null);
    this.amountDigits.set('0');
    this.amountForm.reset({ amount: 0, note: '' });
    this.lookupForm.reset({ userId: '' });
    this.searchQuery.set('');
  }
}
