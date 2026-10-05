import { Component, signal, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../services/catalog.service';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { SubCategoryDetail as SubCategoryDetailModel } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-sub-category-detail',
  imports: [FormsModule],
  templateUrl: './sub-category-detail.html',
  styleUrl: './sub-category-detail.css'
})
export class SubCategoryDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private catalog = inject(CatalogService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  sub = signal<SubCategoryDetailModel | null>(null);
  loading = signal(true);
  error = signal('');
  success = signal('');
  agreed = false;

  // Payment & KYC Form fields
  fullName = '';
  nationalIdFan = '';
  paymentProofUrl = '';
  previewUrl = '';
  submittingProof = signal(false);

  ngOnInit() {
    this.load();
    // Prefill name from user profile
    this.auth.getProfile().subscribe({
      next: (p) => {
        if (!this.fullName) {
          this.fullName = `${p.firstName} ${p.lastName}`.trim();
        }
      },
      error: () => {}
    });
  }

  load() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.catalog.getSubCategoryById(id).subscribe({
      next: (s) => {
        this.sub.set(s);
        if (s?.submittedFullName) this.fullName = s.submittedFullName;
        if (s?.submittedNationalIdFan) this.nationalIdFan = s.submittedNationalIdFan;
        if (s?.submittedPaymentProofUrl) this.previewUrl = s.submittedPaymentProofUrl;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load details');
        this.loading.set(false);
      }
    });
  }

  join() {
    if (!this.agreed || !this.sub()) return;
    this.loading.set(true);
    this.catalog.joinSubCategory(this.sub()!.id, { agreedToTerms: true }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.toast.success('Step 1 Complete! Please submit your payment & National ID (FAN) below.');
        this.sub.update(s => s ? {
          ...s,
          subscriptionId: res.subscriptionId,
          subscriptionStatus: res.subscriptionStatus ?? 0
        } : s);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.error.set(err.error?.title || 'Failed to submit join request');
        this.toast.error(err.error?.title || 'Failed to join');
      }
    });
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.paymentProofUrl = e.target.result;
        this.previewUrl = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  useDemoReceipt() {
    // Generate a quick SVG demo payment receipt data URL
    const subName = this.sub()?.name || 'Ekub Plan';
    const amount = this.sub()?.dailyContribution || 300;
    const svg = `
      <svg xmlns="http://www.w3.org/2000/svg" width="400" height="300" viewBox="0 0 400 300">
        <rect width="400" height="300" fill="#0F6B2D" rx="16"/>
        <rect x="20" y="20" width="360" height="260" fill="#ffffff" rx="12"/>
        <text x="200" y="60" font-family="sans-serif" font-size="18" font-weight="bold" fill="#0F6B2D" text-anchor="middle">TELEBIRR PAYMENT RECEIPT</text>
        <line x1="40" y1="80" x2="360" y2="80" stroke="#e5e7eb" stroke-width="2"/>
        <text x="50" y="115" font-family="sans-serif" font-size="14" fill="#6b7280">Transaction ID:</text>
        <text x="350" y="115" font-family="sans-serif" font-size="14" font-weight="bold" fill="#111827" text-anchor="end">TB-${Date.now().toString().slice(-8)}</text>
        <text x="50" y="150" font-family="sans-serif" font-size="14" fill="#6b7280">Paid For:</text>
        <text x="350" y="150" font-family="sans-serif" font-size="14" font-weight="bold" fill="#111827" text-anchor="end">${subName}</text>
        <text x="50" y="185" font-family="sans-serif" font-size="14" fill="#6b7280">Amount Paid:</text>
        <text x="350" y="185" font-family="sans-serif" font-size="16" font-weight="bold" fill="#0F6B2D" text-anchor="end">${amount} ETB</text>
        <text x="50" y="220" font-family="sans-serif" font-size="14" fill="#6b7280">Status:</text>
        <text x="350" y="220" font-family="sans-serif" font-size="14" font-weight="bold" fill="#15803d" text-anchor="end">SUCCESSFUL ✓</text>
        <text x="200" y="260" font-family="sans-serif" font-size="11" fill="#9ca3af" text-anchor="middle">Official Electronic Transaction Record</text>
      </svg>
    `;
    const encoded = 'data:image/svg+xml;utf8,' + encodeURIComponent(svg.trim());
    this.paymentProofUrl = encoded;
    this.previewUrl = encoded;
  }

  submitPaymentProof() {
    const s = this.sub();
    if (!s || !s.subscriptionId) {
      this.toast.error('Subscription record not found. Please join first.');
      return;
    }
    if (!this.fullName.trim()) {
      this.toast.error('Please enter your Full Name.');
      return;
    }
    if (!this.nationalIdFan.trim()) {
      this.toast.error('Please enter your National ID / Fayda FAN Number.');
      return;
    }
    if (!this.paymentProofUrl) {
      this.toast.error('Please upload or generate a payment screenshot.');
      return;
    }

    this.submittingProof.set(true);
    this.catalog.submitPaymentProof(s.subscriptionId, {
      fullName: this.fullName.trim(),
      nationalIdFan: this.nationalIdFan.trim(),
      paymentProofUrl: this.paymentProofUrl
    }).subscribe({
      next: (res) => {
        this.submittingProof.set(false);
        this.toast.success('Payment proof & National ID (FAN) submitted! Awaiting Admin approval.');
        this.sub.update(sub => sub ? {
          ...sub,
          subscriptionStatus: 1,
          submittedFullName: res.fullName,
          submittedNationalIdFan: res.nationalIdFan,
          submittedPaymentProofUrl: res.paymentProofUrl
        } : sub);
      },
      error: (err: any) => {
        this.submittingProof.set(false);
        this.toast.error(err.error?.title || 'Failed to submit payment proof');
      }
    });
  }

  viewCircle() {
    if (this.sub()?.circleId) {
      this.router.navigate(['/circles', this.sub()!.circleId]);
    }
  }

  goToNotifications() {
    this.router.navigate(['/notifications']);
  }

  goBack() {
    this.router.navigate(['/home']);
  }

  statusLabel(s: number) {
    return ['Open', 'Full', 'Started', 'Completed'][s] || 'Unknown';
  }

  statusClass(s: number) {
    return ['open', 'full', 'started', 'completed'][s] || '';
  }

  formatDate(d: string) {
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
