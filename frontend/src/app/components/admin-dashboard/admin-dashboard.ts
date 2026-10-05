import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { CatalogService } from '../../services/catalog.service';
import { ToastService } from '../../services/toast.service';
import { TokenService } from '../../services/token.service';
import {
  AdminStats, AdminUser, AdminStory, AdminFeedback,
  Category, SubCategory
} from '../../models/models';

type AdminTab = 'overview' | 'catalog' | 'stories' | 'users' | 'feedback';

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  private fb = inject(FormBuilder);
  private adminService = inject(AdminService);
  private catalogService = inject(CatalogService);
  private toast = inject(ToastService);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  currentUserId = this.tokenService.getUserId();

  activeTab = signal<AdminTab>('overview');
  loading = signal(false);

  // Stats
  stats = signal<AdminStats | null>(null);

  // Catalog
  categories = signal<Category[]>([]);
  subCatsMap = signal<Map<number, SubCategory[]>>(new Map());
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

  // Stories
  stories = signal<AdminStory[]>([]);
  storyFilter = signal<'all' | 'pending' | 'approved'>('all');
  filteredStories = computed(() => {
    const filter = this.storyFilter();
    const list = this.stories();
    if (filter === 'pending') return list.filter(s => !s.isApproved);
    if (filter === 'approved') return list.filter(s => s.isApproved);
    return list;
  });

  // Users
  users = signal<AdminUser[]>([]);
  userSearch = signal('');
  filteredUsers = computed(() => {
    const q = this.userSearch().trim().toLowerCase();
    const list = this.users();
    if (!q) return list;
    return list.filter(u =>
      (u.firstName + ' ' + u.lastName).toLowerCase().includes(q) ||
      u.phoneNumber.toLowerCase().includes(q) ||
      u.location.toLowerCase().includes(q) ||
      u.jobType.toLowerCase().includes(q)
    );
  });

  // Feedback
  feedbacks = signal<AdminFeedback[]>([]);
  feedbackFilter = signal<'all' | 'pending' | 'reviewed'>('all');
  filteredFeedbacks = computed(() => {
    const filter = this.feedbackFilter();
    const list = this.feedbacks();
    if (filter === 'pending') return list.filter(f => f.status === 0);
    if (filter === 'reviewed') return list.filter(f => f.status === 1);
    return list;
  });

  ngOnInit() {
    this.refreshAll();
  }

  setTab(tab: AdminTab) {
    this.activeTab.set(tab);
  }

  refreshAll() {
    this.loadStats();
    this.loadCatalog();
    this.loadStories();
    this.loadUsers();
    this.loadFeedbacks();
  }

  loadStats() {
    this.adminService.getStats().subscribe({
      next: (s) => this.stats.set(s),
      error: () => {}
    });
  }

  // --- Catalog logic ---
  loadCatalog() {
    this.catalogService.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        cats.forEach(c => this.loadSubCats(c.id));
      },
      error: () => {}
    });
  }

  loadSubCats(catId: number) {
    this.catalogService.getSubCategories(catId).subscribe({
      next: (subs) => {
        const m = new Map(this.subCatsMap());
        m.set(catId, subs);
        this.subCatsMap.set(m);
      },
      error: () => {}
    });
  }

  getSubCats(catId: number): SubCategory[] {
    return this.subCatsMap().get(catId) || [];
  }

  createCategory() {
    if (this.catForm.invalid) return;
    this.catalogService.createCategory(this.catForm.value as any).subscribe({
      next: () => {
        this.catForm.reset();
        this.toast.success('Category created successfully!');
        this.loadCatalog();
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to create category')
    });
  }

  createSubCategory(catId: number) {
    if (this.subForm.invalid) return;
    const val = { ...this.subForm.value, categoryId: catId };
    this.catalogService.createSubCategory(val as any).subscribe({
      next: () => {
        this.subForm.reset();
        this.activeCatForm.set(null);
        this.toast.success('Plan created successfully!');
        this.loadSubCats(catId);
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to create plan')
    });
  }

  startEkub(subId: number) {
    if (!confirm('Start this Ekub? A Circle will be generated with all joined members and Round 1 initiated.')) return;
    this.catalogService.startEkub(subId).subscribe({
      next: () => {
        this.toast.success('Ekub started! Circle created.');
        this.loadCatalog();
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to start Ekub')
    });
  }

  statusLabel(s: number) {
    return ['Open', 'Full', 'Started', 'Completed'][s] || 'Unknown';
  }

  // --- Stories logic ---
  loadStories() {
    this.adminService.getStories().subscribe({
      next: (stories) => this.stories.set(stories),
      error: () => {}
    });
  }

  approveStory(id: number) {
    this.adminService.approveStory(id).subscribe({
      next: () => {
        this.toast.success('Story approved and published!');
        this.loadStories();
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to approve story')
    });
  }

  deleteStory(id: number) {
    if (!confirm('Are you sure you want to remove this story?')) return;
    this.adminService.deleteStory(id).subscribe({
      next: () => {
        this.toast.info('Story removed.');
        this.loadStories();
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to remove story')
    });
  }

  // --- Users logic ---
  loadUsers() {
    this.adminService.getUsers().subscribe({
      next: (users) => this.users.set(users),
      error: () => {}
    });
  }

  toggleAdmin(user: AdminUser) {
    const action = user.isAdmin ? 'revoke admin access from' : 'grant admin privileges to';
    if (!confirm(`Are you sure you want to ${action} ${user.firstName} ${user.lastName}?`)) return;

    this.adminService.toggleUserAdmin(user.id).subscribe({
      next: (updated) => {
        this.toast.success(`Role updated for ${updated.firstName}`);
        this.loadUsers();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to update user role')
    });
  }

  // --- Feedback logic ---
  loadFeedbacks() {
    this.adminService.getFeedbacks().subscribe({
      next: (fbs) => this.feedbacks.set(fbs),
      error: () => {}
    });
  }

  updateFeedbackStatus(feedbackId: number, newStatus: number) {
    this.adminService.updateFeedbackStatus(feedbackId, newStatus).subscribe({
      next: () => {
        this.toast.success(newStatus === 1 ? 'Marked as reviewed' : 'Status reverted');
        this.loadFeedbacks();
        this.loadStats();
      },
      error: (err) => this.toast.error(err.error?.title || 'Failed to update feedback status')
    });
  }

  goBack() {
    this.router.navigate(['/account']);
  }
}
