import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RoundService } from '../../services/round.service';
import { TokenService } from '../../services/token.service';
import { CircleService } from '../../services/circle.service';
import { RoundDetail as RoundDetailModel, PayoutResult } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-round-detail',
  imports: [FormsModule],
  templateUrl: './round-detail.html',
  styleUrl: './round-detail.css'
})
export class RoundDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private roundService = inject(RoundService);
  private tokenService = inject(TokenService);
  private circleService = inject(CircleService);

  round = signal<RoundDetailModel | null>(null);
  loading = signal(true);
  error = signal('');
  success = signal('');
  confirmPayout = signal(false);
  payoutResult = signal<PayoutResult | null>(null);
  spinning = signal(false);
  spinRevealed = signal(false);
  spinName = signal('');
  circleId = 0;
  isOrganizer = false;

  private spinInterval: any;
  private spinNames: string[] = [];

  ngOnInit() {
    this.circleId = Number(this.route.snapshot.paramMap.get('id'));
    const roundId = Number(this.route.snapshot.paramMap.get('roundId'));
    const userId = this.tokenService.getUserId() || 0;
    this.circleService.getCircleById(this.circleId).subscribe({
      next: (c) => { this.isOrganizer = c.organizerId === userId; },
      error: () => {}
    });
    this.loadRound(roundId);
  }

  loadRound(roundId?: number) {
    const id = roundId || this.round()!.id;
    this.roundService.getRoundById(this.circleId, id).subscribe({
      next: (r) => { this.round.set(r); this.loading.set(false); },
      error: () => { this.error.set('Round not found'); this.loading.set(false); }
    });
  }

  togglePayment(userId: number, hasPaid: boolean) {
    this.roundService.markPayment(this.circleId, this.round()!.id, { userId, hasPaid }).subscribe({
      next: (r) => this.round.set(r),
      error: (err: any) => this.error.set(err.error?.title || 'Failed to update')
    });
  }

  payout() {
    this.confirmPayout.set(false);
    this.spinNames = this.round()?.payments.map(p => p.memberName.split(' ')[0]) ?? [];
    this.spinning.set(true);
    this.spinRevealed.set(false);
    this.error.set('');
    let i = 0;
    this.spinInterval = setInterval(() => {
      this.spinName.set(this.spinNames[i % this.spinNames.length]);
      i++;
    }, 80);
    setTimeout(() => {
      this.roundService.payOut(this.circleId, this.round()!.id).subscribe({
        next: (res) => {
          clearInterval(this.spinInterval);
          // slow down then stop on winner
          let slowCount = 0;
          const slowInterval = setInterval(() => {
            this.spinName.set(res.receiverName.split(' ')[0]);
            slowCount++;
            if (slowCount >= 6) {
              clearInterval(slowInterval);
              this.spinRevealed.set(true);
              this.payoutResult.set(res);
            }
          }, 200);
        },
        error: (err: any) => {
          clearInterval(this.spinInterval);
          this.spinning.set(false);
          this.error.set(err.error?.message || err.error?.title || 'Payout failed');
        }
      });
    }, 2500);
  }

  dismissWinner() {
    this.spinning.set(false);
    this.spinRevealed.set(false);
    this.loadRound();
  }

  openNext() {
    this.loading.set(true);
    this.roundService.openNextRound(this.circleId).subscribe({
      next: (r) => { this.loading.set(false); this.router.navigate(['/circles', this.circleId, 'rounds', r.id]); },
      error: (err: any) => { this.loading.set(false); this.error.set(err.error?.title || 'Failed'); }
    });
  }

  allPaid() { return this.round()?.payments.every(p => p.hasPaid); }
  goBack() { this.router.navigate(['/circles', this.circleId]); }
  statusLabel(s: number) { return ['Pending', 'Open', 'Paid Out'][s] || 'Unknown'; }
  statusClass(s: number) { return ['', 'open', 'paidout'][s] || ''; }
  formatDate(d: string) { return d ? new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : '-'; }
}
