import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FeedbackService } from '../../services/feedback.service';
import { FeedbackDto } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-account-feedback',
  imports: [ReactiveFormsModule],
  templateUrl: './account-feedback.html',
  styleUrl: './account-feedback.css'
})
export class AccountFeedback {
  private fb = inject(FormBuilder);
  private feedbackService = inject(FeedbackService);
  private router = inject(Router);

  feedbacks = signal<FeedbackDto[]>([]);
  loading = signal(false);
  success = signal('');
  error = signal('');

  form = this.fb.group({
    subject: ['', Validators.required],
    message: ['', [Validators.required, Validators.minLength(10)]]
  });

  ngOnInit() {
    this.feedbackService.getMyFeedbacks().subscribe({
      next: (f) => this.feedbacks.set(f),
      error: () => {}
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.feedbackService.submitFeedback(this.form.value as any).subscribe({
      next: (fb) => {
        this.loading.set(false);
        this.success.set('Message sent! We will get back to you soon.');
        this.form.reset();
        this.feedbacks.update(list => [fb, ...list]);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Failed to send');
      }
    });
  }

  formatDate(d: string) { return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }); }
  goBack() { this.router.navigate(['/account']); }
}
