import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-edit-profile',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css'
})
export class EditProfile {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  success = signal('');

  form = this.fb.group({
    firstName: [''],
    lastName: [''],
    gender: [0],
    jobType: [''],
    location: [''],
    email: ['']
  });

  jobs = ['Driver', 'Merchant', 'Teacher', 'Engineer', 'Accountant', 'Business Owner', 'Office Worker', 'Other'];
  genders = ['Male', 'Female'];

  ngOnInit() {
    this.auth.getProfile().subscribe({
      next: (p) => this.form.patchValue({
        firstName: p.firstName, lastName: p.lastName, gender: p.gender,
        jobType: p.jobType, location: p.location, email: p.email || ''
      }),
      error: () => {}
    });
  }

  submit() {
    this.loading.set(true);
    this.auth.updateProfile(this.form.value as any).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set('Profile updated!');
        setTimeout(() => this.router.navigate(['/account']), 1000);
      },
      error: (err: any) => { this.loading.set(false); this.error.set(err.error?.title || 'Failed'); }
    });
  }

  cancel() { this.router.navigate(['/account']); }
}
