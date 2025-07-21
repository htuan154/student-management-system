import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Course } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang khóa học.
 */
export interface PagedCourseResponse {
  courses: Course[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl = `${environment.apiUrl}/Course`; // Chú ý: Tên controller là 'Course'

  constructor(private http: HttpClient) { }

  /** Lấy tất cả khóa học */
  getAllCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(this.apiUrl);
  }

  /** Lấy khóa học theo ID */
  getCourseById(id: string): Observable<Course> {
    return this.http.get<Course>(`${this.apiUrl}/${id}`);
  }

  /** Tạo khóa học mới */
  createCourse(course: Omit<Course, 'teacherCourses' | 'enrollments'>): Observable<any> {
    return this.http.post(this.apiUrl, course);
  }

  /** Cập nhật thông tin khóa học */
  updateCourse(id: string, courseData: Partial<Course>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, courseData);
  }

  /** Xóa khóa học */
  deleteCourse(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Tìm kiếm khóa học */
  searchCourses(term: string): Observable<Course[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Course[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách khóa học được phân trang */
  getPagedCourses(pageNumber: number, pageSize: number, searchTerm?: string, isActive?: boolean): Observable<PagedCourseResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (isActive != null) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PagedCourseResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Lấy danh sách khóa học theo khoa */
  getCoursesByDepartment(department: string): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/department/${department}`);
  }

  /** Lấy số lượng sinh viên đã đăng ký trong một khóa học */
  getEnrollmentCount(courseId: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${courseId}/enrollment-count`);
  }
}
