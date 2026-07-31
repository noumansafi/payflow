import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-verify-email-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-email-page.html',
})
export class VerifyEmailPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    token: [this.route.snapshot.queryParamMap.get('token') ?? '', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.verifyEmail(this.form.controls.token.value).subscribe({
      next: () => {
        this.toast.success('Email verified — sign in to continue.');
        void this.router.navigateByUrl('/login');
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Verification failed.'));
        this.submitting.set(false);
      },
    });
  }
}
