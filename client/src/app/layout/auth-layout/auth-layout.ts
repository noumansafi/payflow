import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  template: `
    <div class="auth-shell">
      <div class="auth-panel">
        <div class="stack-sm text-center">
          <p class="brand-mark">Pay<span>Flow</span></p>
          <p class="page-subtitle">Simple wallet. Clear transfers.</p>
        </div>
        <router-outlet />
      </div>
    </div>
  `,
})
export class AuthLayout {}
