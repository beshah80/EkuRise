import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { CatalogService } from '../../services/catalog.service';
import { CircleService } from '../../services/circle.service';
import { Category, SubCategory, CircleSummary, PublicCircleSummary, SubmitJoinRequest } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
  private catalog = inject(CatalogService);
  private circleService = inject(CircleService);
  private router = inject(Router);

  categories = signal<Category[]>([]);
  subCategories = signal<SubCategory[]>([]);
  circles = signal<CircleSummary[]>([]);
  publicCircles = signal<PublicCircleSummary[]>([]);
  selectedCategory = signal<number | null>(null);
  loading = signal(true);
  error = signal('');

  // Join modal state
  selectedJoinCircle = signal<PublicCircleSummary | null>(null);
  joinAgreed = signal(false);
  joinMessage = signal('');
  joinLoading = signal(false);
  joinError = signal('');

  ngOnInit() {
    this.loadCategories();
    this.loadCircles();
    this.loadPublicCircles();
  }

  loadCategories() {
    this.catalog.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        if (cats.length > 0) this.selectCategory(cats[0].id);
      },
      error: () => this.error.set('Failed to load categories')
    });
  }

  loadCircles() {
    this.circleService.getMyCircles().subscribe({
      next: (c) => { this.circles.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  loadPublicCircles() {
    this.circleService.getPublicCircles().subscribe({
      next: (circles) => this.publicCircles.set(circles),
      error: () => {}
    });
  }

  selectCategory(id: number) {
    this.selectedCategory.set(id);
    this.catalog.getSubCategories(id).subscribe({
      next: (subs) => this.subCategories.set(subs),
      error: () => this.error.set('Failed to load Ekubs')
    });
  }

  viewSubCategory(id: number) {
    this.router.navigate(['/catalog', id]);
  }

  viewCircle(id: number) {
    this.router.navigate(['/circles', id]);
  }

  openJoinModal(circle: PublicCircleSummary) {
    this.selectedJoinCircle.set(circle);
    this.joinAgreed.set(false);
    this.joinMessage.set('');
    this.joinError.set('');
  }

  closeJoinModal() {
    this.selectedJoinCircle.set(null);
    this.joinAgreed.set(false);
    this.joinMessage.set('');
    this.joinError.set('');
    this.joinLoading.set(false);
  }

  submitJoinRequest() {
    const circle = this.selectedJoinCircle();
    if (!circle || !this.joinAgreed()) return;
    this.joinLoading.set(true);
    this.joinError.set('');
    const data: SubmitJoinRequest = { agreedToTerms: true, message: this.joinMessage() || undefined };
    this.circleService.submitJoinRequest(circle.id, data).subscribe({
      next: () => {
        this.joinLoading.set(false);
        this.loadPublicCircles();
        this.closeJoinModal();
      },
      error: (err: any) => {
        this.joinLoading.set(false);
        this.joinError.set(err.error?.title || err.error?.message || 'Failed to submit request');
      }
    });
  }

  statusLabel(status: number): string {
    return ['Open', 'Full', 'Started', 'Completed'][status] || 'Unknown';
  }

  circleStatusLabel(status: number): string {
    return ['Forming', 'Active', 'Completed'][status] || 'Unknown';
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
