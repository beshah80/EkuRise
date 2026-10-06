import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminStats, AdminUser, AdminStory, AdminFeedback, EkubSubscription } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.baseUrl}/stats`);
  }

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.baseUrl}/users`);
  }

  toggleUserAdmin(userId: number): Observable<AdminUser> {
    return this.http.put<AdminUser>(`${this.baseUrl}/users/${userId}/toggle-admin`, {});
  }

  getStories(): Observable<AdminStory[]> {
    return this.http.get<AdminStory[]>(`${this.baseUrl}/stories`);
  }

  approveStory(storyId: number): Observable<AdminStory> {
    return this.http.put<AdminStory>(`${this.baseUrl}/stories/${storyId}/approve`, {});
  }

  deleteStory(storyId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/stories/${storyId}`);
  }

  getFeedbacks(): Observable<AdminFeedback[]> {
    return this.http.get<AdminFeedback[]>(`${this.baseUrl}/feedbacks`);
  }

  updateFeedbackStatus(feedbackId: number, status: number): Observable<AdminFeedback> {
    return this.http.put<AdminFeedback>(`${this.baseUrl}/feedbacks/${feedbackId}/status`, { status });
  }

  getSubscriptions(): Observable<EkubSubscription[]> {
    return this.http.get<EkubSubscription[]>(`${this.baseUrl}/subscriptions`);
  }

  approveSubscription(subscriptionId: number): Observable<EkubSubscription> {
    return this.http.put<EkubSubscription>(`${this.baseUrl}/subscriptions/${subscriptionId}/approve`, {});
  }

  rejectSubscription(subscriptionId: number, reason?: string): Observable<EkubSubscription> {
    return this.http.put<EkubSubscription>(`${this.baseUrl}/subscriptions/${subscriptionId}/reject`, { reason });
  }
}
