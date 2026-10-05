import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RoundService } from '../../services/round.service';
import { RoundSummary } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-round-history',
  imports: [FormsModule],
  templateUrl: './round-history.html',
  styleUrl: './round-history.css'
})
export class RoundHistory {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private roundService = inject(RoundService);

  rounds = signal<RoundSummary[]>([]);
  loading = signal(true);
  error = signal('');
  circleId = 0;
  filterRound: number | undefined;
  filterStatus = '';

  ngOnInit() {
    this.circleId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadRounds();
  }

  loadRounds() {
    this.loading.set(true);
    const status = this.filterStatus !== '' ? Number(this.filterStatus) : undefined;
    this.roundService.getRounds(this.circleId, this.filterRound, status).subscribe({
      next: (r) => { this.rounds.set(r); this.loading.set(false); },
      error: () => { this.error.set('Failed to load rounds'); this.loading.set(false); }
    });
  }

  viewRound(roundId: number) { this.router.navigate(['/circles', this.circleId, 'rounds', roundId]); }
  goBack() { this.router.navigate(['/circles', this.circleId]); }
  statusLabel(s: number) { return ['Pending', 'Open', 'Paid Out'][s] || 'Unknown'; }
  statusClass(s: number) { return ['pending', 'open', 'paidout'][s] || ''; }
  formatDate(d: string) {
    return d && d !== '0001-01-01T00:00:00'
      ? new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
      : '-';
  }
}
