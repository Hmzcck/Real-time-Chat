import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthenticationService } from '../../services/authentication.service';
import { Router, RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
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
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading: boolean = false;
  hide: boolean = true;
  constructor(
    private readonly fb: FormBuilder,
    private readonly authenticationService: AuthenticationService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {
    this.registerForm = this.fb.group({
      userName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.authenticationService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('Registration successful!', 'Close', {
          duration: 3000,
        });
        this.router.navigate(['/login']);
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
