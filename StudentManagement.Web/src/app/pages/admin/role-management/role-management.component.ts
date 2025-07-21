import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Role } from '../../../models';
import { RoleService } from '../../../services/role.service';

@Component({
  selector: 'app-role-management',
  templateUrl: './role-management.component.html',
  styleUrls: ['./role-management.component.scss']
})
export class RoleManagementComponent implements OnInit {
  roles: Role[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private roleService: RoleService) { }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.roleService.getAllRoles().subscribe({
      next: (data) => {
        this.roles = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách vai trò.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editRole(id: string): void {
    console.log('Chỉnh sửa vai trò:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteRole(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa vai trò này không?')) {
      this.roleService.deleteRole(id).subscribe({
        next: () => {
          this.loadRoles(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa vai trò.';
          console.error(err);
        }
      });
    }
  }
}
