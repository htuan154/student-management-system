import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

import { LoginDto, TokenResponse, RefreshTokenDto } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) {}

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

  // 🔍 Giải mã token hiện tại
  private decodeToken(): any | null {
    const token = this.getAccessToken();
    try {
      return token ? jwtDecode(token) : null;
    } catch (e) {
      return null;
    }
  }

  /**
   * Lấy thông tin từ access token đã giải mã.
   * Dùng để lấy các thông tin như studentId, username...
   */
  getDecodedToken(): any | null {
    return this.decodeToken();
  }

  /**
   * Lấy vai trò người dùng hiện tại từ access token.
   */
  getUserRole(): string | null {
    const decoded = this.decodeToken();
    return decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null;
  }

  /**
   * Lấy ID người dùng hiện tại từ access token.
   */
  getCurrentUserId(): string | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;

    // Thử các trường phổ biến cho user ID trong JWT token
    return decoded.sub ||
           decoded.userId ||
           decoded.id ||
           decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
           decoded.nameid ||
           null;
  }

  /**
   * Lấy username từ access token.
   */
  getCurrentUsername(): string | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;

    return decoded.username ||
           decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
           decoded.name ||
           null;
  }

  /**
   * Lấy email từ access token.
   */
  getCurrentUserEmail(): string | null {
    const decoded = this.decodeToken();
    if (!decoded) return null;

    return decoded.email ||
           decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
           null;
  }

  /**
   * Kiểm tra người dùng đã đăng nhập hay chưa (token còn hạn hay không).
   */
  isAuthenticated(): boolean {
    const decoded = this.decodeToken();
    if (!decoded || !decoded.exp) return false;
    return decoded.exp * 1000 > Date.now();
  }

  /**
   * Lấy access token hiện tại từ localStorage.
   */
  getAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }

  /**
   * Lấy refresh token hiện tại từ localStorage.
   */
  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  /**
   * Kiểm tra người dùng có vai trò cụ thể hay không.
   */
  hasRole(role: string): boolean {
    const userRole = this.getUserRole();
    return userRole === role;
  }

  /**
   * Kiểm tra người dùng có phải là Admin hay không.
   */
  isAdmin(): boolean {
    return this.hasRole('Admin') || this.hasRole('SuperAdmin');
  }

  /**
   * Kiểm tra người dùng có phải là Teacher hay không.
   */
  isTeacher(): boolean {
    return this.hasRole('Teacher');
  }

  /**
   * Kiểm tra người dùng có phải là Student hay không.
   */
  isStudent(): boolean {
    return this.hasRole('Student');
  }

  /**
   * Lưu access và refresh token vào localStorage.
   */
  private saveTokens(tokens: TokenResponse): void {
    localStorage.setItem('access_token', tokens.access_token);
    localStorage.setItem('refresh_token', tokens.refresh_token);
  }
}
