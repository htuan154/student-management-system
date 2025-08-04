import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';

// Import models and services
import { Score, TeacherCourse } from '../../../models';
import { ScoreService } from '../../../services/score.service';
import { AuthService } from '../../../services/auth.service';
import { TeacherCourseService } from '../../../services/teacher-course.service';

@Component({
  selector: 'app-class-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './class-list.component.html',
  styleUrls: ['./class-list.component.scss']
})
export class ClassListComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;
  teacherId: string | null = null;

  // Dữ liệu cho dropdown và bảng
  assignedCourses: TeacherCourse[] = [];
  selectedCourseId: string | null = null;
  scores: Score[] = [];

  constructor(
    private scoreService: ScoreService,
    private authService: AuthService,
    private teacherCourseService: TeacherCourseService
  ) { }

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.teacherId = tokenPayload?.teacherId;

    if (this.teacherId) {
      this.loadAssignedCourses();
    } else {
      this.errorMessage = "Không thể xác định thông tin giảng viên.";
    }
  }

  // Tải danh sách các môn học được phân công để đưa vào dropdown
  loadAssignedCourses(): void {
    this.isLoading = true;
    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).subscribe({
      next: (data) => {
        this.assignedCourses = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách lớp học.')
    });
  }

  // Được gọi khi giảng viên chọn một môn học từ dropdown
  onCourseSelect(): void {
    if (this.selectedCourseId) {
      this.loadStudentScores(this.selectedCourseId);
    } else {
      this.scores = []; // Xóa danh sách điểm nếu không chọn môn nào
    }
  }

  // Tải danh sách sinh viên và điểm dựa trên môn học đã chọn
  loadStudentScores(courseId: string): void {
    this.isLoading = true;
    this.scoreService.getByTeacherAndSubject(this.teacherId!, courseId).subscribe({
      next: (data) => {
        this.scores = data;
        this.scores.forEach(score => this.calculateTotalScore(score));
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách sinh viên cho lớp này.')
    });
  }

  calculateTotalScore(score: Score): void {
    const process = score.processScore ?? 0;
    const midterm = score.midtermScore ?? 0;
    const final = score.finalScore ?? 0;
    score.totalScore = +(process * 0.2 + midterm * 0.3 + final * 0.5).toFixed(2);
    score.isPassed = score.totalScore >= 4;
  }

  saveAllScores(): void {
    if (!this.selectedCourseId) return;
    this.isLoading = true;
    const updateRequests = this.scores.map(score => {
        const scoreData: Partial<Score> = {
            processScore: score.processScore,
            midtermScore: score.midtermScore,
            finalScore: score.finalScore,
            totalScore: score.totalScore,
            isPassed: score.isPassed
        };
        return this.scoreService.updateScore(score.scoreId, scoreData);
    });

    forkJoin(updateRequests).subscribe({
        next: () => {
            this.isLoading = false;
            alert('Lưu điểm thành công!');
            this.loadStudentScores(this.selectedCourseId!);
        },
        error: (err) => this.handleError(err, 'Đã có lỗi xảy ra khi lưu điểm.')
    });
  }

  /**
 * Format điểm số hiển thị
 */
formatScore(score: number | null): string {
  if (score === null || score === undefined) return 'N/A';
  return score.toFixed(1);
}

/**
 * Lấy text kết quả
 */
getResultText(isPassed: boolean | null): string {
  if (isPassed === null || isPassed === undefined) return 'N/A';
  return isPassed ? 'Đậu' : 'Rớt';
}

/**
 * Lấy CSS class cho kết quả
 */
getResultClass(isPassed: boolean | null): string {
  if (isPassed === null || isPassed === undefined) return 'result-pending';
  return isPassed ? 'result-pass' : 'result-fail';
}

  private handleError(error: HttpErrorResponse, message: string): void {
    this.errorMessage = message;
    this.isLoading = false;
    console.error(error);
  }
}
