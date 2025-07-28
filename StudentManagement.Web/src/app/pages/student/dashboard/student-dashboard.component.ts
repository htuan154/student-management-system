import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Enrollment } from '../../../models';
import { EnrollmentService } from '../../../services/enrollment.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.scss']
})
export class StudentDashboardComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;
  studentId: string | null = null;

  // Dữ liệu thống kê
  totalCourses = 0;
  completedCourses = 0;
  averageScore: number | string = 'Chưa có';
  recentEnrollments: Enrollment[] = [];

  constructor(
    private enrollmentService: EnrollmentService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.studentId = tokenPayload?.studentId;

    if (this.studentId) {
      this.loadDashboardData();
    } else {
      this.errorMessage = 'Không thể xác thực thông tin sinh viên.';
    }
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.enrollmentService.getEnrollmentsByStudentId(this.studentId!).subscribe({
      next: (enrollments) => {
        this.processEnrollmentData(enrollments);
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải dữ liệu.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  /**
   * Xử lý dữ liệu đăng ký thô để tính toán các số liệu thống kê.
   */
  private processEnrollmentData(enrollments: Enrollment[]): void {
    if (!enrollments) return;

    this.totalCourses = enrollments.length;

    // ✅ SỬA LỖI: Đếm số môn dựa trên trạng thái "Completed"
    this.completedCourses = enrollments.filter(e => e.status === 'Completed').length;

    // Tính điểm trung bình
    const enrollmentsWithScores = enrollments.filter(e => e.score && e.score.totalScore !== null);
    if (enrollmentsWithScores.length > 0) {
      const totalScoreSum = enrollmentsWithScores.reduce((sum, e) => sum + (e.score?.totalScore ?? 0), 0);
      this.averageScore = +(totalScoreSum / enrollmentsWithScores.length).toFixed(2);
    } else {
      this.averageScore = 'Chưa có';
    }

    // Lấy 5 hoạt động gần nhất để hiển thị
    this.recentEnrollments = enrollments
      .sort((a, b) => (b.year ?? 0) - (a.year ?? 0))
      .slice(0, 5);
  }
}
