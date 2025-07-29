import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { Enrollment } from '../../../models';
import { EnrollmentService } from '../../../services/enrollment.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-my-scores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-scores.component.html',
  styleUrls: ['./my-scores.component.scss']
})
export class MyScoresComponent implements OnInit {
  enrollmentsWithScores: any[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  studentId: string | null = null;

  constructor(
    private enrollmentService: EnrollmentService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.studentId = tokenPayload?.studentId;

    if (this.studentId) {
      this.loadMyScores();
    } else {
      this.errorMessage = 'Không thể xác thực thông tin sinh viên.';
    }
  }

  loadMyScores(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.enrollmentService.getStudentEnrollmentsWithScores(this.studentId!).subscribe({
      next: (data) => {
        this.enrollmentsWithScores = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải kết quả học tập.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  refreshScores(): void {
    if (this.studentId) {
      this.loadMyScores();
    }
  }

  hasScores(): boolean {
    return this.enrollmentsWithScores.length > 0;
  }

  getAverageScore(): number {
    if (!this.hasScores()) return 0;

    let totalScoreTimesCredits = 0;
    let totalCredits = 0;

    for (const e of this.enrollmentsWithScores) {
      const score = e.totalScore;
      const credits = e.credits;

      if (e.status === 'Completed' && typeof score === 'number' && typeof credits === 'number') {
        totalScoreTimesCredits += score * credits;
        totalCredits += credits;
      }
    }

    if (totalCredits === 0) return 0;

    return Math.round((totalScoreTimesCredits / totalCredits) * 100) / 100;
  }

  getCompletedSubjectsCount(): number {
    return this.enrollmentsWithScores
      .filter(e => e.status === 'Completed' && typeof e.totalScore === 'number' && e.totalScore > 0).length;
  }

  getCompletedCredits(): number {
    return this.enrollmentsWithScores
      .filter(e => e.status === 'Completed' && typeof e.credits === 'number')
      .reduce((sum, e) => sum + e.credits, 0);
  }
  getTotalCompletedCredits(): number {
    return this.enrollmentsWithScores
      .filter(e => e.status === 'Completed' && typeof e.credits === 'number')
      .reduce((sum, e) => sum + e.credits, 0);
  }
  getScoreClass(score: number): string {
    if (score >= 9.0) return 'excellent';
    if (score >= 8.0) return 'good';
    if (score >= 6.5) return 'average';
    if (score >= 5.0) return 'below-average';
    return 'poor';
  }

  getScoreClassification(score: number): string {
    if (score >= 9.0) return 'Xuất sắc';
    if (score >= 8.0) return 'Giỏi';
    if (score >= 6.5) return 'Khá';
    if (score >= 5.0) return 'Trung bình';
    if (score >= 4.0) return 'Yếu';
    return 'Kém';
  }

  getGradeBadgeClass(score: number): string {
    if (score >= 9.0) return 'excellent';
    if (score >= 8.0) return 'good';
    if (score >= 6.5) return 'average';
    if (score >= 5.0) return 'below-average';
    return 'poor';
  }

  getGradeClassification(): string {
    const average = this.getAverageScore();
    if (average >= 9.0) return 'Xuất sắc';
    if (average >= 8.0) return 'Giỏi';
    if (average >= 6.5) return 'Khá';
    if (average >= 5.0) return 'Trung bình';
    if (average >= 4.0) return 'Yếu';
    return 'Kém';
  }
}
