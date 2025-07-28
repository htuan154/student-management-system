import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Enrollment } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang.
 */
export interface PagedEnrollmentResponse {
  enrollments: Enrollment[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {
  private apiUrl = `${environment.apiUrl}/Enrollment`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy thông tin đăng ký theo ID.
   * @param id ID của lượt đăng ký.
   */
  getEnrollmentById(id: number): Observable<Enrollment> {
    return this.http.get<Enrollment>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy tất cả các lượt đăng ký của một sinh viên.
   * @param studentId ID của sinh viên.
   */
  getEnrollmentsByStudentId(studentId: string): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/student/${studentId}`);
  }

  /**
   * Lấy tất cả các lượt đăng ký trong một khóa học.
   * @param courseId ID của khóa học.
   */
  getEnrollmentsByCourseId(courseId: string): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /**
   * Tìm kiếm các lượt đăng ký.
   * @param term Từ khóa tìm kiếm.
   */
  searchEnrollments(term: string): Observable<Enrollment[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Enrollment[]>(`${this.apiUrl}/search`, { params });
  }

  /**
   * Lấy danh sách đăng ký được phân trang.
   */
  getPagedEnrollments(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedEnrollmentResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedEnrollmentResponse>(`${this.apiUrl}/paged`, { params });
  }

  /**
   * Tạo một lượt đăng ký mới.
   * @param enrollmentData Dữ liệu để tạo lượt đăng ký.
   */
  createEnrollment(enrollmentData: Omit<Enrollment, 'enrollmentId' | 'student' | 'course' | 'teacher' | 'score'>): Observable<any> {
    return this.http.post(this.apiUrl, enrollmentData);
  }

  /**
   * Cập nhật một lượt đăng ký.
   * @param id ID của lượt đăng ký cần cập nhật.
   * @param enrollmentData Dữ liệu cập nhật.
   */
  updateEnrollment(id: number, enrollmentData: Partial<Enrollment>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, enrollmentData);
  }

  /**
   * Xóa một lượt đăng ký.
   * @param id ID của lượt đăng ký cần xóa.
   */
  deleteEnrollment(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
  /**
 * Lấy danh sách các lượt đăng ký chưa được chấm điểm.
 * @returns Một Observable chứa mảng các EnrollmentDto.
 */
getUnscoredEnrollments(): Observable<Enrollment[]> {
  const url = `${this.apiUrl}/unscored`;
  return this.http.get<Enrollment[]>(url);
}
}
