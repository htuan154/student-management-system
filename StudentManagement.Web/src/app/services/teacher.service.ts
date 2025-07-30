import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Teacher, Course } from '../models';

/**
 * Interface cho dữ liệu chi tiết của giáo viên (bao gồm các khóa học, v.v.)
 */
export interface TeacherDetail extends Teacher {
  coursesTaught: Course[];
  // Thêm các thuộc tính chi tiết khác nếu có
}

/**
 * Interface cho dữ liệu trả về khi phân trang giáo viên.
 */
export interface PagedTeacherResponse {
  teachers: Teacher[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private apiUrl = `${environment.apiUrl}/Teachers`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả giáo viên */
  getAllTeachers(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(this.apiUrl);
  }

  /** Lấy giáo viên theo ID */
  getTeacherById(id: string): Observable<Teacher> {
    return this.http.get<Teacher>(`${this.apiUrl}/${id}`);
  }

  /** Lấy thông tin chi tiết của giáo viên */
  getTeacherDetails(id: string): Observable<TeacherDetail> {
    return this.http.get<TeacherDetail>(`${this.apiUrl}/${id}/details`);
  }

  /** Tạo giáo viên mới */
  createTeacher(teacher: Omit<Teacher, 'teacherId' | 'users' | 'teacherCourses' | 'enrollments'>): Observable<Teacher> {
    return this.http.post<Teacher>(this.apiUrl, teacher);
  }

  /** Cập nhật thông tin giáo viên */
  updateTeacher(id: string, teacherData: Partial<Teacher>): Observable<Teacher> {
    return this.http.put<Teacher>(`${this.apiUrl}/${id}`, teacherData);
  }

  /** Xóa giáo viên */
  deleteTeacher(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy danh sách giáo viên theo khoa */
  getTeachersByDepartment(department: string): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(`${this.apiUrl}/department/${department}`);
  }

  /** Lấy danh sách giáo viên theo bằng cấp */
  getTeachersByDegree(degree: string): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(`${this.apiUrl}/degree/${degree}`);
  }

  /** Lấy danh sách giáo viên đang có khóa học hoạt động */
  getTeachersWithActiveCourses(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(`${this.apiUrl}/with-courses`);
  }

  /** Lấy danh sách giáo viên theo khoảng lương */
  getTeachersBySalaryRange(minSalary?: number, maxSalary?: number): Observable<Teacher[]> {
    let params = new HttpParams();
    if (minSalary != null) {
      params = params.set('minSalary', minSalary.toString());
    }
    if (maxSalary != null) {
      params = params.set('maxSalary', maxSalary.toString());
    }
    return this.http.get<Teacher[]>(`${this.apiUrl}/salary-range`, { params });
  }

  /** Tìm kiếm giáo viên */
  searchTeachers(searchTerm: string): Observable<Teacher[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Teacher[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách giáo viên được phân trang */
  getPagedTeachers(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedTeacherResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<PagedTeacherResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Lấy danh sách các khoa duy nhất */
  getDistinctDepartments(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/departments`);
  }

  /** Lấy danh sách các bằng cấp duy nhất */
  getDistinctDegrees(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/degrees`);
  }

  /** Kiểm tra email đã tồn tại hay chưa */
  checkEmailExists(email: string, excludeTeacherId?: string): Observable<boolean> {
    let params = new HttpParams().set('email', email);
    if (excludeTeacherId) {
      params = params.set('excludeTeacherId', excludeTeacherId);
    }
    return this.http.get<boolean>(`${this.apiUrl}/check-email`, { params });
  }

  /** Kiểm tra mã giáo viên đã tồn tại hay chưa */
  checkTeacherIdExists(teacherId: string): Observable<boolean> {
    const params = new HttpParams().set('teacherId', teacherId);
    return this.http.get<boolean>(`${this.apiUrl}/check-teacherid`, { params });
  }
  /** Lấy danh sách giáo viên theo môn học */
  getTeachersByCourseId(courseId: string): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(`${this.apiUrl}/course/${courseId}`);
  }
}
