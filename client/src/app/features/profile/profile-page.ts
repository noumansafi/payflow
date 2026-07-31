import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment';
import { problemMessage } from '../../core/http/problem-details';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-profile-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './profile-page.html',
})
export class ProfilePage {
  readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly busy = signal(false);

  readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.http
      .post<void>(`${environment.apiBaseUrl}/auth/password/change`, this.passwordForm.getRawValue())
      .subscribe({
        next: () => {
          this.toast.success('Password updated');
          this.passwordForm.reset();
          this.busy.set(false);
        },
        error: (error: unknown) => {
          this.toast.error(problemMessage(error, 'Could not change password.'));
          this.busy.set(false);
        },
      });
  }

  logout(): void {
    this.busy.set(true);
    this.auth.logout().subscribe({
      error: () => this.busy.set(false),
    });
  }
}
