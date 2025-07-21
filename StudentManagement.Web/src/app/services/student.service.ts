import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Student } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang.
 */
export interface PagedStudentResponse {
  students: Student[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private apiUrl = `${environment.apiUrl}/Students`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả sinh viên */
  getAllStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.apiUrl);
  }

  /** Lấy sinh viên theo ID */
  getStudentById(id: string): Observable<Student> {
    return this.http.get<Student>(`${this.apiUrl}/${id}`);
  }

  /** Tạo sinh viên mới */
  createStudent(student: Omit<Student, 'studentId' | 'class' | 'users' | 'enrollments'>): Observable<Student> {
    return this.http.post<Student>(this.apiUrl, student);
  }

  /** Cập nhật thông tin sinh viên */
  updateStudent(id: string, studentData: Partial<Student>): Observable<Student> {
    return this.http.put<Student>(`${this.apiUrl}/${id}`, studentData);
  }

  /** Xóa sinh viên */
  deleteStudent(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy danh sách sinh viên theo lớp */
  getStudentsByClass(classId: string): Observable<Student[]> {
    return this.http.get<Student[]>(`${this.apiUrl}/class/${classId}`);
  }

  /** Tìm kiếm sinh viên theo tên hoặc mã số */
  searchStudents(searchTerm: string): Observable<Student[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Student[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách sinh viên được phân trang */
  getPagedStudents(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedStudentResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedStudentResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Kiểm tra email đã tồn tại hay chưa */
  checkEmailExists(email: string, excludeStudentId?: string): Observable<boolean> {
    let params = new HttpParams().set('email', email);
    if (excludeStudentId) {
      params = params.set('excludeStudentId', excludeStudentId);
    }
    return this.http.get<boolean>(`${this.apiUrl}/check-email`, { params });
  }

  /** Kiểm tra mã sinh viên đã tồn tại hay chưa */
  checkStudentIdExists(studentId: string): Observable<boolean> {
    const params = new HttpParams().set('studentId', studentId);
    return this.http.get<boolean>(`${this.apiUrl}/check-studentid`, { params });
  }
}
