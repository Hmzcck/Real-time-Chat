import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthenticationService } from '../../services/authentication.service';
import { CommonModule } from '@angular/common';

// Angular Material imports
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading: boolean = false;
  hide: boolean = true;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authenticationService: AuthenticationService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      userNameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }
    this.isLoading = true;
    this.authenticationService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.router.navigate(['/chats']);
      },
      error: (error) => {
        this.isLoading = false;
        this.snackBar.open(
          error.error?.detail ?? 'Login failed. Please try again.',
          'Close',
          {
            duration: 3000,
          }
        );
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }
}
