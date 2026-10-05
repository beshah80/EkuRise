import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-account-security',
  imports: [ReactiveFormsModule],
  templateUrl: './account-security.html',
  styleUrl: './account-security.css'
})
export class AccountSecurity {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  success = signal('');
  showPinForm = signal(false);
  showDeleteForm = signal(false);
  deleteSent = signal(false);
  deleteCode = signal('');

  pinForm = this.fb.group({
    currentCredential: ['', Validators.required],
    pin: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(6)]]
  });

  deleteForm = this.fb.group({
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });

  setPin() {
    if (this.pinForm.invalid) return;
    this.loading.set(true);
    this.auth.setPin(this.pinForm.value as any).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set('PIN updated successfully!');
        this.showPinForm.set(false);
        this.pinForm.reset();
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Failed to update PIN');
      }
    });
  }

  requestDelete() {
    this.loading.set(true);
    this.auth.getProfile().subscribe({
      next: (p) => {
        this.auth.requestDeleteAccount({ phoneNumber: p.phoneNumber }).subscribe({
          next: (res) => {
            this.loading.set(false);
            this.deleteSent.set(true);
            this.deleteCode.set(res.demoCode || '');
          },
          error: (err: any) => {
            this.loading.set(false);
            this.error.set(err.error?.title || 'Failed to send code');
          }
        });
      },
      error: () => { this.loading.set(false); }
    });
  }

  confirmDelete() {
    if (this.deleteForm.invalid) return;
    this.loading.set(true);
    this.auth.confirmDeleteAccount({ code: this.deleteForm.value.code! }).subscribe({
      next: () => {
        this.loading.set(false);
        localStorage.clear();
        this.router.navigate(['/login']);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Invalid code');
      }
    });
  }

  goBack() { this.router.navigate(['/account']); }
}
