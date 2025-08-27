import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SemesterService } from '../../../../services/semester.service';
import { Semester } from '../../../../models/Semester.model';

@Component({
  selector: 'app-semester-form',
  templateUrl: './semester-form.component.html',
  styleUrls: ['./semester-form.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
})
export class SemesterFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  id?: number;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private semesterService: SemesterService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group(
      {
        semesterName: ['', Validators.required],
        academicYear: ['', Validators.required],
        startDate: ['', Validators.required],    // input type="date"
        endDate:   ['', Validators.required],    // input type="date"
        isActive:  [true],
      },
      { validators: [this.dateRangeValidator] }
    );
  }

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      this.isEdit = true;
      this.isLoading = true;
      this.semesterService.getSemesterById(this.id).subscribe({
        next: (s: Semester) => {
          this.form.patchValue({
            semesterName: s.semesterName,
            academicYear: s.academicYear,
            startDate: this.toInputDate(s.startDate),
            endDate:   this.toInputDate(s.endDate),
            isActive:  s.isActive,
          });
          // đồng bộ trạng thái ngay lần đầu
          this.syncActiveByExpiry();
          this.isLoading = false;
        },
        error: () => {
          this.errorMessage = 'Không tải được học kỳ.';
          this.isLoading = false;
        }
      });
    }

    // Khi endDate đổi → tự đồng bộ isActive (hết hạn thì khóa & off)
    this.form.get('endDate')?.valueChanges.subscribe(() => this.syncActiveByExpiry());
  }

  // ===== Helpers =====
  private toInputDate(iso?: string | null): string {
    if (!iso) return '';
    if (/^\d{4}-\d{2}-\d{2}$/.test(iso)) return iso; // đã đúng định dạng input
    const d = new Date(iso);
    if (isNaN(d.getTime())) return '';
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${m}-${day}`;
  }

  private today(): Date {
    const t = new Date();
    t.setHours(0, 0, 0, 0);
    return t;
  }

  private isExpired(endDateStr: string | null | undefined): boolean {
    if (!endDateStr) return false;
    const ed = new Date(endDateStr);
    ed.setHours(0, 0, 0, 0);
    return ed.getTime() < this.today().getTime();
  }

  /** Nếu hết hạn: tự tắt isActive và khóa control. Ngược lại: mở khóa. */
  private syncActiveByExpiry(): void {
    const endStr = this.form.get('endDate')?.value as string;
    const expired = this.isExpired(endStr);

    const isActiveCtl = this.form.get('isActive')!;
    if (expired) {
      if (isActiveCtl.enabled) isActiveCtl.disable({ emitEvent: false });
      if (isActiveCtl.value !== false) isActiveCtl.setValue(false, { emitEvent: false });
    } else {
      if (isActiveCtl.disabled) isActiveCtl.enable({ emitEvent: false });
      // không tự bật khi chưa hết hạn — để người dùng quyết định
    }
  }

  private dateRangeValidator(group: AbstractControl): ValidationErrors | null {
    const s = group.get('startDate')?.value as string;
    const e = group.get('endDate')?.value as string;
    if (!s || !e) return null;
    const sd = new Date(s); sd.setHours(0,0,0,0);
    const ed = new Date(e); ed.setHours(0,0,0,0);
    return ed.getTime() >= sd.getTime()
      ? null
      : { dateRange: 'Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.' };
  }

  // ===== Submit =====
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // Báo sớm nếu thiếu token để tránh 401 khó hiểu
    const token = localStorage.getItem('access_token');
    if (!token) {
      this.errorMessage = 'Token is invalid or missing. Vui lòng đăng nhập lại.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const value = {
      semesterName: String(this.form.value.semesterName).trim(),
      academicYear: String(this.form.value.academicYear).trim(),
      startDate: String(this.form.value.startDate), // YYYY-MM-DD
      endDate: String(this.form.value.endDate),     // YYYY-MM-DD
      // bảo vệ lần cuối: hết hạn thì luôn false
      isActive: !this.isExpired(this.form.value.endDate) && !!this.form.value.isActive,
    };

    const req$ =
      this.isEdit && this.id
        ? this.semesterService.updateSemester(this.id!, value)
        : this.semesterService.createSemester(value);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/semester-management']),
      error: (err) => {
        if (err?.status === 401) {
          this.errorMessage = 'Phiên đăng nhập hết hạn hoặc thiếu quyền. Vui lòng đăng nhập lại.';
        } else if (err?.status === 400 && err?.error?.errors) {
          const msgs = (Object.values(err.error.errors).flat() as string[]) ?? [];
          this.errorMessage = msgs.join(' ');
        } else if (typeof err?.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = err?.error?.message || 'Đã có lỗi xảy ra. Vui lòng kiểm tra lại.';
        }
        this.isLoading = false;
      },
    });
  }
}
