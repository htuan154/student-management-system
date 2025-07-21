// src/app/pages/admin/user-management/user-management.component.ts

import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { User } from '../../../models';
import { UserService } from '../../../services/user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  // Biến để lưu danh sách người dùng từ API
  users: User[] = [];
  isLoading = false;

  // Tiêm UserService vào để sử dụng
  constructor(private userService: UserService) { }

  // ngOnInit là một lifecycle hook, được gọi một lần khi component được khởi tạo
  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (data: User[] ) => {
        this.users = data;
        this.isLoading = false;
        console.log('Tải danh sách người dùng thành công!', this.users);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Lỗi khi tải danh sách người dùng', err);
        this.isLoading = false;
      }
    });
  }

  // Placeholder cho các hàm trong tương lai
  editUser(id: string): void {
    console.log(`Chỉnh sửa người dùng với ID: ${id}`);
    // Logic để mở popup hoặc điều hướng đến trang chỉnh sửa
  }

  deleteUser(id: string): void {
    console.log(`Xóa người dùng với ID: ${id}`);
    // Logic để gọi API xóa và cập nhật lại danh sách
  }
}
