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
        console.log('=== DEBUG ASSIGNED COURSES ===');
        console.log('Raw data:', data);
        data.forEach((course, index) => {
          console.log(`Course ${index}:`, {
            courseId: course.courseId,
            courseName: course.course?.courseName,
            semester: course.semester,
            semesterName: course.semester?.semesterName,
            academicYear: course.semester?.academicYear,
            semesterId: course.semesterId
          });
        });
        console.log('===============================');

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
      this.scores = [];
    }
  }

  // Tải danh sách sinh viên và điểm dựa trên môn học đã chọn
  loadStudentScores(courseId: string): void {
    this.isLoading = true;
    console.log('Loading scores for teacherId:', this.teacherId, 'courseId:', courseId);

    // ✅ SỬA: Dùng method đúng
    this.scoreService.getByTeacherAndSubject(this.teacherId!, courseId).subscribe({
      next: (data) => {
        console.log('Loaded scores:', data);
        this.scores = data;
        this.scores.forEach(score => this.calculateTotalScore(score));
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading scores:', err);
        this.handleError(err, 'Không thể tải danh sách sinh viên cho lớp này.');
      }
    });
  }

  /** ✅ SỬA: Sửa calculateTotalScore để handle null */
  calculateTotalScore(score: Score): void {
    const process = this.validateScore(score.processScore);
    const midterm = this.validateScore(score.midtermScore);
    const final = this.validateScore(score.finalScore);

    // ✅ SỬA: Chỉ tính tổng khi có đủ điểm
    if (process !== null && midterm !== null && final !== null) {
      const total = +(process * 0.2 + midterm * 0.3 + final * 0.5).toFixed(2);
      score.totalScore = total;
      score.isPassed = total >= 4.0;
    } else {
      score.totalScore = null;
      score.isPassed = null;
    }
  }

  /** ✅ THÊM: Validation helper */
  private validateScore(score: number | null | undefined): number | null {
    if (score === null || score === undefined || isNaN(score)) return null;
    return Math.max(0, Math.min(10, score)); // Clamp between 0-10
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
formatScore(score: number | null | undefined): string {
  if (score === null || score === undefined || isNaN(score)) return 'N/A';
  return score.toFixed(1);
}

/**
 * Lấy text kết quả
 */
getResultText(isPassed: boolean | null | undefined): string {
  if (isPassed === null || isPassed === undefined) return 'N/A';
  return isPassed ? 'Đậu' : 'Rớt';
}

/**
 * Lấy CSS class cho kết quả
 */
getResultClass(isPassed: boolean | null | undefined): string {
  if (isPassed === null || isPassed === undefined) return 'result-pending';
  return isPassed ? 'result-pass' : 'result-fail';
}

/** ✅ THÊM: Helper methods cho template */
getTotalScoreValue(score: Score): number {
  return score.totalScore ?? 0;
}

isScoreComplete(score: Score): boolean {
  return score.totalScore !== null && score.totalScore !== undefined && !isNaN(score.totalScore);
}

  private handleError(error: HttpErrorResponse, message: string): void {
    this.errorMessage = message;
    this.isLoading = false;
    console.error(error);
  }
}
