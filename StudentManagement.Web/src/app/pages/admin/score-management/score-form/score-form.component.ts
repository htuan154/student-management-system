import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ScoreService } from '../../../../services/score.service';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { Enrollment } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-score-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './score-form.component.html',
  styleUrls: ['./score-form.component.scss']
})
export class ScoreFormComponent implements OnInit {
  scoreForm!: FormGroup;
  isEditMode = false;
  scoreId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  enrollments: Enrollment[] = [];

  private courseId: string | null = null;
  private teacherId: string | null = null;
  teacherCourseId: number = 0; // ⬅ SỬA: Khởi tạo với 0 thay vì null

  constructor(
    private fb: FormBuilder,
    private scoreService: ScoreService,
    private enrollmentService: EnrollmentService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // ✅ KHỞI TẠO FORM NGAY TRONG CONSTRUCTOR
    this.initForm();
  }

  ngOnInit(): void {
    // Lấy params từ query string
    this.route.queryParams.subscribe(params => {
      this.courseId = params['courseId'];
      this.teacherId = params['teacherId'];

      console.log('Query params:', { courseId: this.courseId, teacherId: this.teacherId });

      if (this.courseId && this.teacherId) {
        this.loadEnrollmentsForClass();
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.scoreId = +id;
      this.loadScoreData(this.scoreId);
    } else {
      this.isEditMode = false;
    }
  }

  initForm(): void {
    this.scoreForm = this.fb.group({
      enrollmentId: [{ value: null, disabled: this.isEditMode }, Validators.required],
      processScore: [null, [Validators.min(0), Validators.max(10)]],
      midtermScore: [null, [Validators.min(0), Validators.max(10)]],
      finalScore: [null, [Validators.min(0), Validators.max(10)]]
    });
  }

  loadEnrollmentsForClass(): void {
    if (!this.courseId || !this.teacherId) {
      this.errorMessage = "URL không hợp lệ, thiếu mã môn học hoặc mã giảng viên.";
      return;
    }

    this.isLoading = true;

    // ✅ SỬA: Sử dụng method từ enrollment service
    this.enrollmentService.getTeacherCourseByIds(this.teacherId, this.courseId).subscribe({
      next: (teacherCourse) => {
        if (!teacherCourse) {
          this.errorMessage = "Không tìm thấy phân công giảng dạy phù hợp.";
          this.isLoading = false;
          return;
        }

        console.log('Found TeacherCourse:', teacherCourse);
        this.teacherCourseId = teacherCourse.teacherCourseId;

        // Lấy sinh viên chưa có điểm
        this.loadUnscoredStudents(this.teacherCourseId);
      },
      error: (error) => {
        console.error('Error finding teacher course:', error);
        this.errorMessage = "Không thể tìm thông tin phân công giảng dạy.";
        this.isLoading = false;
      }
    });
  }

  loadUnscoredStudents(teacherCourseId: number): void {
    this.enrollmentService.getUnscoredEnrollmentsForClass(teacherCourseId).subscribe({
      next: (enrollments) => {
        console.log('Unscored enrollments:', enrollments);
        this.enrollments = enrollments;
        this.isLoading = false;

        if (enrollments.length === 0) {
          this.errorMessage = "Tất cả sinh viên trong lớp này đã được nhập điểm.";
        } else {
          this.errorMessage = null;
        }
      },
      error: (error) => {
        console.error('Error getting unscored enrollments:', error);
        this.errorMessage = "Không thể tải danh sách sinh viên chưa có điểm.";
        this.isLoading = false;
      }
    });
  }

  loadScoreData(id: number): void {
    this.isLoading = true;
    this.scoreService.getScoreById(id).subscribe({
      next: (scoreData) => {
        this.scoreForm.patchValue(scoreData);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Không thể tải dữ liệu điểm.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.scoreForm.invalid) {
      this.scoreForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formData = this.scoreForm.getRawValue();

    console.log('Form data:', formData);

    const operation = this.isEditMode
      ? this.scoreService.updateScore(this.scoreId!, formData)
      : this.scoreService.createScore(formData);

    operation.subscribe({
      next: () => {
        alert('Lưu điểm thành công!');
        this.router.navigate(['/admin/scores']);
      },
      error: (err) => {
        console.error('Error saving score:', err);
        this.errorMessage = `Đã có lỗi xảy ra. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
