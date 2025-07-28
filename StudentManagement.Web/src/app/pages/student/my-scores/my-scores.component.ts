import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

// Import models and services
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
  enrollmentsWithScores: Enrollment[] = [];
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
    this.enrollmentService.getEnrollmentsByStudentId(this.studentId!).subscribe({
      next: (data) => {
        // Lọc ra những lượt đăng ký đã có điểm
        this.enrollmentsWithScores = data.filter(e => e.score);
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải kết quả học tập.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }
}
