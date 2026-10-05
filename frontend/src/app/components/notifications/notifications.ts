import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { NotificationDto } from '../../models/models';

@Component({
  standalone: true,
  selector: 'app-notifications',
  imports: [],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css'
})
export class Notifications {
  private notifService = inject(NotificationService);
  private router = inject(Router);

  notifications = signal<NotificationDto[]>([]);
  loading = signal(true);

  ngOnInit() { this.load(); }

  load() {
    this.notifService.getNotifications().subscribe({
      next: (n) => { this.notifications.set(n); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  markRead(id: number) {
    this.notifService.markAsRead(id).subscribe(() => {
      this.notifications.update(list => list.map(n => n.id === id ? { ...n, isRead: true } : n));
    });
  }

  markAllRead() {
    this.notifService.markAllRead().subscribe(() => {
      this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
    });
  }

  click(n: NotificationDto) {
    this.markRead(n.id);
    if (n.relatedCircleId && n.relatedRoundId) {
      this.router.navigate(['/circles', n.relatedCircleId, 'rounds', n.relatedRoundId]);
    } else if (n.relatedCircleId) {
      this.router.navigate(['/circles', n.relatedCircleId]);
    } else if (n.title.toLowerCase().includes('payment') || n.title.toLowerCase().includes('membership')) {
      this.router.navigate(['/account/my-ekubs']);
    }
  }

  typeIcon(t: number): string {
    return ['💸', '🎉', '🔔', '👥', '🚀', '✅', '📢'][t] || '📢';
  }

  formatDate(d: string): string {
    const diff = Date.now() - new Date(d).getTime();
    const h = Math.floor(diff / 3600000);
    if (h < 1) return 'just now';
    if (h < 24) return `${h}h ago`;
    return `${Math.floor(h / 24)}d ago`;
  }

  unreadCount() { return this.notifications().filter(n => !n.isRead).length; }
}
