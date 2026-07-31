import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-register-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.html',
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly submitting = signal(false);
  readonly verifyToken = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.verifyToken.set(response.emailVerificationToken);
        this.toast.success('Account created — verify email, then sign in.');
        this.submitting.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not create your account.'));
        this.submitting.set(false);
      },
    });
  }

  goVerify(): void {
    const token = this.verifyToken();
    if (token) {
      void this.router.navigate(['/verify-email'], { queryParams: { token } });
    }
  }
}
