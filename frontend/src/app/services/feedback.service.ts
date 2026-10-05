import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateFeedback, FeedbackDto, CreateSuccessStory, SuccessStoryDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api';

  submitFeedback(data: CreateFeedback): Observable<FeedbackDto> {
    return this.http.post<FeedbackDto>(`${this.baseUrl}/feedback`, data);
  }

  getMyFeedbacks(): Observable<FeedbackDto[]> {
    return this.http.get<FeedbackDto[]>(`${this.baseUrl}/feedback`);
  }

  getSuccessStories(): Observable<SuccessStoryDto[]> {
    return this.http.get<SuccessStoryDto[]>(`${this.baseUrl}/success-stories`);
  }

  submitSuccessStory(data: CreateSuccessStory): Observable<SuccessStoryDto> {
    return this.http.post<SuccessStoryDto>(`${this.baseUrl}/success-stories`, data);
  }
}
