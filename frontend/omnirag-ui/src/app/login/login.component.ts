import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService, LoginCredentials } from '../core/auth.service';

@Component({
  selector: 'login-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastr = inject(ToastrService);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
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
      this.toastr.success(`Signed in as ${email}`, 'Welcome back');
      await this.router.navigate(['/dashboard']);
    } catch (err) {
      console.error('Login failed', err);
      this.errorMessage = 'Login failed';
      this.toastr.error('Unable to sign in. Please check your credentials.', 'Login failed');
    } finally {
      this.isSubmitting = false;
    }
  }
}

