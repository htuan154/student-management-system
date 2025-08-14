import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Class } from '../models';

export interface PagedClassResponse {
  classes: Class[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ClassCreateUpdateDto {
  classId: string;
  className: string;
  major: string;
  semesterId: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class ClassService {
  private apiUrl = `${environment.apiUrl}/Classes`;

  constructor(private http: HttpClient) {}

  getAllClasses(): Observable<Class[]> {
    return this.http.get<Class[]>(this.apiUrl);
  }

  getClassById(id: string): Observable<Class> {
    return this.http.get<Class>(`${this.apiUrl}/${id}`);
  }


  createClass(dto: ClassCreateUpdateDto): Observable<void> {
    return this.http.post<void>(this.apiUrl, dto);
  }

  updateClass(id: string, dto: ClassCreateUpdateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  deleteClass(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
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
