import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FeedbackService } from '../../services/feedback.service';
import { SuccessStoryDto } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-success-stories',
  imports: [ReactiveFormsModule],
  templateUrl: './success-stories.html',
  styleUrl: './success-stories.css'
})
export class SuccessStories {
  private fb = inject(FormBuilder);
  private feedbackService = inject(FeedbackService);
  private router = inject(Router);

  stories = signal<SuccessStoryDto[]>([]);
  loading = signal(true);
  showForm = signal(false);
  success = signal('');
  error = signal('');
  rating = signal(5);

  form = this.fb.group({
    content: ['', [Validators.required, Validators.minLength(20)]],
    authorName: ['']
  });

  ngOnInit() {
    this.feedbackService.getSuccessStories().subscribe({
      next: (s) => { this.stories.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  setRating(r: number) { this.rating.set(r); }

  submit() {
    if (this.form.invalid) return;
    this.feedbackService.submitSuccessStory({
      content: this.form.value.content!,
      rating: this.rating(),
      authorName: this.form.value.authorName || undefined
    }).subscribe({
      next: () => {
        this.success.set('Story submitted! It will appear after admin approval.');
        this.showForm.set(false);
        this.form.reset();
        this.rating.set(5);
      },
      error: (err: any) => this.error.set(err.error?.title || 'Failed to submit')
    });
  }

  stars(n: number) { return Array(n).fill('⭐'); }
  formatDate(d: string) { return new Date(d).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }); }
  goBack() { this.router.navigate(['/account']); }
}
