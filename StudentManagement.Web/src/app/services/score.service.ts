import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Score } from '../models';

export interface PagedScoreResponse {
  scores: Score[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScoreService {
  private apiUrl = `${environment.apiUrl}/Score`;

  constructor(private http: HttpClient) { }

  getAllScores(): Observable<Score[]> {
    return this.http.get<Score[]>(this.apiUrl);
  }

  getScoreById(id: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/${id}`);
  }

  getScoreByEnrollmentId(enrollmentId: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/enrollment/${enrollmentId}`);
  }

  /**
   * Lấy danh sách điểm theo ID giảng viên và ID môn học.
   * @param teacherId ID của giảng viên.
   * @param courseId ID của môn học.
   */
  getByTeacherAndSubject(teacherId: string, courseId: string): Observable<Score[]> {
    // Đảm bảo gửi đi tham số 'courseId'
    const params = new HttpParams()
      .set('teacherId', teacherId)
      .set('courseId', courseId);
    return this.http.get<Score[]>(`${this.apiUrl}/teacher-subject`, { params });
  }

  getPagedScores(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedScoreResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedScoreResponse>(`${this.apiUrl}/paged`, { params });
  }

  createScore(scoreData: Omit<Score, 'scoreId' | 'enrollment' | 'totalScore' | 'isPassed'>): Observable<any> {
    return this.http.post(this.apiUrl, scoreData);
  }

  updateScore(id: number, scoreData: Partial<Score>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, scoreData);
  }

  deleteScore(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
