import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Class } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang lớp học.
 */
export interface PagedClassResponse {
  classes: Class[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ClassService {
  private apiUrl = `${environment.apiUrl}/Classes`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả các lớp học */
  getAllClasses(): Observable<Class[]> {
    return this.http.get<Class[]>(this.apiUrl);
  }

  /** Lấy lớp học theo ID */
  getClassById(id: string): Observable<Class> {
    return this.http.get<Class>(`${this.apiUrl}/${id}`);
  }

  /** Lấy thông tin lớp học kèm danh sách sinh viên */
  getClassWithStudents(id: string): Observable<Class> {
    return this.http.get<Class>(`${this.apiUrl}/${id}/students`);
  }

  /** Tạo một lớp học mới */
  createClass(classData: Omit<Class, 'classId' | 'students'>): Observable<Class> {
    return this.http.post<Class>(this.apiUrl, classData);
  }

  /** Cập nhật thông tin lớp học */
  updateClass(id: string, classData: Partial<Class>): Observable<Class> {
    return this.http.put<Class>(`${this.apiUrl}/${id}`, classData);
  }

  /** Xóa một lớp học */
  deleteClass(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy các lớp học đang hoạt động */
  getActiveClasses(): Observable<Class[]> {
    return this.http.get<Class[]>(`${this.apiUrl}/active`);
  }

  /** Tìm kiếm lớp học */
  searchClasses(searchTerm: string): Observable<Class[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Class[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách lớp học được phân trang */
  getPagedClasses(pageNumber: number, pageSize: number, searchTerm?: string, isActive?: boolean): Observable<PagedClassResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (isActive != null) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PagedClassResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Lấy các lớp học theo chuyên ngành */
  getClassesByMajor(major: string): Observable<Class[]> {
    return this.http.get<Class[]>(`${this.apiUrl}/major/${major}`);
  }

  /** Lấy các lớp học theo năm học */
  getClassesByAcademicYear(academicYear: string): Observable<Class[]> {
    return this.http.get<Class[]>(`${this.apiUrl}/academic-year/${academicYear}`);
  }

  /** Kiểm tra mã lớp đã tồn tại hay chưa */
  checkClassIdExists(classId: string): Observable<boolean> {
    const params = new HttpParams().set('classId', classId);
    return this.http.get<boolean>(`${this.apiUrl}/check-classid`, { params });
  }

  /** Kiểm tra tên lớp đã tồn tại hay chưa */
  checkClassNameExists(className: string, excludeClassId?: string): Observable<boolean> {
    let params = new HttpParams().set('className', className);
    if (excludeClassId) {
      params = params.set('excludeClassId', excludeClassId);
    }
    return this.http.get<boolean>(`${this.apiUrl}/check-classname`, { params });
  }

  /** Kiểm tra xem một lớp có thể bị xóa hay không */
  canDeleteClass(id: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${id}/can-delete`);
  }
}
