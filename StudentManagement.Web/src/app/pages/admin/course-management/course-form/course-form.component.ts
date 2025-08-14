import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CourseService } from '../../../../services/course.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-course-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './course-form.component.html',
  styleUrls: ['./course-form.component.scss']
})
export class CourseFormComponent implements OnInit {
  courseForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  errorMessage: string | null = null;
  private courseId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private courseService: CourseService
  ) {}

  ngOnInit(): void {
    this.courseForm = this.fb.group({
      courseId: ['', [Validators.required, Validators.maxLength(10)]],
      courseName: ['', [Validators.required, Validators.maxLength(100)]],
      credits: [1, [Validators.required, Validators.min(1), Validators.max(50)]],
      department: [null],
      description: [null],
      isActive: [true]
    });

    this.courseId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.courseId;

    if (this.isEditMode && this.courseId) {
      this.isLoading = true;
      this.courseService.getCourseById(this.courseId).subscribe({
        next: (c) => {
          this.courseForm.patchValue({
            courseId: c.courseId,
            courseName: c.courseName,
            credits: c.credits,
            department: c.department ?? null,
            description: c.description ?? null,
            isActive: c.isActive
          });
          // khóa mã khi sửa
          this.courseForm.get('courseId')?.disable();
          this.isLoading = false;
        },
        error: () => {
          this.errorMessage = 'Không tải được dữ liệu môn học.';
          this.isLoading = false;
        }
      });
    }
  }

  // ---- CÁCH A: Random mã ở FE + kiểm tra không trùng trên server ----
  private makeCourseId(len = 8): string {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789'; // bỏ O/I/0/1 cho dễ đọc
    let s = '';
    for (let i = 0; i < len; i++) s += chars[Math.floor(Math.random() * chars.length)];
    return `MH-${s}`.slice(0, 10); // tôn trọng maxlength=10
  }

  async generateRandomCourseId(): Promise<void> {
    if (this.isEditMode) return; // chỉ dùng khi tạo mới
    this.errorMessage = null;

    const maxTries = 12;
    for (let i = 0; i < maxTries; i++) {
      const candidate = this.makeCourseId();
      try {
        // Nếu tồn tại -> sẽ trả 200 => thử mã khác
        await firstValueFrom(this.courseService.getCourseById(candidate));
        continue;
      } catch (err: any) {
        if (err?.status === 404) {
          // chưa tồn tại -> dùng được
          this.courseForm.get('courseId')?.setValue(candidate);
          this.courseForm.get('courseId')?.markAsDirty();
          return;
        }
        // lỗi khác
        this.errorMessage = 'Không thể kiểm tra mã ngẫu nhiên. Vui lòng thử lại.';
        return;
      }
    }
    this.errorMessage = 'Không tạo được mã ngẫu nhiên (trùng quá nhiều). Thử lại lần nữa nhé.';
  }

  onSubmit(): void {
    if (this.courseForm.invalid) return;
    this.isLoading = true;
    this.errorMessage = null;

    const raw = this.courseForm.getRawValue(); // lấy luôn courseId khi control bị disable
    const payload = {
      courseId: String(raw.courseId).trim(),
      courseName: String(raw.courseName).trim(),
      credits: +raw.credits,
      department: (raw.department ?? '').toString().trim() || null,
      description: (raw.description ?? '').toString().trim() || null,
      isActive: !!raw.isActive
    };

    const req$ = this.isEditMode && this.courseId
      ? this.courseService.updateCourse(this.courseId, payload)
      : this.courseService.createCourse(payload);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/courses']),
      error: (err) => {
        if (err.status === 409) {
          this.errorMessage = typeof err.error === 'string' ? err.error : 'Mã môn học đã tồn tại.';
        } else if (err.status === 400 && err.error?.errors) {
          const all = Object.values(err.error.errors).flat() as string[];
          this.errorMessage = all.join(' ');
        } else if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'Bạn chưa đăng nhập hoặc không đủ quyền.';
        } else {
          this.errorMessage = err.error?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại.';
        }
        this.isLoading = false;
      }
    });
  }
}
