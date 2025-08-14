import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { User } from '../../../../models';
import { UserService } from '../../../../services/user.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  // Data properties
  users: User[] = [];
  filteredUsers: User[] = [];
  selectedUsers: string[] = [];

  // State properties
  isLoading = false;
  errorMessage: string | null = null;

  // Filter properties
  searchTerm = '';
  selectedRole = '';
  selectedStatus = '';

  // Pagination properties
  currentPage = 1;
  pageSize = 10;
  totalUsers = 0;
  totalPages = 0;

  // Role mapping
  private roleNames: { [key: string]: string } = {
    'R001': 'Admin',
    'R002': 'Teacher',
    'R003': 'Employee',
    'R004': 'Student'
  };

  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  // ✅ Load users data
  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.userService.getAllUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.totalUsers = data.length;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách người dùng.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  // ✅ Search functionality
  onSearch(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  // ✅ Filter functionality
  onFilterChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  // ✅ Apply filters and pagination
  private applyFilters(): void {
    let filtered = [...this.users];

    // Apply search filter
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(user =>
        user.username.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term) ||
        user.userId.toLowerCase().includes(term)
      );
    }

    // Apply role filter
    if (this.selectedRole) {
      filtered = filtered.filter(user => user.roleId === this.selectedRole);
    }

    // Apply status filter
    if (this.selectedStatus !== '') {
      const isActive = this.selectedStatus === 'true';
      filtered = filtered.filter(user => user.isActive === isActive);
    }

    this.totalUsers = filtered.length;
    this.totalPages = Math.ceil(this.totalUsers / this.pageSize);

    // Apply pagination
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.filteredUsers = filtered.slice(startIndex, endIndex);
  }

  // ✅ Reset filters
  resetFilters(): void {
    this.searchTerm = '';
    this.selectedRole = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.applyFilters();
  }

  // ✅ Selection functionality
  toggleSelectAll(event: any): void {
    if (event.target.checked) {
      this.selectedUsers = this.filteredUsers.map(user => user.userId);
    } else {
      this.selectedUsers = [];
    }
  }

  toggleUserSelection(userId: string, event: any): void {
    if (event.target.checked) {
      this.selectedUsers.push(userId);
    } else {
      this.selectedUsers = this.selectedUsers.filter(id => id !== userId);
    }
  }

  // ✅ Bulk actions
  bulkActivate(): void {
    if (confirm(`Bạn có chắc chắn muốn kích hoạt ${this.selectedUsers.length} người dùng?`)) {
      // TODO: Implement bulk activation
      console.log('Bulk activate:', this.selectedUsers);
      this.selectedUsers = [];
    }
  }

  bulkDeactivate(): void {
    if (confirm(`Bạn có chắc chắn muốn vô hiệu hóa ${this.selectedUsers.length} người dùng?`)) {
      // TODO: Implement bulk deactivation
      console.log('Bulk deactivate:', this.selectedUsers);
      this.selectedUsers = [];
    }
  }

  bulkDelete(): void {
    if (confirm(`Bạn có chắc chắn muốn xóa ${this.selectedUsers.length} người dùng? Hành động này không thể hoàn tác.`)) {
      // TODO: Implement bulk deletion
      console.log('Bulk delete:', this.selectedUsers);
      this.selectedUsers = [];
    }
  }

  // ✅ Individual actions
  viewUser(id: string): void {
    // TODO: Navigate to user detail view
    console.log('View user:', id);
  }

  toggleUserStatus(id: string): void {
    const user = this.users.find(u => u.userId === id);
    if (!user) return;

    const action = user.isActive ? 'vô hiệu hóa' : 'kích hoạt';
    if (confirm(`Bạn có chắc chắn muốn ${action} người dùng này?`)) {
      // TODO: Implement status toggle
      console.log('Toggle status:', id, !user.isActive);
    }
  }

  deleteUser(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này? Hành động này không thể hoàn tác.')) {
      this.userService.deleteUser(id).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Có lỗi xảy ra khi xóa người dùng.';
          console.error(err);
        }
      });
    }
  }

  // ✅ Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.applyFilters();
    }
  }

  getStartIndex(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getEndIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalUsers);
  }

  // ✅ Helper functions
  getRoleName(roleId: string): string {
    return this.roleNames[roleId] || roleId;
  }

  getActiveUsersCount(): number {
    return this.users.filter(user => user.isActive).length;
  }

  trackByUserId(index: number, user: User): string {
    return user.userId;
  }
}
