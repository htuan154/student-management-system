import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Semester } from '../models';

export interface PagedSemesterResponse {
  data: Semester[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;


}
export interface StudentSemesterSummary {
  semesterId: number;
  semesterName: string;
  academicYear: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  courseCount: number;
  
}

@Injectable({
  providedIn: 'root'
})
export class SemesterService {
  private apiUrl = `${environment.apiUrl}/Semester`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả học kỳ */
  getAllSemesters(): Observable<Semester[]> {
    return this.http.get<Semester[]>(this.apiUrl);
  }

  /** Lấy học kỳ theo ID */
  getSemesterById(id: number): Observable<Semester> {
    return this.http.get<Semester>(`${this.apiUrl}/${id}`);
  }

  /** Lấy các học kỳ đang hoạt động */
  getActiveSemesters(): Observable<Semester[]> {
    return this.http.get<Semester[]>(`${this.apiUrl}/active`);
  }

  /** Lấy học kỳ theo năm học */
  getSemestersByAcademicYear(academicYear: string): Observable<Semester[]> {
    return this.http.get<Semester[]>(`${this.apiUrl}/academic-year/${academicYear}`);
  }

  /** Tạo học kỳ mới */
  createSemester(semesterData: Omit<Semester, 'semesterId' | 'classes' | 'teacherCourses' | 'enrollments'>): Observable<Semester> {
    return this.http.post<Semester>(this.apiUrl, semesterData);
  }

  /** Cập nhật học kỳ */
  updateSemester(id: number, semesterData: any): Observable<Semester> {
    return this.http.put<Semester>(`${this.apiUrl}/${id}`, semesterData);
  }

  /** Xóa học kỳ */
  deleteSemester(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Tìm kiếm học kỳ */
  searchSemesters(searchTerm: string): Observable<Semester[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<Semester[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy học kỳ phân trang */
  getPagedSemesters(pageNumber: number, pageSize: number, searchTerm?: string, isActive?: boolean): Observable<PagedSemesterResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (isActive != null) {
      params = params.set('isActive', isActive.toString());
    }

    return this.http.get<PagedSemesterResponse>(`${this.apiUrl}/paged`, { params });
  }
  getMySemesters(): Observable<StudentSemesterSummary[]> {

    return this.http.get<StudentSemesterSummary[]>(`${environment.apiUrl}/Schedule/my-semesters`);
  }

  
}
