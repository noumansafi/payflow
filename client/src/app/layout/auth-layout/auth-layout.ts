import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  template: `
    <div class="auth-shell">
      <div class="auth-panel">
        <div class="mb-1 flex flex-col items-center gap-3 text-center">
          <img
            src="assets/pf-logo-128.png"
            alt="PayFlow"
            width="56"
            height="56"
            class="h-14 w-14 rounded-2xl shadow-sm shadow-primary/20"
          />
          <div class="stack-sm">
            <p class="brand-mark text-2xl">Pay<span>Flow</span></p>
            <p class="page-subtitle">Send money clearly. Keep every transfer accountable.</p>
          </div>
        </div>
        <router-outlet />
      </div>
    </div>
  `,
})
export class AuthLayout {}
