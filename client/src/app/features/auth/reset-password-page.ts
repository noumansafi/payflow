import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-reset-password-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password-page.html',
})
export class ResetPasswordPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    token: [this.route.snapshot.queryParamMap.get('token') ?? '', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const { token, newPassword } = this.form.getRawValue();
    this.auth.resetPassword(token, newPassword).subscribe({
      next: () => {
        this.toast.success('Password updated — you can sign in now.');
        void this.router.navigateByUrl('/login');
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Reset failed. Token may be invalid or expired.'));
        this.submitting.set(false);
      },
    });
  }
}
