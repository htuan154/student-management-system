// src/app/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
import { LoginDto, TokenResponse, RefreshTokenDto } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;
  constructor(private http: HttpClient) {}

  // ---------- Auth APIs ----------
  login(loginData: LoginDto): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/login`, loginData).pipe(
      tap(tokens => this.saveTokens(tokens))
    );
  }

  refreshToken(refreshToken: string): Observable<TokenResponse> {
    const dto: RefreshTokenDto = { refreshToken };
    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh`, dto).pipe(
      tap(tokens => this.saveTokens(tokens))
    );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }

  // ---------- Helpers ----------
  private decodeToken(): any | null {
    const token = this.getAccessToken();
    try { return token ? jwtDecode(token) : null; } catch { return null; }
  }

  getDecodedToken(): any | null { return this.decodeToken(); }

  /** Đọc 1 claim trong token với nhiều key có thể có */
  private getClaim(keys: string[]): string | null {
    const p = this.decodeToken();
    if (!p) return null;
    for (const k of keys) {
      const v = (p as any)[k];
      if (typeof v === 'string' && v.trim()) return v;
    }
    return null;
  }

  getUserRole(): string | null {
    return this.getClaim([
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
      'role',
      'roles'
    ]);
  }

  getUsername(): string | null {
    return this.getClaim([
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
      'unique_name',
      'username',
      'name'
    ]);
  }
  getCurrentUsername(): string | null {
    return this.getUsername();
  }

  /** Quan trọng: chuẩn hoá lấy UserId */
  getCurrentUserId(): string | null {
    return this.getClaim([
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
      'nameidentifier',
      'nameid',
      'sub',
      'userId',
      'uid',
      'id'
    ]);
  }
  // alias
  getUserId(): string | null { return this.getCurrentUserId(); }

  getTeacherId(): string | null {
    return this.getClaim(['teacherId', 'TeacherId', 'tId']);
  }
  getStudentId(): string | null {
    return this.getClaim(['studentId', 'StudentId', 'sId']);
  }

  getCurrentUserEmail(): string | null {
    return this.getClaim([
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
      'email'
    ]);
  }

  isAuthenticated(): boolean {
    const p = this.decodeToken();
    return !!(p?.exp && p.exp * 1000 > Date.now());
  }

  getAccessToken(): string | null { return localStorage.getItem('access_token'); }
  getRefreshToken(): string | null { return localStorage.getItem('refresh_token'); }

  hasRole(role: string): boolean { return this.getUserRole() === role; }
  isAdmin(): boolean { return this.hasRole('Admin') || this.hasRole('SuperAdmin'); }
  isTeacher(): boolean { return this.hasRole('Teacher'); }
  isStudent(): boolean { return this.hasRole('Student'); }

  private saveTokens(tokens: TokenResponse): void {
    localStorage.setItem('access_token', tokens.access_token);
    localStorage.setItem('refresh_token', tokens.refresh_token);
  }
}
