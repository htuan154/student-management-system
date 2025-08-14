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

export interface CreateAnnouncementDto {
  title: string;
  content: string;
  expiryDate?: string | null;
  roleIds?: string[];
  classIds?: string[];
  courseIds?: string[];
  userIds?: string[];
}

// Có thể dùng chung cho update (nếu BE cho phép partial)
export type UpdateAnnouncementDto = CreateAnnouncementDto;

@Injectable({ providedIn: 'root' })
export class AnnouncementService {
  private apiUrl = `${environment.apiUrl}/Announcement`;

  constructor(private http: HttpClient) {}

  getAllAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(this.apiUrl);
  }

  getAnnouncementById(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.apiUrl}/${id}`);
  }

  getAnnouncementWithDetails(id: number): Observable<Announcement> {
    return this.http.get<Announcement>(`${this.apiUrl}/${id}/with-details`);
  }

  getAnnouncementsByUser(userId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/user/${userId}`);
  }

  getAnnouncementsByRole(roleId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/role/${roleId}`);
  }

  getAnnouncementsByClass(classId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/class/${classId}`);
  }

  getAnnouncementsByCourse(courseId: string): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/course/${courseId}`);
  }

  getActiveAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/active`);
  }

  getExpiredAnnouncements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${this.apiUrl}/expired`);
  }

  // ✅ CHỈ GIỮ 1 HÀM createAnnouncement
  createAnnouncement(dto: CreateAnnouncementDto): Observable<Announcement> {
    return this.http.post<Announcement>(this.apiUrl, dto);
  }

  updateAnnouncement(id: number, dto: UpdateAnnouncementDto): Observable<Announcement> {
    return this.http.put<Announcement>(`${this.apiUrl}/${id}`, dto);
  }

  deleteAnnouncement(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getPagedAnnouncements(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedAnnouncementResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get<PagedAnnouncementResponse>(`${this.apiUrl}/paged`, { params });
  }
}
