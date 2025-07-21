import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  // Lấy URL của API từ file môi trường
  private apiUrl = `${environment.apiUrl}/User`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy tất cả người dùng từ API.
   * @returns Một Observable chứa mảng các đối tượng User.
   */
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  /**
   * Lấy thông tin của một người dùng dựa trên ID.
   * @param id ID của người dùng cần lấy.
   * @returns Một Observable chứa đối tượng User.
   */
  getUserById(id: string): Observable<User> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.get<User>(url);
  }

  /**
   * Tạo một người dùng mới.
   * @param user Đối tượng User chứa thông tin cần tạo.
   * @returns Một Observable chứa đối tượng User đã được tạo.
   */
  createUser(user: Omit<User, 'userId'>): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }

  /**
   * Cập nhật thông tin của một người dùng.
   * @param id ID của người dùng cần cập nhật.
   * @param user Đối tượng User chứa thông tin mới.
   * @returns Một Observable không chứa dữ liệu (hoặc chứa đối tượng đã cập nhật).
   */
  updateUser(id: string, user: User): Observable<any> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.put(url, user);
  }

  /**
   * Xóa một người dùng.
   * @param id ID của người dùng cần xóa.
   * @returns Một Observable không chứa dữ liệu.
   */
  deleteUser(id: string): Observable<any> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.delete(url);
  }
}
