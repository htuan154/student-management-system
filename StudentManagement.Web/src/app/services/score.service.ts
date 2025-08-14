import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators'; // ⬅ THÊM IMPORT
import { environment } from '../../environments/environment';
import { Score } from '../models';

export interface PagedScoreResponse {
  data: Score[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScoreService {
  private apiUrl = `${environment.apiUrl}/Score`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả điểm */
  getAllScores(): Observable<Score[]> {
    return this.http.get<Score[]>(this.apiUrl);
  }

  /** Lấy điểm theo ID */
  getScoreById(id: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/${id}`);
  }

  /** Lấy điểm theo Student ID */
  getScoresByStudentId(studentId: string): Observable<Score[]> {
    return this.http.get<Score[]>(`${this.apiUrl}/student/${studentId}`);
  }

  /** Lấy điểm theo Course ID */
  getScoresByCourseId(courseId: string): Observable<Score[]> {
    return this.http.get<Score[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /** Xóa method getScoresByTeacherCourseId, dùng method này thay thế */
  getScoresByTeacherAndCourse(teacherId: string, courseId: string): Observable<Score[]> {
    const params = new HttpParams()
      .set('teacherId', teacherId)
      .set('courseId', courseId);

    return this.http.get<Score[]>(`${this.apiUrl}/teacher-subject`, { params }).pipe(
      catchError((err) => {
        console.warn('API /teacher-subject failed, falling back to getAll + filter', err);
        // Fallback: lấy tất cả điểm rồi filter FE
        return this.getAllScores().pipe(
          map((allScores: Score[]) => {
            return (allScores || []).filter(score => {
              // Filter theo teacherId và courseId trong enrollment hoặc computed fields
              const matchTeacher = score.enrollment?.teacherCourse?.teacherId === teacherId;
              const matchCourse =
                                score.enrollment?.course?.courseId === courseId ||
                                score.enrollment?.teacherCourse?.courseId === courseId;
              return matchTeacher && matchCourse;
            });
          }),
          catchError(() => of([] as Score[])) // Nếu getAll cũng fail
        );
      })
    );
  }

  /** Lấy điểm theo Enrollment ID */
  getScoreByEnrollmentId(enrollmentId: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/enrollment/${enrollmentId}`);
  }

  /** Tạo điểm mới */
  createScore(scoreData: Omit<Score, 'scoreId' | 'student' | 'course' | 'teacherCourse' | 'enrollment'>): Observable<Score> {
    return this.http.post<Score>(this.apiUrl, scoreData);
  }

  /** Cập nhật điểm */
  updateScore(id: number, scoreData: any): Observable<Score> {
    return this.http.put<Score>(`${this.apiUrl}/${id}`, scoreData);
  }

  /** Xóa điểm */
  deleteScore(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Tìm kiếm điểm */
  searchScores(searchTerm: string): Observable<Score[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Score[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy điểm phân trang - ĐÂY LÀ METHOD THIẾU TRONG CONTROLLER */
  getPagedScores(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedScoreResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedScoreResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Nhập điểm hàng loạt */
  bulkCreateScores(scoresData: any[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/bulk-create`, scoresData);
  }

  /** Cập nhật điểm hàng loạt */
  bulkUpdateScores(scoresData: any[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/bulk-update`, scoresData);
  }

  /** Lấy điểm theo giảng viên và môn học */
  getByTeacherAndSubject(teacherId: string, courseId: string): Observable<Score[]> {
    console.log('Getting scores for teacherId:', teacherId, 'courseId:', courseId);

    const params = new HttpParams()
      .set('teacherId', teacherId)
      .set('courseId', courseId);

    // ✅ SỬA: Dùng endpoint đúng như backend
    return this.http.get<Score[]>(`${this.apiUrl}/teacher-subject`, { params }).pipe(
      catchError((err) => {
        console.error('API /teacher-subject failed:', err);
        // Fallback: lấy tất cả điểm rồi filter FE
        return this.getAllScores().pipe(
          map((allScores: Score[]) => {
            return (allScores || []).filter(score => {
              const matchTeacher = score.enrollment?.teacherCourse?.teacherId === teacherId;
              const matchCourse =
                score.enrollment?.course?.courseId === courseId ||
                score.enrollment?.teacherCourse?.courseId === courseId;
              return matchTeacher && matchCourse;
            });
          }),
          catchError(() => of([] as Score[]))
        );
      })
    );
  }

  /** Lấy điểm theo Teacher Course ID - SỬA ĐỔI */
  getScoresByTeacherCourseId(teacherCourseId: number): Observable<Score[]> {
    // Score không có teacherCourseId, cần filter qua enrollment
    return this.http.get<Score[]>(this.apiUrl).pipe(
      map((scores: Score[]) =>
        scores.filter(s => s.enrollment?.teacherCourseId === teacherCourseId)
      )
    );
  }
}
