import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Student } from '../../../../models';
import { StudentService } from '../../../../services/student.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss'],
})
export class StudentListComponent implements OnInit {
  // Dữ liệu đầy đủ và dữ liệu theo trang
  allStudents: Student[] = [];
  students: Student[] = [];

  // Phân trang (client-side)
  pageSize = 10;
  currentPage = 1;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private studentService: StudentService) {}

  ngOnInit(): void {
    this.loadStudents();
  }

  get totalCount(): number {
    return this.allStudents.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get pages(): number[] {
    // Tạo mảng [1..totalPages] cho *ngFor
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.studentService.getAllStudents().subscribe({
      next: (data) => {
        this.allStudents = data || [];
        // Reset về trang 1 mỗi khi reload
        this.currentPage = 1;
        this.applyPagination();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of students.';
        this.isLoading = false;
        console.error(err);
      },
    });
  }

  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.students = this.allStudents.slice(start, end);
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

  changePageSize(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.applyPagination();
  }

  deleteStudent(id: string): void {
    if (confirm('Are you sure you want to delete this student?')) {
      this.studentService.deleteStudent(id).subscribe({
        next: () => {
          // Xoá xong: cập nhật mảng allStudents tại client để không cần gọi lại server (tuỳ ý)
          this.allStudents = this.allStudents.filter(s => s.studentId !== id);
          // Điều chỉnh currentPage nếu trang hiện tại rỗng
          const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          if (this.currentPage > maxPage) this.currentPage = maxPage;
          this.applyPagination();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the student.';
          console.error(err);
        },
      });
    }
  }

  trackById(_: number, s: Student) {
    return s.studentId;
  }
}
