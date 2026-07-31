import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.html',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: () => {
        this.toast.success('Welcome back');
        void this.router.navigateByUrl('/');
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Invalid email or password.'));
        this.submitting.set(false);
      },
      complete: () => this.submitting.set(false),
    });
  }
}
