import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Employee } from '../../../../models';
import { EmployeeService } from '../../../../services/employees.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss']
})
export class EmployeeListComponent implements OnInit {
  // dữ liệu đầy đủ + dữ liệu theo trang
  allEmployees: Employee[] = [];
  employees: Employee[] = [];

  // phân trang (client-side)
  pageSize = 10;
  currentPage = 1;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  // tổng số bản ghi
  get totalCount(): number {
    return this.allEmployees.length;
  }

  // tổng số trang
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  // mảng số trang để *ngFor
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  loadEmployees(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.employeeService.getAllEmployees().subscribe({
      next: (data) => {
        this.allEmployees = data || [];
        this.currentPage = 1;      // reset về trang 1 khi reload
        this.applyPagination();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of employees.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.employees = this.allEmployees.slice(start, end);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }

  prev(): void {
    this.goToPage(this.currentPage - 1);
  }

  next(): void {
    this.goToPage(this.currentPage + 1);
  }

  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.applyPagination();
  }

  deleteEmployee(id: string): void {
    if (confirm('Are you sure you want to delete this employee?')) {
      this.employeeService.deleteEmployee(id).subscribe({
        next: () => {
          // cập nhật mảng tại client để không cần reload toàn bộ
          this.allEmployees = this.allEmployees.filter(e => e.employeeId !== id);
          const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          if (this.currentPage > maxPage) this.currentPage = maxPage;
          this.applyPagination();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the employee.';
          console.error(err);
        }
      });
    }
  }

  trackById = (_: number, e: Employee) => e.employeeId;
}
