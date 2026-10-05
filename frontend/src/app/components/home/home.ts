import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { CatalogService } from '../../services/catalog.service';
import { CircleService } from '../../services/circle.service';
import { Category, SubCategory, CircleSummary } from '../../models/models';

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
  selectedCategory = signal<number | null>(null);
  loading = signal(true);
  error = signal('');

  ngOnInit() {
    this.loadCategories();
    this.loadCircles();
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
