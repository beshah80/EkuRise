import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { TokenService } from '../../services/token.service';

@Component({
  standalone: true,
  selector: 'app-verify',
  imports: [ReactiveFormsModule],
  templateUrl: './verify.html',
  styleUrl: './verify.css'
})
export class Verify {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  phoneNumber = '';
  demoCode = '';

  form = this.fb.group({
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  constructor() {
    const state = history.state as { phoneNumber?: string; demoCode?: string };
    this.phoneNumber = state.phoneNumber || '';
    this.demoCode = state.demoCode || '';
    if (!this.phoneNumber) this.router.navigate(['/register']);
  }

  verify() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.verifyRegistration({
      phoneNumber: this.phoneNumber,
      code: this.form.value.code!
    }).subscribe({
      next: (res) => {
        this.tokenService.setToken(res.token);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Invalid code');
      }
    });
  }
}
