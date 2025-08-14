import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TeacherCourse } from '../models/teacher-course.model';
import { environment } from '../../environments/environment';

export interface PagedTeacherCourseResponse {
  tcs: TeacherCourse[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherCourseService {
  private apiUrl = `${environment.apiUrl}/TeacherCourse`;
  private baseUrl = '/api/teacher-courses'; // chỉnh theo API của bạn

  constructor(private http: HttpClient) { }

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
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/teacher/${teacherId}`);
  }

  /** Lấy phân công theo Course ID */
  getTeacherCoursesByCourseId(courseId: string): Observable<TeacherCourse[]> {
    return this.http.get<TeacherCourse[]>(`${this.apiUrl}/course/${courseId}`);
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
  checkTeacherCourseExists(teacherId: string, courseId: string, semesterId: number, excludeId?: number): Observable<{exists: boolean}> {
    let params = new HttpParams()
      .set('teacherId', teacherId)
      .set('courseId', courseId)
      .set('semesterId', semesterId.toString());

    if (excludeId) {
      params = params.set('excludeId', excludeId.toString());
    }

    return this.http.get<{exists: boolean}>(`${this.apiUrl}/check-exists`, { params });
  }

  /** Bổ sung method này */
  getPagedTeacherCourses(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedTeacherCourseResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedTeacherCourseResponse>(`${this.apiUrl}/paged`, { params });
  }

  // Lấy danh sách phân công theo courseId
  getByCourseId(courseId: string): Observable<TeacherCourse[]> {
    // Nếu API của bạn khác (ví dụ: `${this.baseUrl}/course/${courseId}`) hãy chỉnh lại endpoint này
    return this.http
      .get<TeacherCourse[]>(`${this.baseUrl}/by-course/${encodeURIComponent(courseId)}`)
      .pipe(
        catchError(err => {
          if (err.status === 404) {
            // Fallback: lấy tất cả rồi filter theo courseId
            return this.http.get<TeacherCourse[]>(`${this.baseUrl}`).pipe(
              map(list => (list || []).filter(tc => tc.courseId === courseId)),
              catchError(() => of([]))
            );
          }
          return of([]);
        })
      );
  }

  getById(id: number): Observable<TeacherCourse> {
    return this.http.get<TeacherCourse>(`${this.baseUrl}/${id}`);
  }
}
