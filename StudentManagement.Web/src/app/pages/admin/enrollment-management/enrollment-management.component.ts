import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Enrollment } from '../../../models';
import { EnrollmentService, PagedEnrollmentResponse } from '../../../services/enrollment.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-enrollment-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './enrollment-management.component.html',
  styleUrls: ['./enrollment-management.component.scss']
})
export class EnrollmentManagementComponent implements OnInit {
  enrollments: Enrollment[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // Tìm kiếm
  searchTerm: string = '';

  // Tạo/sửa
  editingId: number | null = null;
  formData: Partial<Enrollment> = {
    studentId: '',
    courseId: ''
  };

  constructor(private enrollmentService: EnrollmentService) {}

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.enrollmentService.getPagedEnrollments(this.currentPage, this.pageSize, this.searchTerm).subscribe({
      next: (response: PagedEnrollmentResponse) => {
        this.enrollments = response.enrollments;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách đăng ký.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  saveEnrollment(): void {
    if (!this.formData.studentId || !this.formData.courseId) {
      alert('Vui lòng nhập đầy đủ thông tin.');
      return;
    }

    if (this.editingId) {
      this.enrollmentService.updateEnrollment(this.editingId, this.formData).subscribe({
        next: () => {
          alert('Cập nhật thành công!');
          this.cancelEdit();
          this.loadEnrollments();
        },
        error: (err) => {
          alert('Lỗi khi cập nhật.');
          console.error(err);
        }
      });
    } else {
      this.enrollmentService.createEnrollment(this.formData as any).subscribe({
        next: () => {
          alert('Tạo mới thành công!');
          this.formData = {};
          this.loadEnrollments();
        },
        error: (err) => {
          alert('Lỗi khi tạo mới.');
          console.error(err);
        }
      });
    }
  }

  editEnrollment(id: number): void {
    const found = this.enrollments.find(e => e.enrollmentId === id);
    if (found) {
      this.editingId = id;
      this.formData = {
        studentId: found.studentId,
        courseId: found.courseId
      };
    }
  }

  cancelEdit(): void {
    this.editingId = null;
    this.formData = {};
  }

  deleteEnrollment(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa lượt đăng ký này không?')) {
      this.enrollmentService.deleteEnrollment(id).subscribe({
        next: () => {
          this.loadEnrollments();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa lượt đăng ký.';
          console.error(err);
        }
      });
    }
  }

  search(): void {
    this.currentPage = 1;
    this.loadEnrollments();
  }

  pageChanged(page: number): void {
    this.currentPage = page;
    this.loadEnrollments();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }
}
