import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { TeacherCourse } from '../../../../models/teacher-course.model';
import { Semester } from '../../../../models/Semester.model';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';
import { TeacherService } from '../../../../services/teacher.service';
import { CourseService } from '../../../../services/course.service';

@Component({
  selector: 'app-teacher-course-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './teacher-course-form.component.html',
  styleUrls: ['./teacher-course-form.component.scss']
})
export class TeacherCourseFormComponent implements OnInit {
  form!: FormGroup;

  isEditMode = false;
  id: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  teachers: any[] = [];
  courses: any[] = [];
  semesters: Semester[] = []; // chỉ chứa các kỳ ĐANG HOẠT ĐỘNG

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService,
    private teacherService: TeacherService,
    private courseService: CourseService
  ) {
    this.form = this.fb.group({
      teacherId: ['', Validators.required],
      courseId: ['', Validators.required],
      semesterId: [null as number | null, Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (param) {
      this.id = +param;
      this.isEditMode = true;
    }
    this.loadLookups();

    if (this.isEditMode && this.id) {
      this.isLoading = true;
      this.teacherCourseService.getTeacherCourseById(this.id).subscribe({
        next: (tc: TeacherCourse) => {
          this.form.patchValue({
            teacherId: tc.teacherId,
            courseId: tc.courseId,
            semesterId: tc.semesterId,
            isActive: tc.isActive
          });
          // nếu học kỳ của bản ghi không còn active -> reset, buộc chọn lại
          if (!this.isActiveSemester(tc.semesterId!)) {
            this.form.get('semesterId')?.setValue(null);
            this.errorMessage = 'Học kỳ của bản ghi đã ngưng hoạt động. Vui lòng chọn học kỳ đang hoạt động.';
          }
          this.isLoading = false;
        },
        error: () => {
          this.errorMessage = 'Không thể tải dữ liệu phân công.';
          this.isLoading = false;
        }
      });
    }
  }

  /** Nạp dữ liệu danh mục; CHỈ lấy học kỳ đang hoạt động */
  loadLookups(): void {
    this.teacherService.getAllTeachers().subscribe(r => this.teachers = r || []);
    this.courseService.getAllCourses().subscribe(r => this.courses = r || []);
    this.semesterService.getActiveSemesters().subscribe(r => {
      this.semesters = r || [];
      // nếu đã chọn sẵn semesterId nhưng không còn active -> reset
      const cur = this.form.get('semesterId')?.value as number | null;
      if (cur != null && !this.isActiveSemester(cur)) {
        this.form.get('semesterId')?.setValue(null);
      }
    });
  }

  /** Kiểm tra kỳ có đang hoạt động không (dựa vào danh sách đã load) */
  private isActiveSemester(semesterId: number): boolean {
    return this.semesters.some(s => s.semesterId === semesterId);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    this.errorMessage = null;

    const payload = this.form.value;

    // CHẶN: chỉ cho phép phân công ở học kỳ đang hoạt động
    if (!this.isActiveSemester(payload.semesterId)) {
      this.errorMessage = 'Chỉ được phân công ở học kỳ đang hoạt động.';
      this.isLoading = false;
      return;
    }

    const req$ = this.isEditMode && this.id
      ? this.teacherCourseService.updateTeacherCourse(this.id, payload)
      : this.teacherCourseService.createTeacherCourse(payload);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/assignments']),
      error: (e) => {
        this.errorMessage = e?.error?.message || 'Có lỗi xảy ra.';
        this.isLoading = false;
      }
    });
  }
}
