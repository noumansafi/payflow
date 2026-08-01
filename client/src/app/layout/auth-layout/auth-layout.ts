import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  template: `
    <div class="auth-shell">
      <div class="auth-panel">
        <div class="mb-1 flex flex-col items-center gap-3 text-center">
          <span
            class="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary text-lg font-bold text-white shadow-sm shadow-primary/25"
            aria-hidden="true"
          >
            P
          </span>
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
