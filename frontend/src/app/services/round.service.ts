import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  RoundSummary, RoundDetail, MarkPayment, PayoutResult
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class RoundService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/circles';

  getRounds(circleId: number, roundNumber?: number, status?: number): Observable<RoundSummary[]> {
    let params = new HttpParams();
    if (roundNumber) params = params.set('roundNumber', roundNumber);
    if (status !== undefined) params = params.set('status', status);
    return this.http.get<RoundSummary[]>(`${this.baseUrl}/${circleId}/rounds`, { params });
  }

  getCurrentRound(circleId: number): Observable<RoundDetail> {
    return this.http.get<RoundDetail>(`${this.baseUrl}/${circleId}/rounds/current`);
  }

  getRoundById(circleId: number, roundId: number): Observable<RoundDetail> {
    return this.http.get<RoundDetail>(`${this.baseUrl}/${circleId}/rounds/${roundId}`);
  }

  markPayment(circleId: number, roundId: number, data: MarkPayment): Observable<RoundDetail> {
    return this.http.put<RoundDetail>(`${this.baseUrl}/${circleId}/rounds/${roundId}/payments`, data);
  }

  payOut(circleId: number, roundId: number): Observable<PayoutResult> {
    return this.http.post<PayoutResult>(`${this.baseUrl}/${circleId}/rounds/${roundId}/payout`, {});
  }

  openNextRound(circleId: number): Observable<RoundDetail> {
    return this.http.post<RoundDetail>(`${this.baseUrl}/${circleId}/rounds/next`, {});
  }
}
