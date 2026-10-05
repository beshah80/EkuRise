import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotificationDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/notifications';

  getNotifications(): Observable<NotificationDto[]> {
    return this.http.get<NotificationDto[]>(this.baseUrl);
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/unread-count`);
  }

  markAsRead(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/read`, {});
  }

  markAllRead(): Observable<any> {
    return this.http.put(`${this.baseUrl}/read-all`, {});
  }
}
