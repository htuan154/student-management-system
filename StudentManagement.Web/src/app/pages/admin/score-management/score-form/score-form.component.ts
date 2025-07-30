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

  enrollments: Enrollment[] = []; // Dữ liệu cho dropdown

  // Thêm các thuộc tính này để lưu tham số từ URL
  private courseId: string | null = null;
  private teacherId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private scoreService: ScoreService,
    private enrollmentService: EnrollmentService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      // Chế độ Sửa
      this.isEditMode = true;
      this.scoreId = +id;
      this.initForm();
      this.loadScoreData(this.scoreId);
    } else {
      // Chế độ Tạo mới
      this.isEditMode = false;
      // Lấy courseId và teacherId từ URL
      this.courseId = this.route.snapshot.paramMap.get('courseId');
      this.teacherId = this.route.snapshot.paramMap.get('teacherId');
      this.initForm();
      // Gọi phương thức đúng để tải danh sách đã lọc
      this.loadEnrollmentsForClass();
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

  // Phương thức mới để tải các lượt đăng ký cho một lớp học cụ thể
  loadEnrollmentsForClass(): void {
    if (!this.courseId || !this.teacherId) {
      this.errorMessage = "URL không hợp lệ, thiếu mã môn học hoặc mã giảng viên.";
      return;
    }

    this.isLoading = true;
    this.enrollmentService.getUnscoredEnrollmentsForClass(this.courseId, this.teacherId).subscribe({
      next: (data) => {
        this.enrollments = data;
        this.isLoading = false;
        if (data.length === 0) {
            this.errorMessage = "Tất cả sinh viên trong lớp này đã được nhập điểm hoặc không có sinh viên nào.";
        }
      },
      error: () => {
        this.errorMessage = "Không thể tải danh sách đăng ký cho lớp học này.";
        this.isLoading = false;
      }
    });
  }

  loadScoreData(id: number): void {
    this.isLoading = true;
    this.scoreService.getScoreById(id).subscribe({
      next: (scoreData) => {
        this.scoreForm.patchValue(scoreData);
        // Ở chế độ sửa, bạn có thể cần tải chi tiết của lượt đăng ký
        // để hiển thị thông tin trong dropdown (dù bị vô hiệu hóa).
        // Ví dụ:
        // this.enrollmentService.getEnrollmentById(scoreData.enrollmentId).subscribe(enrollment => {
        //   this.enrollments = [enrollment];
        // });
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

    const operation = this.isEditMode
      ? this.scoreService.updateScore(this.scoreId!, formData)
      : this.scoreService.createScore(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/scores']);
      },
      error: (err) => {
        this.errorMessage = `Đã có lỗi xảy ra. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
