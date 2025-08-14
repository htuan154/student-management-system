import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TeacherCourse } from '../models/teacher-course.model';
import { environment } from '../../environments/environment';

export interface PagedTeacherCourseResponse {
  tcs: TeacherCourse[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class TeacherCourseService {
  // BE: [Route("api/[controller]")] => /api/TeacherCourse
  private apiUrl = `${environment.apiUrl}/TeacherCourse`;

  constructor(private http: HttpClient) {}

  /** Lấy tất cả phân công */
  getAllTeacherCourses(): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(this.apiUrl);
  }

  /** Lấy phân công theo ID */
  getTeacherCourseById(id: number): Observable<TeacherCourse> {
    return this.http.get<TeacherCourse>(`${this.apiUrl}/${id}`);
  }

  /** Lấy phân công theo Teacher ID */
  getTeacherCoursesByTeacherId(teacherId: string): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/teacher/${encodeURIComponent(teacherId)}`);
  }

  /** Lấy phân công theo Course ID */
  getTeacherCoursesByCourseId(courseId: string): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/course/${encodeURIComponent(courseId)}`);
  }

  /** Lấy phân công theo Semester ID */
  getTeacherCoursesBySemesterId(semesterId: number): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/semester/${semesterId}`);
  }

  /** Tạo phân công mới */
  createTeacherCourse(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  /** Cập nhật phân công */
  updateTeacherCourse(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  /** Xóa phân công */
  deleteTeacherCourse(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Tìm kiếm phân công */
  searchTeacherCourses(searchTerm: string): Observable<TeacherCourse[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/search`, { params });
  }

  /** Kiểm tra phân công đã tồn tại */
  checkTeacherCourseExists(teacherId: string, courseId: string, semesterId: number, excludeId?: number) {
    let params = new HttpParams()
      .set('teacherId', teacherId)
      .set('courseId', courseId)
      .set('semesterId', String(semesterId));
    if (excludeId != null) params = params.set('excludeId', String(excludeId));
    return this.http.get<{ exists: boolean }>(`${this.apiUrl}/check-exists`, { params });
  }

  /** Phân trang */
  getPagedTeacherCourses(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedTeacherCourseResponse> {
    let params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get<PagedTeacherCourseResponse>(`${this.apiUrl}/paged`, { params });
  }

  // Wrapper cho code cũ trong component (nếu đang gọi getByCourseId)
  getByCourseId(courseId: string): Observable<TeacherCourse[]> {
    return this.getTeacherCoursesByCourseId(courseId);
  }

  // Wrapper cho code cũ trong component
  getById(id: number): Observable<TeacherCourse> {
    return this.getTeacherCourseById(id);
  }
}
