import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Enrollment } from '../../../models';
import { EnrollmentService, PagedEnrollmentResponse } from '../../../services/enrollment.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-enrollment-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './enrollment-management.component.html',
  styleUrls: ['./enrollment-management.component.scss']
})
export class EnrollmentManagementComponent implements OnInit {
  enrollments: Enrollment[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Thuộc tính cho phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  constructor(private enrollmentService: EnrollmentService) { }

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.enrollmentService.getPagedEnrollments(this.currentPage, this.pageSize).subscribe({
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

  editEnrollment(id: number): void {
    console.log('Chỉnh sửa đăng ký:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteEnrollment(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa lượt đăng ký này không?')) {
      this.enrollmentService.deleteEnrollment(id).subscribe({
        next: () => {
          this.loadEnrollments(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa lượt đăng ký.';
          console.error(err);
        }
      });
    }
  }
}
