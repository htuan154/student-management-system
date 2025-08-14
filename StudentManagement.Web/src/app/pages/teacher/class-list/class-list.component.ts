import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Models & services
import { Score, TeacherCourse } from '../../../models';
import { ScoreService } from '../../../services/score.service';
import { AuthService } from '../../../services/auth.service';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { SemesterService } from '../../../services/semester.service';
import { Semester } from '../../../models/Semester.model';

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

  // Dropdown + bảng
  assignedCourses: TeacherCourse[] = [];
  selectedCourseId: string | null = null;
  scores: Score[] = [];

  // Map học kỳ để hiển thị nhãn đẹp
  private semMap = new Map<number, Semester>();

  constructor(
    private scoreService: ScoreService,
    private authService: AuthService,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.teacherId = tokenPayload?.teacherId;
    if (this.teacherId) {
      this.loadAssignedCourses();
    } else {
      this.errorMessage = 'Không thể xác định thông tin giảng viên.';
    }
  }

  /** Tải danh sách phân công + join học kỳ để dropdown không còn (N/A) */
  loadAssignedCourses(): void {
    this.isLoading = true;
    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).subscribe({
      next: (data) => {
        this.assignedCourses = data || [];

        // Thu thập các semesterId duy nhất rồi nạp thông tin kỳ
        const ids = Array.from(
          new Set(
            this.assignedCourses
              .map(tc => tc.semesterId)
              .filter((x): x is number => x != null)
          )
        );

        if (!ids.length) {
          this.isLoading = false;
          return;
        }

        forkJoin(
          ids.map(id =>
            this.semesterService.getSemesterById(id)
              .pipe(catchError(() => of(null as unknown as Semester)))
          )
        ).subscribe({
          next: (semList) => {
            this.semMap.clear();
            (semList || []).forEach(s => {
              if (s?.semesterId != null) this.semMap.set(s.semesterId, s);
            });
            this.isLoading = false;
          },
          error: () => { this.isLoading = false; }
        });
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách lớp học.')
    });
  }

  /** Khi chọn môn trong dropdown */
  onCourseSelect(): void {
    if (this.selectedCourseId) {
      this.loadStudentScores(this.selectedCourseId);
    } else {
      this.scores = [];
    }
  }

  /** Tải danh sách SV/điểm theo môn */
  loadStudentScores(courseId: string): void {
    this.isLoading = true;
    this.scoreService.getByTeacherAndSubject(this.teacherId!, courseId).subscribe({
      next: (data) => {
        this.scores = data || [];
        this.scores.forEach(s => this.calculateTotalScore(s));
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách sinh viên cho lớp này.')
    });
  }

  /** Tính tổng điểm (chỉ khi đủ 3 cột) */
  calculateTotalScore(score: Score): void {
    const process = this.validateScore(score.processScore);
    const midterm = this.validateScore(score.midtermScore);
    const final = this.validateScore(score.finalScore);

    if (process !== null && midterm !== null && final !== null) {
      const total = +(process * 0.2 + midterm * 0.3 + final * 0.5).toFixed(2);
      score.totalScore = total;
      score.isPassed = total >= 4.0;
    } else {
      score.totalScore = null;
      score.isPassed = null;
    }
  }

  /** Chuẩn hoá điểm (0–10) và chấp nhận null */
  private validateScore(score: number | null | undefined): number | null {
    if (score === null || score === undefined || isNaN(score)) return null;
    return Math.max(0, Math.min(10, +score));
  }

  /** Lưu tất cả điểm của bảng hiện tại */
  saveAllScores(): void {
    if (!this.selectedCourseId) return;
    this.isLoading = true;

    const toNullable = (v: any) =>
      v === null || v === undefined || isNaN(+v) ? null : +(+v).toFixed(2);

    const reqs = this.scores.map(score => {
      const dto = {
        scoreId: score.scoreId,
        studentId:
          score.studentId ||
          score.enrollment?.student?.studentId ||
          score.enrollment?.studentId ||
          null,
        courseId: this.selectedCourseId, // môn đang chọn

        processScore: toNullable(score.processScore),
        midtermScore: toNullable(score.midtermScore),
        finalScore:   toNullable(score.finalScore),

        totalScore:
          score.processScore != null &&
          score.midtermScore  != null &&
          score.finalScore    != null
            ? toNullable(score.totalScore)
            : null,

        isPassed:
          score.isPassed === null || score.isPassed === undefined
            ? null
            : !!score.isPassed
      };

      return this.scoreService.updateScore(score.scoreId, dto);
      // Nếu có bản ghi mới (chưa có scoreId) thì dùng createScore(dto)
    });

    forkJoin(reqs).subscribe({
      next: () => {
        this.isLoading = false;
        alert('Lưu điểm thành công!');
        this.loadStudentScores(this.selectedCourseId!);
      },
      error: (err) => this.handleError(err, 'Đã có lỗi xảy ra khi lưu điểm.')
    });
  }

  /* ===== Helpers cho template ===== */

  /** Nhãn option: CS101 – Học kỳ 1 (2024-2025) */
  optionLabel(tc: TeacherCourse): string {
    const s = tc.semesterId != null ? this.semMap.get(tc.semesterId) : undefined;
    const hk = s?.semesterName ?? 'HK?';
    const year = s?.academicYear ?? 'N/A';
    return `${tc.courseId} - ${hk} (${year})`;
  }

  formatScore(score: number | null | undefined): string {
    if (score === null || score === undefined || isNaN(score)) return 'N/A';
    return (+score).toFixed(1);
  }

  getResultText(isPassed: boolean | null | undefined): string {
    if (isPassed === null || isPassed === undefined) return 'N/A';
    return isPassed ? 'Đậu' : 'Rớt';
  }

  getResultClass(isPassed: boolean | null | undefined): string {
    if (isPassed === null || isPassed === undefined) return 'result-pending';
    return isPassed ? 'result-pass' : 'result-fail';
  }

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
