import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Role } from '../../../../models';
import { RoleService } from '../../../../services/role.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; // ⬅️ thêm

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {
  allRoles: Role[] = [];   // toàn bộ dữ liệu
  roles: Role[] = [];      // dữ liệu trang hiện tại

  // phân trang
  pageSize = 10;
  currentPage = 1;

  isLoading = false;
  errorMessage: string | null = null;

  constructor(private roleService: RoleService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  get totalCount(): number {
    return this.allRoles.length;
  }
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.roles = this.allRoles.slice(start, end);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }
  prev(): void { this.goToPage(this.currentPage - 1); }
  next(): void { this.goToPage(this.currentPage + 1); }
  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.applyPagination();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.roleService.getAllRoles().subscribe({
      next: (data) => {
        this.allRoles = data;
        this.currentPage = 1;
        this.applyPagination();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of roles.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  trackById = (_: number, r: Role) => r.roleId;

  deleteRole(id: string): void {
    if (confirm('Are you sure you want to delete this role?')) {
      this.roleService.deleteRole(id).subscribe({
        next: () => {
          this.allRoles = this.allRoles.filter(r => r.roleId !== id);
          const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          if (this.currentPage > maxPage) this.currentPage = maxPage;
          this.applyPagination();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the role.';
          console.error(err);
        }
      });
    }
  }
}
