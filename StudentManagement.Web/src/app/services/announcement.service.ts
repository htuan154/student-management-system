import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Announcement } from '../models';

export interface PagedAnnouncementResponse {
  data: Announcement[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnnouncementService {
  private apiUrl = `${environment.apiUrl}/Announcement`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả thông báo */
  getAllAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(this.apiUrl);
  }

  /** Lấy thông báo theo ID */
  getAnnouncementById(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.apiUrl}/${id}`);
  }

  /** Lấy thông báo với chi tiết */
  getAnnouncementWithDetails(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.apiUrl}/${id}/with-details`);
  }

  /** Lấy thông báo theo user */
  getAnnouncementsByUser(userId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/user/${userId}`);
  }

  /** Lấy thông báo theo role */
  getAnnouncementsByRole(roleId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/role/${roleId}`);
  }

  /** Lấy thông báo theo class */
  getAnnouncementsByClass(classId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/class/${classId}`);
  }

  /** Lấy thông báo theo course */
  getAnnouncementsByCourse(courseId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /** Lấy thông báo đang hoạt động */
  getActiveAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/active`);
  }

  /** Lấy thông báo đã hết hạn */
  getExpiredAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/expired`);
  }

  /** Tạo thông báo mới */
  createAnnouncement(announcementData: Omit<Announcement, 'announcementId' | 'user' | 'announcementDetails' | 'createdAt'>): Observable<Announcement> {
    return this.http.post<Announcement>(this.apiUrl, announcementData);
  }

  /** Cập nhật thông báo */
  updateAnnouncement(id: number, announcementData: any): Observable<Announcement> {
    return this.http.put<Announcement>(`${this.apiUrl}/${id}`, announcementData);
  }

  /** Xóa thông báo */
  deleteAnnouncement(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy thông báo phân trang */
  getPagedAnnouncements(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedAnnouncementResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedAnnouncementResponse>(`${this.apiUrl}/paged`, { params });
  }
}