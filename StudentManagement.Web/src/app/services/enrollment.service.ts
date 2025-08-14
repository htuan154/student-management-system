import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Enrollment } from '../models/enrollment.model';

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


  getEnrollmentById(id: number): Observable<Enrollment> {
    return this.http.get<Enrollment>(`${this.apiUrl}/${id}`);
  }

  getEnrollmentsByStudentId(studentId: string): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/student/${studentId}`);
  }

  getEnrollmentsByCourseId(courseId: string): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/by-course/${encodeURIComponent(courseId)}`);
  }

  getEnrollmentsByTeacherCourseId(teacherCourseId: number): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/teacher-course/${teacherCourseId}`);
  }


  getEnrollmentsBySemesterId(semesterId: number): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/semester/${semesterId}`);
  }


  searchEnrollments(term: string): Observable<Enrollment[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Enrollment[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách đăng ký được phân trang */
  getPagedEnrollments(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedEnrollmentResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedEnrollmentResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Tạo một lượt đăng ký mới */
  createEnrollment(enrollmentData: any): Observable<any> {
    return this.http.post(this.apiUrl, enrollmentData);
  }

  /** Cập nhật một lượt đăng ký */
  updateEnrollment(id: number, enrollmentData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, enrollmentData);
  }

  /** Xóa một lượt đăng ký */
  deleteEnrollment(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy danh sách các lượt đăng ký chưa được chấm điểm */
  getUnscoredEnrollments(): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/unscored`);
  }

  /** ✅ SỬA - Lấy danh sách đăng ký chưa có điểm cho một lớp học cụ thể */
  getUnscoredEnrollmentsForClass(teacherCourseId: number): Observable<Enrollment[]> {
    // Sử dụng endpoint có sẵn: /api/Enrollment/unscored-by-class?teacherCourseId=xxx
    return this.http.get<Enrollment[]>(`${this.apiUrl}/unscored-by-class?teacherCourseId=${teacherCourseId}`).pipe(
      catchError(error => {
        console.error('Error getting unscored enrollments:', error);
        return of([] as Enrollment[]);
      })
    );
  }

  /** Tìm TeacherCourse từ teacherId + courseId (sử dụng TeacherCourseService) */
  getTeacherCourseByIds(teacherId: string, courseId: string): Observable<any> {
    // Sử dụng TeacherCourseController endpoint có sẵn
    return this.http.get<any[]>(`/api/TeacherCourse/teacher/${teacherId}`).pipe(
      map(teacherCourses =>
        teacherCourses.find(tc => tc.courseId === courseId)
      ),
      catchError(error => {
        console.error('Error finding teacher course:', error);
        return of(null);
      })
    );
  }

  /** Lấy enrollments theo teacherCourseId - SỬ DỤNG ENDPOINT CÓ SẴN */
  getEnrollmentsByTeacherCourse(teacherCourseId: number): Observable<Enrollment[]> {
    // Sử dụng endpoint có sẵn: /api/Enrollment/teacher-course/{teacherCourseId}
    return this.http.get<Enrollment[]>(`${this.apiUrl}/teacher-course/${teacherCourseId}`).pipe(
      catchError(error => {
        console.error('Error getting enrollments by teacher course:', error);
        return of([] as Enrollment[]);
      })
    );
  }

  /** Lấy danh sách đăng ký của sinh viên kèm điểm số */
  getStudentEnrollmentsWithScores(studentId: string): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(`${this.apiUrl}/student/${studentId}/with-scores`);
  }
}
