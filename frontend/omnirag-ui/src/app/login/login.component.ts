import { NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCard, MatCardActions, MatCardContent } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
import { AuthService, LoginCredentials } from '../core/auth.service';

@Component({
  selector: 'login-page',
  standalone: true,
  imports: [
    NgOptimizedImage,
    MatCard,
    MatCardActions,
    MatCardContent,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatIconModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  isSubmitting = false;
  errorMessage = '';

  async onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();
    const credentials: LoginCredentials = {
      username: email,
      password,
    };
    this.isSubmitting = true;
    this.errorMessage = '';

    try {
      await this.auth.login(credentials);
      await this.router.navigate(['/dashboard']);
    } catch (err) {
      console.error('Login failed', err);
      this.errorMessage = 'Login failed';
    } finally {
      this.isSubmitting = false;
    }
  }
}

