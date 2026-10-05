import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CatalogService } from '../../services/catalog.service';
import { Category, SubCategory } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-admin-catalog',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-catalog.html',
  styleUrl: './admin-catalog.css'
})
export class AdminCatalog {
  private fb = inject(FormBuilder);
  private catalog = inject(CatalogService);
  private router = inject(Router);

  categories = signal<Category[]>([]);
  subCatsMap = signal<Map<number, SubCategory[]>>(new Map());
  error = signal('');
  success = signal('');
  activeCatForm = signal<number | null>(null);

  catForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: ['']
  });

  subForm = this.fb.group({
    name: ['', Validators.required],
    dailyContribution: [null as number | null, [Validators.required, Validators.min(1)]],
    totalRounds: [null as number | null, [Validators.required, Validators.min(2)]],
    startDate: ['', Validators.required],
    termsAndConditions: ['', [Validators.required, Validators.minLength(20)]],
    maxMembers: [null as number | null, [Validators.required, Validators.min(2)]]
  });

  ngOnInit() { this.load(); }

  load() {
    this.catalog.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        cats.forEach(c => this.loadSubCats(c.id));
      },
      error: () => this.error.set('Failed to load')
    });
  }

  loadSubCats(catId: number) {
    this.catalog.getSubCategories(catId).subscribe({
      next: (subs) => {
        const m = new Map(this.subCatsMap());
        m.set(catId, subs);
        this.subCatsMap.set(m);
      },
      error: () => {}
    });
  }

  getSubCats(catId: number): SubCategory[] { return this.subCatsMap().get(catId) || []; }

  createCategory() {
    if (this.catForm.invalid) return;
    this.catalog.createCategory(this.catForm.value as any).subscribe({
      next: () => { this.catForm.reset(); this.success.set('Category created!'); this.load(); },
      error: (err: any) => this.error.set(err.error?.title || 'Failed')
    });
  }

  createSubCategory(catId: number) {
    if (this.subForm.invalid) return;
    const val = { ...this.subForm.value, categoryId: catId };
    this.catalog.createSubCategory(val as any).subscribe({
      next: () => {
        this.subForm.reset();
        this.activeCatForm.set(null);
        this.success.set('Sub-category created!');
        this.loadSubCats(catId);
      },
      error: (err: any) => this.error.set(err.error?.title || 'Failed')
    });
  }

  startEkub(subId: number) {
    if (!confirm('Start this Ekub? A circle will be created for all joined members.')) return;
    this.catalog.startEkub(subId).subscribe({
      next: () => { this.success.set('Ekub started!'); this.load(); },
      error: (err: any) => this.error.set(err.error?.title || 'Failed to start')
    });
  }

  statusLabel(s: number) { return ['Open', 'Full', 'Started', 'Completed'][s] || 'Unknown'; }
  goBack() { this.router.navigate(['/account']); }
}
