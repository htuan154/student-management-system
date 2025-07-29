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

  // Thống kê
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

    this.enrollmentService.getStudentEnrollmentsWithScores(this.studentId!).subscribe({
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

  private processEnrollmentData(enrollments: any[]): void {
    console.log('📦 Dữ liệu enrollments:', enrollments);

    this.totalCourses = enrollments.length;
    this.completedCourses = enrollments.filter(e => e.status === 'Completed').length;

    let totalScoreTimesCredits = 0;
    let totalCredits = 0;

    for (const e of enrollments) {
      const score = e.totalScore;
      const credits = e.credits;

      if (e.status === 'Completed') {
        if (typeof score !== 'number') {
          console.warn(`⚠️ Môn ${e.courseId} không có điểm hợp lệ:`, score);
        }

        if (typeof credits !== 'number') {
          console.warn(`⚠️ Môn ${e.courseId} không có số tín chỉ hợp lệ:`, credits);
        }

        if (typeof score === 'number' && typeof credits === 'number') {
          totalScoreTimesCredits += score * credits;
          totalCredits += credits;
          console.log(`✅ Tính điểm: ${score} * ${credits} = ${score * credits}`);
        }
      }
    }

    if (totalCredits > 0) {
      this.averageScore = +(totalScoreTimesCredits / totalCredits).toFixed(2);
      console.log('✅ Tổng điểm * tín chỉ:', totalScoreTimesCredits);
      console.log('✅ Tổng tín chỉ:', totalCredits);
      console.log('✅ Điểm trung bình:', this.averageScore);
    } else {
      this.averageScore = 'Chưa có';
      console.warn('⚠️ Không có môn nào đủ điều kiện tính điểm trung bình.');
    }

    this.recentEnrollments = enrollments
      .sort((a, b) => (b.year ?? 0) - (a.year ?? 0))
      .slice(0, 5);
  }




}
