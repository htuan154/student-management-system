import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Employee } from '../models';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private apiUrl = `${environment.apiUrl}/Employees`; // Giả sử endpoint là /api/Employees

  constructor(private http: HttpClient) { }

  /**
   * Lấy tất cả nhân viên từ API.
   */
  getAllEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.apiUrl);
  }

  /**
   * Lấy thông tin của một nhân viên dựa trên ID.
   * @param id ID của nhân viên cần lấy.
   */
  getEmployeeById(id: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  /**
   * Tạo một nhân viên mới.
   * @param employee Đối tượng Employee chứa thông tin cần tạo.
   */
  createEmployee(employee: Omit<Employee, 'employeeId' | 'users'>): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, employee);
  }

  /**
   * Cập nhật thông tin của một nhân viên.
   * @param id ID của nhân viên cần cập nhật.
   * @param employeeData Đối tượng Employee chứa thông tin mới.
   */
  updateEmployee(id: string, employeeData: Partial<Employee>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, employeeData);
  }

  /**
   * Xóa một nhân viên.
   * @param id ID của nhân viên cần xóa.
   */
  deleteEmployee(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
