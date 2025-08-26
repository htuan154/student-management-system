import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface StudentAvg {
  studentId: string;
  fullName: string;
  className: string;
  averageScore: number;
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private http = inject(HttpClient);
  // Nếu bạn không dùng proxy, đổi thành: environment.apiUrl + '/analytics'
  private baseUrl = '/api/analytics';

  getTopStudents(page = 1, size = 5, semesterId?: number): Observable<PagedResponse<StudentAvg>> {
    // Ép string để tránh ASP.NET model binding trả 400
    let params = new HttpParams()
      .set('page', String(page))
      .set('size', String(size));
    if (semesterId != null) params = params.set('semesterId', String(semesterId));

    return this.http.get<PagedResponse<StudentAvg>>(`${this.baseUrl}/top-students`, { params });
  }
}
