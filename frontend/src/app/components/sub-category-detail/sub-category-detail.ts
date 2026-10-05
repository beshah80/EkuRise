import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../services/catalog.service';
import { SubCategoryDetail as SubCategoryDetailModel } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-sub-category-detail',
  imports: [FormsModule],
  templateUrl: './sub-category-detail.html',
  styleUrl: './sub-category-detail.css'
})
export class SubCategoryDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private catalog = inject(CatalogService);

  sub = signal<SubCategoryDetailModel | null>(null);
  loading = signal(true);
  error = signal('');
  success = signal('');
  agreed = false;

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.catalog.getSubCategoryById(id).subscribe({
      next: (s) => { this.sub.set(s); this.loading.set(false); },
      error: () => { this.error.set('Failed to load details'); this.loading.set(false); }
    });
  }

  join() {
    if (!this.agreed || !this.sub()) return;
    this.loading.set(true);
    this.catalog.joinSubCategory(this.sub()!.id, { agreedToTerms: true }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.success.set(res.message);
        this.sub.update(s => s ? { ...s, hasJoined: true, currentMemberCount: s.currentMemberCount + 1 } : s);
      },
      error: (err: any) => { this.loading.set(false); this.error.set(err.error?.title || 'Failed to join'); }
    });
  }

  viewCircle() { this.router.navigate(['/circles', this.sub()!.circleId]); }
  goBack() { this.router.navigate(['/home']); }
  statusLabel(s: number) { return ['Open', 'Full', 'Started', 'Completed'][s] || 'Unknown'; }
  statusClass(s: number) { return ['open', 'full', 'started', 'completed'][s] || ''; }
  formatDate(d: string) { return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }); }
}
