import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models';
import { UserUpdateDto } from '../models/dtos/user-update.dto';
import { UserCreateDto } from '../models/dtos/user-create.dto';

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
   * @param userDto Đối tượng UserCreateDto chứa thông tin cần tạo.
   * @returns Một Observable chứa đối tượng User đã được tạo.
   */
  // SỬA LỖI: Bỏ dấu chấm, đúng phải là "UserCreateDto"
  createUser(userDto: UserCreateDto): Observable<User> {
    return this.http.post<User>(this.apiUrl, userDto);
  }

  /**
   * Cập nhật thông tin của một người dùng.
   * @param id ID của người dùng cần cập nhật.
   * @param userDto Đối tượng UserUpdateDto chứa thông tin mới.
   * @returns Một Observable không chứa dữ liệu (hoặc chứa đối tượng đã cập nhật).
   */
  updateUser(id: string, userDto: UserUpdateDto): Observable<any> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.put(url, userDto);
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
  changePassword(payload: { userId: string; currentPassword: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, payload);
  }

}
