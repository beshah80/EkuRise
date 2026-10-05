import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TokenService } from './token.service';
import {
  RegisterRequest, VerifyRegistration, SendOtp, VerifyOtp, PinLogin,
  SetPin, UpdateProfile, AuthResponse, OtpSent, UserProfile,
  DeleteAccountRequest, ConfirmDeleteAccount, UserDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);
  private readonly baseUrl = 'http://localhost:5000/api/auth';

  register(data: RegisterRequest): Observable<OtpSent> {
    return this.http.post<OtpSent>(`${this.baseUrl}/register`, data);
  }

  verifyRegistration(data: VerifyRegistration): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register/verify`, data);
  }

  sendLoginOtp(data: SendOtp): Observable<OtpSent> {
    return this.http.post<OtpSent>(`${this.baseUrl}/login/otp`, data);
  }

  verifyOtp(data: VerifyOtp): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login/verify`, data);
  }

  pinLogin(data: PinLogin): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login/pin`, data);
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/me`);
  }

  updateProfile(data: UpdateProfile): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.baseUrl}/me`, data);
  }

  setPin(data: SetPin): Observable<any> {
    return this.http.post(`${this.baseUrl}/me/pin`, data);
  }

  requestDeleteAccount(data: DeleteAccountRequest): Observable<OtpSent> {
    return this.http.post<OtpSent>(`${this.baseUrl}/delete-account/request`, data);
  }

  confirmDeleteAccount(data: ConfirmDeleteAccount): Observable<any> {
    return this.http.post(`${this.baseUrl}/delete-account/confirm`, data);
  }

  getUserById(userId: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.baseUrl}/user/${userId}`);
  }

  logout(): void {
    this.tokenService.removeToken();
  }
}
