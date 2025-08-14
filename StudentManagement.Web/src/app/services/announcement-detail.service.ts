import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AnnouncementDetail } from '../models';

export interface PagedAnnouncementDetailResponse {
  data: AnnouncementDetail[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnnouncementDetailService {
  private apiUrl = `${environment.apiUrl}/AnnouncementDetail`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả chi tiết thông báo */
  getAllAnnouncementDetails(): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(this.apiUrl);
  }

  /** Lấy chi tiết thông báo theo ID */
  getAnnouncementDetailById(id: number): Observable<AnnouncementDetail> {
    return this.http.get<AnnouncementDetail>(`${this.apiUrl}/${id}`);
  }

  /** Lấy chi tiết theo announcement ID */
  getDetailsByAnnouncementId(announcementId: number): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/announcement/${announcementId}`);
  }

  /** Lấy chi tiết theo role ID */
  getDetailsByRoleId(roleId: string): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/role/${roleId}`);
  }

  /** Lấy chi tiết theo class ID */
  getDetailsByClassId(classId: string): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/class/${classId}`);
  }

  /** Lấy chi tiết theo course ID */
  getDetailsByCourseId(courseId: string): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /** Lấy chi tiết theo user ID */
  getDetailsByUserId(userId: string): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/user/${userId}`);
  }

  /** Tạo chi tiết thông báo mới */
  createAnnouncementDetail(detailData: Omit<AnnouncementDetail, 'announcementDetailId' | 'announcement' | 'role' | 'class' | 'course' | 'user'>): Observable<AnnouncementDetail> {
    return this.http.post<AnnouncementDetail>(this.apiUrl, detailData);
  }

  /** Cập nhật chi tiết thông báo */
  updateAnnouncementDetail(id: number, detailData: any): Observable<AnnouncementDetail> {
    return this.http.put<AnnouncementDetail>(`${this.apiUrl}/${id}`, detailData);
  }

  /** Xóa chi tiết thông báo */
  deleteAnnouncementDetail(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Xóa chi tiết theo announcement ID */
  deleteDetailsByAnnouncementId(announcementId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/announcement/${announcementId}`);
  }

  /** Lấy chi tiết phân trang */
  getPagedAnnouncementDetails(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedAnnouncementDetailResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedAnnouncementDetailResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Tạo chi tiết hàng loạt */
  bulkCreateAnnouncementDetails(announcementId: number, bulkData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/bulk-create/${announcementId}`, bulkData);
  }

  /** Lấy chi tiết thông báo của user */
  getUserAnnouncementDetails(userId: string): Observable<AnnouncementDetail[]> {
    return this.http.get<AnnouncementDetail[]>(`${this.apiUrl}/user-announcement-details/${userId}`);
  }
}