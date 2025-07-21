import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Score } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang điểm số.
 */
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

  /**
   * Lấy thông tin điểm theo ID.
   * @param id ID của điểm.
   */
  getScoreById(id: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy thông tin điểm theo ID của lượt đăng ký.
   * @param enrollmentId ID của lượt đăng ký.
   */
  getScoreByEnrollmentId(enrollmentId: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/enrollment/${enrollmentId}`);
  }

  /**
   * Tìm kiếm điểm số.
   * @param term Từ khóa tìm kiếm.
   */
  searchScores(term: string): Observable<Score[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Score[]>(`${this.apiUrl}/search`, { params });
  }

  /**
   * Lấy danh sách điểm được phân trang.
   */
  getPagedScores(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedScoreResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedScoreResponse>(`${this.apiUrl}/paged`, { params });
  }

  /**
   * Tạo một bản ghi điểm mới.
   * @param scoreData Dữ liệu để tạo điểm.
   */
  createScore(scoreData: Omit<Score, 'scoreId' | 'enrollment' | 'totalScore' | 'isPassed'>): Observable<any> {
    return this.http.post(this.apiUrl, scoreData);
  }

  /**
   * Cập nhật một bản ghi điểm.
   * @param id ID của điểm cần cập nhật.
   * @param scoreData Dữ liệu cập nhật.
   */
  updateScore(id: number, scoreData: Partial<Score>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, scoreData);
  }

  /**
   * Xóa một bản ghi điểm.
   * @param id ID của điểm cần xóa.
   */
  deleteScore(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
