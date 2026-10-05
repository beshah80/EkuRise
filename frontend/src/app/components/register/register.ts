import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');

  form = this.fb.group({
    phoneNumber: ['', [Validators.required, Validators.minLength(10)]],
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    gender: [0, Validators.required],
    jobType: ['', Validators.required],
    location: ['', Validators.required],
    referralCode: ['']
  });

  jobs = ['Driver', 'Merchant', 'Teacher', 'Engineer', 'Accountant', 'Business Owner', 'Office Worker', 'Other'];
  genders = ['Male', 'Female', 'Other'];

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.register(this.form.value as any).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.router.navigate(['/register/verify'], {
          state: {
            phoneNumber: this.form.value.phoneNumber,
            demoCode: res.demoCode
          }
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Registration failed');
      }
    });
  }
}
