import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TeacherCourse } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang.
 */
export interface PagedTeacherCourseResponse {
  tcs: TeacherCourse[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherCourseService {
  private apiUrl = `${environment.apiUrl}/TeacherCourse`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy thông tin phân công theo ID.
   * @param id ID của phân công.
   */
  getTeacherCourseById(id: number): Observable<TeacherCourse> {
    return this.http.get<TeacherCourse>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy danh sách phân công theo ID giáo viên.
   * @param teacherId ID của giáo viên.
   */
  getTeacherCoursesByTeacherId(teacherId: string): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/teacher/${teacherId}`);
  }

  /**
   * Lấy danh sách phân công theo ID khóa học.
   * @param courseId ID của khóa học.
   */
  getTeacherCoursesByCourseId(courseId: string): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /**
   * Tìm kiếm các phân công.
   * @param term Từ khóa tìm kiếm.
   */
  searchTeacherCourses(term: string): Observable<TeacherCourse[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/search`, { params });
  }

  /**
   * Lấy danh sách phân công được phân trang.
   */
  getPagedTeacherCourses(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedTeacherCourseResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedTeacherCourseResponse>(`${this.apiUrl}/paged`, { params });
  }

  /**
   * Tạo một phân công mới.
   * @param data Dữ liệu để tạo phân công.
   */
  createTeacherCourse(data: Omit<TeacherCourse, 'teacherCourseId' | 'teacher' | 'course'>): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  /**
   * Cập nhật một phân công.
   * @param id ID của phân công cần cập nhật.
   * @param data Dữ liệu cập nhật.
   */
  updateTeacherCourse(id: number, data: Partial<TeacherCourse>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  /**
   * Xóa một phân công.
   * @param id ID của phân công cần xóa.
   */
  deleteTeacherCourse(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
