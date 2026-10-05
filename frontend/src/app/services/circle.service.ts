import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateCircle, AddMember, CircleSummary, CircleDetail, MemberDto, MemberHome
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class CircleService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/circles';

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
}
