import { Component, signal, inject, computed } from '@angular/core';
import { Router } from '@angular/router';
import { CatalogService } from '../../services/catalog.service';
import { MyEkub } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-my-ekubs',
  imports: [],
  templateUrl: './my-ekubs.html',
  styleUrl: './my-ekubs.css'
})
export class MyEkubs {
  private catalog = inject(CatalogService);
  private router = inject(Router);

  ekubs = signal<MyEkub[]>([]);
  loading = signal(true);
  filter = signal('');

  filtered = computed(() => {
    const f = this.filter();
    if (!f) return this.ekubs();
    return this.ekubs().filter(e => e.status.toString() === f);
  });

  ngOnInit() {
    this.catalog.getMyEkubs().subscribe({
      next: (e) => { this.ekubs.set(e); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  viewCircle(circleId: number) { this.router.navigate(['/circles', circleId]); }
  goBack() { this.router.navigate(['/account']); }
  goHome() { this.router.navigate(['/home']); }

  statusLabel(s: number) { return ['Open', 'Full', 'Started', 'Completed'][s] || 'Unknown'; }
  statusClass(s: number) { return ['open', 'full', 'started', 'completed'][s] || ''; }
  formatDate(d: string) { return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }); }
}
