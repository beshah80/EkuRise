import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateCircle, AddMember, CircleSummary, CircleDetail, MemberDto, MemberHome,
  PublicCircleSummary, JoinRequest, SubmitJoinRequest, ReviewJoinRequest, MarkJoinRequestPaid
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class CircleService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/circles`;

  createCircle(data: CreateCircle): Observable<CircleDetail> {
    return this.http.post<CircleDetail>(this.baseUrl, data);
  }

  getMyCircles(): Observable<CircleSummary[]> {
    return this.http.get<CircleSummary[]>(this.baseUrl);
  }

  getCircleById(id: number): Observable<CircleDetail> {
    return this.http.get<CircleDetail>(`${this.baseUrl}/${id}`);
  }

  addMember(circleId: number, data: AddMember): Observable<MemberDto> {
    return this.http.post<MemberDto>(`${this.baseUrl}/${circleId}/members`, data);
  }

  removeMember(circleId: number, userId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${circleId}/members/${userId}`);
  }

  startCircle(circleId: number): Observable<CircleDetail> {
    return this.http.post<CircleDetail>(`${this.baseUrl}/${circleId}/start`, {});
  }

  getMemberStatus(circleId: number): Observable<MemberHome> {
    return this.http.get<MemberHome>(`${this.baseUrl}/${circleId}/my-status`);
  }

  getPublicCircles(categoryId?: number): Observable<PublicCircleSummary[]> {
    const params = categoryId != null ? `?categoryId=${categoryId}` : '';
    return this.http.get<PublicCircleSummary[]>(`${this.baseUrl}/public${params}`);
  }

  submitJoinRequest(circleId: number, data: SubmitJoinRequest): Observable<JoinRequest> {
    return this.http.post<JoinRequest>(`${this.baseUrl}/${circleId}/join`, data);
  }

  getJoinRequests(circleId: number): Observable<JoinRequest[]> {
    return this.http.get<JoinRequest[]>(`${this.baseUrl}/${circleId}/join-requests`);
  }

  reviewJoinRequest(circleId: number, requestId: number, data: ReviewJoinRequest): Observable<JoinRequest> {
    return this.http.put<JoinRequest>(`${this.baseUrl}/${circleId}/join-requests/${requestId}`, data);
  }

  markJoinRequestPaid(circleId: number, requestId: number, hasPaid: boolean): Observable<JoinRequest> {
    return this.http.put<JoinRequest>(`${this.baseUrl}/${circleId}/join-requests/${requestId}/mark-paid`, { hasPaid } as MarkJoinRequestPaid);
  }
}
