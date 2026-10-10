import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CircleService } from '../../services/circle.service';
import { CatalogService } from '../../services/catalog.service';
import { Category } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-create-circle',
  imports: [ReactiveFormsModule],
  templateUrl: './create-circle.html',
  styleUrl: './create-circle.css'
})
export class CreateCircle {
  private fb = inject(FormBuilder);
  private circleService = inject(CircleService);
  private catalogService = inject(CatalogService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  categories = signal<Category[]>([]);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    contribution: [null as number | null, [Validators.required, Validators.min(1)]],
    meetingLabel: ['Daily', Validators.required],
    categoryId: [null as number | null]
  });

  ngOnInit() {
    this.catalogService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats),
      error: () => {}
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.circleService.createCircle(this.form.value as any).subscribe({
      next: (circle) => this.router.navigate(['/circles', circle.id]),
      error: (err: any) => { this.loading.set(false); this.error.set(err.error?.title || 'Failed to create'); }
    });
  }

  cancel() { this.router.navigate(['/home']); }
}
