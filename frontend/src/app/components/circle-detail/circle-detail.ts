import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CircleService } from '../../services/circle.service';
import { TokenService } from '../../services/token.service';
import { CircleDetail as CircleDetailModel, RoundDetail } from '../../models/models';
import { RoundService } from '../../services/round.service';

@Component({
  standalone: true,
  selector: 'app-circle-detail',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './circle-detail.html',
  styleUrl: './circle-detail.css'
})
export class CircleDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private circleService = inject(CircleService);
  private roundService = inject(RoundService);
  private tokenService = inject(TokenService);

  circle = signal<CircleDetailModel | null>(null);
  currentRound = signal<RoundDetail | null>(null);
  loading = signal(true);
  error = signal('');
  success = signal('');
  circleId = 0;
  currentUserId = 0;
  isOrganizer = signal(false);
  showConfirm = signal(false);

  addMemberForm = this.fb.group({
    phoneNumber: ['', [Validators.required, Validators.minLength(10)]]
  });

  ngOnInit() {
    this.circleId = Number(this.route.snapshot.paramMap.get('id'));
    this.currentUserId = this.tokenService.getUserId() || 0;
    this.loadCircle();
  }

  loadCircle() {
    this.circleService.getCircleById(this.circleId).subscribe({
      next: (c) => {
        this.circle.set(c);
        this.loading.set(false);
        this.isOrganizer.set(this.currentUserId !== 0);
        if (c.status === 1) this.loadCurrentRound();
      },
      error: () => { this.error.set('Circle not found'); this.loading.set(false); }
    });
  }

  loadCurrentRound() {
    this.roundService.getCurrentRound(this.circleId).subscribe({
      next: (r) => this.currentRound.set(r),
      error: () => {}
    });
  }

  addMember() {
    if (this.addMemberForm.invalid) return;
    this.circleService.addMember(this.circleId, this.addMemberForm.value as any).subscribe({
      next: () => { this.success.set('Member added!'); this.addMemberForm.reset(); this.loadCircle(); },
      error: (err: any) => this.error.set(err.error?.title || 'Failed to add member')
    });
  }

  removeMember(userId: number) {
    this.circleService.removeMember(this.circleId, userId).subscribe({
      next: () => this.loadCircle(),
      error: (err: any) => this.error.set(err.error?.title || 'Failed to remove member')
    });
  }

  startCircle() {
    this.showConfirm.set(false);
    this.loading.set(true);
    this.circleService.startCircle(this.circleId).subscribe({
      next: (c) => { this.circle.set(c); this.loading.set(false); this.success.set('Circle started!'); this.loadCurrentRound(); },
      error: (err: any) => { this.loading.set(false); this.error.set(err.error?.title || 'Failed to start'); }
    });
  }

  viewCurrentRound() {
    if (this.currentRound()) this.router.navigate(['/circles', this.circleId, 'rounds', this.currentRound()!.id]);
  }

  viewHistory() { this.router.navigate(['/circles', this.circleId, 'rounds']); }
  goBack() { this.router.navigate(['/home']); }

  myPayment() { return this.currentRound()?.payments.find(p => p.userId === this.currentUserId); }
  myMembership() { return this.circle()?.members.find(m => m.userId === this.currentUserId); }

  statusLabel(s: number) { return ['Forming', 'Active', 'Completed'][s] || 'Unknown'; }
  statusClass(s: number) { return ['forming', 'active', 'completed'][s] || ''; }
}
