import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

@Component({
  selector: 'app-placeholder-page',
  template: `
    <section class="page stack">
      <h1 class="page-title">{{ title() }}</h1>
      <p class="page-subtitle">{{ description() }}</p>
      <div class="empty-state">This screen will be wired to the API in the next slices.</div>
    </section>
  `,
})
export class PlaceholderPage {
  private readonly route = inject(ActivatedRoute);

  readonly title = toSignal(
    this.route.data.pipe(map((data) => (data['title'] as string) ?? 'PayFlow')),
    { initialValue: 'PayFlow' },
  );

  readonly description = toSignal(
    this.route.data.pipe(map((data) => (data['description'] as string) ?? 'Coming soon.')),
    { initialValue: 'Coming soon.' },
  );
}
