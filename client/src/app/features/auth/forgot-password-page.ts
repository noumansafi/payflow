import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-forgot-password-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password-page.html',
})
export class ForgotPasswordPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly submitting = signal(false);
  readonly doneMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.forgotPassword(this.form.controls.email.value).subscribe({
      next: (result) => {
        this.doneMessage.set(
          result.message ??
            'If an account exists for that email, a password reset token has been issued.',
        );
        this.toast.info('Check API logs for the reset token in Development.');
        this.submitting.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error));
        this.submitting.set(false);
      },
    });
  }
}
