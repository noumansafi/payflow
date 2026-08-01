import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BeneficiariesApiService } from '../../core/api/beneficiaries-api.service';
import { problemMessage } from '../../core/http/problem-details';
import { Beneficiary, BeneficiaryCandidate } from '../../core/models/transfer.models';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-beneficiaries-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './beneficiaries-page.html',
})
export class BeneficiariesPage implements OnInit {
  private readonly api = inject(BeneficiariesApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly items = signal<Beneficiary[]>([]);
  readonly candidate = signal<BeneficiaryCandidate | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);

  readonly lookupForm = this.fb.nonNullable.group({
    userId: ['', [Validators.required, Validators.minLength(32)]],
  });

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (page) => {
        this.items.set(page.items);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not load people.'));
        this.loading.set(false);
      },
    });
  }

  lookup(): void {
    if (this.lookupForm.invalid) {
      this.lookupForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.api.lookup(this.lookupForm.controls.userId.value.trim()).subscribe({
      next: (candidate) => {
        this.candidate.set(candidate);
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.candidate.set(null);
        this.toast.error(problemMessage(error, 'User not found.'));
        this.busy.set(false);
      },
    });
  }

  save(): void {
    const c = this.candidate();
    if (!c || c.alreadySaved || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.api.add(c.userId).subscribe({
      next: () => {
        this.toast.success('Saved');
        this.candidate.set(null);
        this.lookupForm.reset({ userId: '' });
        this.reload();
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not save.'));
        this.busy.set(false);
      },
    });
  }

  remove(item: Beneficiary): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.api.remove(item.id).subscribe({
      next: () => {
        this.toast.success('Removed');
        this.reload();
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not remove.'));
        this.busy.set(false);
      },
    });
  }
}
