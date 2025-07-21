import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { User } from '../../../models';
import { UserService } from '../../../services/user.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {

  users: User[] = [];
  isLoading = false;

  // Biến dùng để bind dữ liệu form
  newUser: Partial<User> = {
    username: '',
    email: '',
    passwordHash: '',
    roleId: '',
    isActive: true
  };

  editingUserId: string | null = null;

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (data: User[]) => {
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

  // ======================= CREATE ==========================
  createUser(): void {
    if (!this.newUser.username || !this.newUser.email || !this.newUser.passwordHash || !this.newUser.roleId) {
      alert('Vui lòng điền đầy đủ thông tin người dùng');
      return;
    }

    this.userService.createUser(this.newUser as Omit<User, 'userId'>).subscribe({
      next: (createdUser) => {
        alert('Tạo người dùng thành công!');
        this.users.push(createdUser);
        this.newUser = {};
      },
      error: (err) => {
        console.error('Lỗi tạo người dùng', err);
        alert('Tạo người dùng thất bại.');
      }
    });
  }

  // ======================= UPDATE ==========================
  editUser(id: string): void {
    this.editingUserId = id;
    const user = this.users.find(u => u.userId === id);
    if (user) {
      this.newUser = { ...user }; // Sao chép dữ liệu sang form
    }
  }

  updateUser(): void {
    if (!this.editingUserId) return;

    this.userService.updateUser(this.editingUserId, this.newUser as User).subscribe({
      next: () => {
        alert('Cập nhật người dùng thành công!');
        this.loadUsers(); // Tải lại danh sách
        this.newUser = {};
        this.editingUserId = null;
      },
      error: (err) => {
        console.error('Lỗi cập nhật người dùng', err);
        alert('Cập nhật thất bại.');
      }
    });
  }

  cancelEdit(): void {
    this.editingUserId = null;
    this.newUser = {};
  }

  // ======================= DELETE ==========================
  deleteUser(id: string): void {
    if (confirm('Bạn có chắc muốn xóa người dùng này không?')) {
      this.userService.deleteUser(id).subscribe({
        next: () => {
          alert('Xóa người dùng thành công!');
          this.users = this.users.filter(u => u.userId !== id);
        },
        error: (err) => {
          console.error('Lỗi khi xóa người dùng', err);
          alert('Xóa thất bại.');
        }
      });
    }
  }
}
