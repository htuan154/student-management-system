import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ClassService, ClassCreateUpdateDto } from '../../../../services/class.service';
import { SemesterService } from '../../../../services/semester.service'; // <-- THÊM

type SemesterOption = {
  semesterId: number;
  semesterName: string;
  academicYear: string;
  isActive: boolean;
};

@Component({
  selector: 'app-class-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './class-form.component.html',
  styleUrls: ['./class-form.component.scss']
})
export class ClassFormComponent implements OnInit {
  classForm!: FormGroup;
  isEditMode = false;
  isLoading = false;
  errorMessage: string | null = null;
  private classId: string | null = null;

  // ---- Học kỳ
  semesterOptions: SemesterOption[] = [];
  isLoadingSemesters = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private classService: ClassService,
    private semesterService: SemesterService // <-- THÊM
  ) {}

  ngOnInit(): void {
    this.classId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.classId;

    this.initForm();
    this.loadSemesters(); // <-- tải danh sách học kỳ

    if (this.isEditMode && this.classId) this.loadClassData(this.classId);

    // Khi chọn học kỳ -> tự set Niên khóa
    this.classForm.get('semesterId')!.valueChanges.subscribe(() => {
      this.updateAcademicYearFromSelection();
    });
  }

  private initForm(): void {
    this.classForm = this.fb.group({
      classId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      className: ['', [Validators.required, Validators.maxLength(50)]],
      major: ['', [Validators.required, Validators.maxLength(100)]],
      semesterId: [null, [Validators.required]],    // <-- dùng select
      academicYear: [{ value: '', disabled: true }],// <-- hiển thị theo học kỳ
      isActive: [true],
    });
  }

  private loadSemesters(): void {
    this.isLoadingSemesters = true;
    // Có thể dùng getAllSemesters() nếu muốn thấy cả học kỳ ngưng hoạt động
    this.semesterService.getActiveSemesters().subscribe({
      next: (list) => {
        this.semesterOptions = (list || []).map(s => ({
          semesterId: s.semesterId,
          semesterName: (s as any).semesterName ?? `Học kỳ ${s.semesterId}`,
          academicYear: (s as any).academicYear ?? '',
          isActive: !!(s as any).isActive
        }));
        this.isLoadingSemesters = false;

        // Nếu form đã có sẵn semesterId (trường hợp edit), cập nhật niên khóa
        this.updateAcademicYearFromSelection();
      },
      error: () => {
        this.isLoadingSemesters = false;
        // không chặn form; chỉ không có danh sách học kỳ
      }
    });
  }

  private updateAcademicYearFromSelection(): void {
    const id = Number(this.classForm.get('semesterId')!.value);
    const found = this.semesterOptions.find(s => s.semesterId === id);
    this.classForm.get('academicYear')!.setValue(found?.academicYear || '');
  }

  private loadClassData(id: string): void {
    this.isLoading = true;
    this.classService.getClassById(id).subscribe({
      next: (c) => {
        this.classForm.patchValue({
          classId: c.classId,
          className: c.className,
          major: c.major,
          semesterId: (c as any).semesterId ?? null,
          academicYear: (c as any).academicYear ?? '',
          isActive: c.isActive
        });
        this.classForm.get('classId')?.disable();
        this.isLoading = false;

        // đã patch xong -> đồng bộ lại niên khóa theo danh sách (nếu danh sách vừa load xong)
        this.updateAcademicYearFromSelection();
      },
      error: () => {
        this.errorMessage = 'Không tải được dữ liệu lớp.';
        this.isLoading = false;
      }
    });
  }

  trackSemester = (_: number, s: SemesterOption) => s.semesterId;

  onSubmit(): void {
    if (this.classForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    const raw = this.classForm.getRawValue(); // lấy luôn classId khi control disabled
    const dto: ClassCreateUpdateDto = {
      classId: String(raw.classId).trim(),
      className: String(raw.className).trim(),
      major: String(raw.major).trim(),
      semesterId: Number(raw.semesterId),
      isActive: !!raw.isActive
    };

    const req$ = this.isEditMode && this.classId
      ? this.classService.updateClass(this.classId, dto)
      : this.classService.createClass(dto);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/classes']),
      error: (err) => {
        if (err.status === 400 && (typeof err.error === 'string' || err.error?.message)) {
          this.errorMessage = typeof err.error === 'string' ? err.error : err.error.message;
        } else if (err.status === 400 && err.error?.errors) {
          const all = Object.values(err.error.errors).flat() as string[];
          this.errorMessage = all.join(' ');
        } else if (err.status === 409) {
          this.errorMessage = 'Mã hoặc tên lớp đã tồn tại.';
        } else if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'Bạn chưa đăng nhập hoặc không đủ quyền.';
        } else {
          this.errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại.';
        }
        this.isLoading = false;
      }
    });
  }
}
