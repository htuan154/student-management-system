import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { ScoreService } from '../../../../services/score.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
type EnrollmentLite = {
  enrollmentId: number;
  studentId: string;
  courseId: string;
  teacherCourseId?: number | null;
  student?: { fullName?: string };
  course?: { courseName?: string };
};

@Component({
  selector: 'app-score-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './score-form.component.html',
  styleUrls: ['./score-form.component.scss']
})
export class ScoreFormComponent implements OnInit {
  // trạng thái
  isLoading = false;
  isSaving = false;
  isEditMode = false; // template cần biến này
  errorMessage: string | null = null;

  // query params
  courseId!: string;
  teacherId!: string;
  teacherCourseId!: number;

  // form + data
  scoreForm!: FormGroup;
  enrollments: EnrollmentLite[] = []; // khớp với template

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private enrollmentService: EnrollmentService,
    private scoreService: ScoreService
  ) {}

  ngOnInit(): void {
    this.scoreForm = this.fb.group({
      enrollmentId: [null, Validators.required],
      processScore: [null, [Validators.min(0), Validators.max(10)]],
      midtermScore: [null, [Validators.min(0), Validators.max(10)]],
      finalScore:   [null, [Validators.min(0), Validators.max(10)]],
    });

    const qp = this.route.snapshot.queryParamMap;
    this.courseId = qp.get('courseId') || '';
    this.teacherId = qp.get('teacherId') || '';

    if (!this.courseId || !this.teacherId) {
      this.errorMessage = 'URL không hợp lệ: thiếu courseId hoặc teacherId.';
      return;
    }

    this.loadData();
  }

  private loadData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // 1) Resolve TeacherCourseId từ teacherId + courseId
    this.enrollmentService.getTeacherCourseByIds(this.teacherId, this.courseId).subscribe({
      next: (tc: any) => {
        if (!tc || !tc.teacherCourseId) {
          this.isLoading = false;
          this.errorMessage = 'Không tìm thấy lớp (TeacherCourse) phù hợp.';
          return;
        }
        this.teacherCourseId = tc.teacherCourseId;

        // 2) Lấy danh sách ENROLLMENT CHƯA CÓ ĐIỂM
        this.enrollmentService.getUnscoredEnrollmentsForClass(this.teacherCourseId).subscribe({
          next: (rows) => {
            // rows thường có kiểu Enrollment[] -> ép về EnrollmentLite để tránh TS2322
            this.enrollments = (rows || []) as unknown as EnrollmentLite[];
            this.isLoading = false;
          },
          error: (err) => {
            console.error('getUnscoredEnrollmentsForClass error', err);
            this.errorMessage = 'Không tải được danh sách lượt đăng ký.';
            this.isLoading = false;
          }
        });
      },
      error: (err) => {
        console.error('getTeacherCourseByIds error', err);
        this.errorMessage = 'Không xác định được lớp (TeacherCourse).';
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.scoreForm.invalid) {
      this.scoreForm.markAllAsTouched();
      return;
    }
    this.isSaving = true;
    const v = this.scoreForm.value;

    const payload = {
      enrollmentId: v.enrollmentId,
      processScore: v.processScore ?? null,
      midtermScore: v.midtermScore ?? null,
      finalScore:   v.finalScore ?? null
    };

    this.scoreService.createScore(payload).subscribe({
      next: () => {
        this.isSaving = false;
        // reload lại danh sách "chưa có điểm" để item vừa nhập biến mất
        this.loadData();
        this.scoreForm.reset();
      },
      error: (err) => {
        console.error('createScore error', err);
        this.errorMessage = 'Lưu điểm thất bại. Vui lòng thử lại.';
        this.isSaving = false;
      }
    });
  }
}
