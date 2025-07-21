import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Role } from '../../../models';
import { RoleService } from '../../../services/role.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-role-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './role-management.component.html',
  styleUrls: ['./role-management.component.scss']
})
export class RoleManagementComponent implements OnInit {
  roles: Role[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  formRole: Partial<Role> = {
    roleName: '',
    description: ''
  };
  editingId: string | null = null;

  constructor(private roleService: RoleService) {}

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

  saveRole(): void {
    if (!this.formRole.roleName || this.formRole.roleName.trim() === '') {
      alert('Tên vai trò không được để trống.');
      return;
    }

    if (this.editingId) {
      this.roleService.updateRole(this.editingId, this.formRole).subscribe({
        next: () => {
          alert('Cập nhật vai trò thành công!');
          this.cancelEdit();
          this.loadRoles();
        },
        error: (err) => {
          console.error(err);
          alert('Cập nhật thất bại.');
        }
      });
    } else {
      this.roleService.createRole(this.formRole as any).subscribe({
        next: () => {
          alert('Tạo vai trò mới thành công!');
          this.formRole = {};
          this.loadRoles();
        },
        error: (err) => {
          console.error(err);
          alert('Tạo mới thất bại.');
        }
      });
    }
  }

  editRole(id: string): void {
    const role = this.roles.find(r => r.roleId === id);
    if (role) {
      this.editingId = id;
      this.formRole = {
        roleName: role.roleName,
        description: role.description
      };
    }
  }

  cancelEdit(): void {
    this.editingId = null;
    this.formRole = {};
  }

  deleteRole(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa vai trò này không?')) {
      this.roleService.deleteRole(id).subscribe({
        next: () => {
          alert('Xóa thành công!');
          this.loadRoles();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa vai trò.';
          console.error(err);
        }
      });
    }
  }
}
