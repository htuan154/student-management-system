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

  users: User[] = [];
  isLoading = false;

  constructor(private userService: UserService) { }

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
  /**
   * Chỉnh sửa thông tin người dùng.
   * @param id ID của người dùng cần chỉnh sửa.
   */
  editUser(id: string): void {
    console.log(`Chỉnh sửa người dùng với ID: ${id}`);

  }
  /**
   * Xóa người dùng sau khi xác nhận.
   * @param id ID của người dùng cần xóa.
   */
  deleteUser(id: string): void {
    console.log(`Xóa người dùng với ID: ${id}`);

  }
}
