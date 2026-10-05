import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { TokenService } from '../../services/token.service';
import { ToastService } from '../../services/toast.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  private toast = inject(ToastService);
  loading = signal(false);
  error = signal('');
  showPin = signal(false);
  otpSent = signal(false);

  form = this.fb.group({
    phoneNumber: ['', [Validators.required, Validators.minLength(10)]],
    pin: [''],
    otpCode: ['']
  });

  submitOtp() {
    if (this.form.controls.phoneNumber.invalid) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.sendLoginOtp({ phoneNumber: this.form.value.phoneNumber! }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.otpSent.set(true);
        this.error.set(res.demoCode ? `Demo code: ${res.demoCode}` : '');
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Failed to send code');
      }
    });
  }

  verifyOtp() {
    if (!this.form.value.otpCode) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.verifyOtp({
      phoneNumber: this.form.value.phoneNumber!,
      code: this.form.value.otpCode!
    }).subscribe({
      next: (res) => {
        this.tokenService.setToken(res.token);
        this.toast.success('Login successful! Welcome back 👋');
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.title || 'Invalid code');
      }
    });
  }

  togglePin() {
    this.showPin.set(!this.showPin());
    this.otpSent.set(false);
    this.error.set('');
  }

  submitPin() {
    if (this.form.controls.phoneNumber.invalid || this.form.controls.pin.invalid) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.pinLogin({
      phoneNumber: this.form.value.phoneNumber!,
      pin: this.form.value.pin!
    }).subscribe({
      next: (res) => {
        this.tokenService.setToken(res.token);
        this.toast.success('Login successful! Welcome back 👋');
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err.error?.title || 'Incorrect credentials');
      }
    });
  }
}
