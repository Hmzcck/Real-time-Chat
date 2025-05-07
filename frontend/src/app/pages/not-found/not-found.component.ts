import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <div class="not-found-container">
      <h1>404 - Page Not Found</h1>
      <p>The page you are looking for does not exist.</p>
      <button mat-raised-button color="primary" routerLink="/">
        Return to Home
      </button>
    </div>
  `,
  styles: [`
    .not-found-container {
      text-align: center;
      padding: 2rem;
      margin-top: 2rem;
    }
    h1 {
      margin-bottom: 1rem;
    }
    p {
      margin-bottom: 2rem;
    }
  `]
})
export class NotFoundComponent {}
