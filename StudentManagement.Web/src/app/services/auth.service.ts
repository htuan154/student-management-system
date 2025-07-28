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
   * Lưu access và refresh token vào localStorage.
   */
  private saveTokens(tokens: TokenResponse): void {
    localStorage.setItem('access_token', tokens.access_token);
    localStorage.setItem('refresh_token', tokens.refresh_token);
  }
}
