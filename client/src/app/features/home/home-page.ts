import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  templateUrl: './home-page.html',
})
export class HomePage {
  readonly auth = inject(AuthService);
}
