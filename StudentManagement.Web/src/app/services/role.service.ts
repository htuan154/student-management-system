import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Role } from '../models';

/**
 * Interface cho dữ liệu trả về khi phân trang vai trò.
 */
export interface PagedRoleResponse {
  roles: Role[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private apiUrl = `${environment.apiUrl}/Role`;

  constructor(private http: HttpClient) { }

  /** Lấy tất cả các vai trò */
  getAllRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl);
  }

  /** Lấy vai trò theo ID */
  getRoleById(id: string): Observable<Role> {
    return this.http.get<Role>(`${this.apiUrl}/${id}`);
  }

  /** Tạo một vai trò mới */
  createRole(roleData: Omit<Role, 'users'>): Observable<any> {
    return this.http.post(this.apiUrl, roleData);
  }

  /** Cập nhật thông tin vai trò */
  updateRole(id: string, roleData: Partial<Role>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, roleData);
  }

  /** Xóa một vai trò */
  deleteRole(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Tìm kiếm vai trò */
  searchRoles(term: string): Observable<Role[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Role[]>(`${this.apiUrl}/search`, { params });
  }

  /** Lấy danh sách vai trò được phân trang */
  getPagedRoles(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedRoleResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedRoleResponse>(`${this.apiUrl}/paged`, { params });
  }
}
