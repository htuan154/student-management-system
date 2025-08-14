import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { EnrollmentService } from '../../../../services/enrollment.service';
import { StudentService } from '../../../../services/student.service';
import { CourseService } from '../../../../services/course.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';

import { Student, Course, TeacherCourse, Semester, Enrollment } from '../../../../models';
import { of, forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-enrollment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './enrollment-form.component.html',
  styleUrls: ['./enrollment-form.component.scss']
})
export class EnrollmentFormComponent implements OnInit {
  enrollmentForm!: FormGroup;

  isEditMode = false;
  isLoading = false;
  errorMessage: string | null = null;

  // data sources
  students: Student[] = [];
  courses: Course[] = [];
  semesters: Semester[] = [];          // CHỈ các kỳ đang hoạt động (getActiveSemesters)
  teacherCourses: TeacherCourse[] = []; // CHỈ các phân công thuộc kỳ đang hoạt động

  isLoadingTeacherCourses = false;
  private enrollmentId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private enrollmentService: EnrollmentService,
    private studentService: StudentService,
    private courseService: CourseService,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.initForm();

    this.enrollmentId = Number(this.route.snapshot.paramMap.get('id')) || null;
    this.isEditMode = !!this.enrollmentId;

    // load dropdown data in parallel
    this.loadInitialData();

    // when course changes -> reload teacher courses & reset selections
    this.enrollmentForm.get('courseId')!.valueChanges.subscribe((courseId: string) => {
      this.enrollmentForm.get('teacherCourseId')!.setValue(null);
      this.enrollmentForm.get('semesterId')!.setValue(null);
      if (courseId) this.loadTeacherCourses(courseId);
      else this.teacherCourses = [];
    });

    // when teacher course changes -> auto-set semester
    this.enrollmentForm.get('teacherCourseId')!.valueChanges.subscribe(() => {
      this.applySemesterFromSelectedTeacherCourse();
    });

    // edit mode: load record
    if (this.isEditMode && this.enrollmentId) {
      this.loadEnrollmentData(this.enrollmentId);
    }
  }

  private initForm(): void {
    this.enrollmentForm = this.fb.group({
      studentId: ['', Validators.required],
      courseId: ['', Validators.required],
      teacherCourseId: [null],              // optional
      semesterId: [null, Validators.required],
      status: ['Enrolled', Validators.required]
    });
  }

  private loadInitialData(): void {
    forkJoin({
      students: this.studentService.getAllStudents().pipe(catchError(() => of([] as Student[]))),
      courses: this.courseService.getAllCourses().pipe(catchError(() => of([] as Course[]))),
      // CHỈ lấy kỳ đang hoạt động
      semesters: this.semesterService.getActiveSemesters().pipe(catchError(() => of([] as Semester[])))
    }).subscribe(({ students, courses, semesters }) => {
      this.students = students || [];
      this.courses = courses || [];
      this.semesters = semesters || [];
      // Sau khi đã có danh sách kỳ hoạt động -> lọc lại teacherCourses (nếu có)
      this.applySemesterFilterToTeacherCourses();
    });
  }

  private loadTeacherCourses(courseId: string): void {
    this.isLoadingTeacherCourses = true;
    this.teacherCourseService.getByCourseId(courseId)
      .pipe(catchError(() => of([] as TeacherCourse[])))
      .subscribe(list => {
        // CHỈ giữ các TC thuộc kỳ đang hoạt động
        const activeIds = new Set(this.semesters.map(s => (s as any).semesterId));
        this.teacherCourses = (list || []).filter(tc => activeIds.has(tc.semesterId!));
        this.isLoadingTeacherCourses = false;
        this.applySemesterFromSelectedTeacherCourse();
      });
  }

  private applySemesterFromSelectedTeacherCourse(): void {
    const selId = this.enrollmentForm.get('teacherCourseId')!.value as number | null;
    if (!selId) return;
    const found = this.teacherCourses.find(tc => tc.teacherCourseId === selId);
    if (found?.semesterId) {
      this.enrollmentForm.get('semesterId')!.setValue(found.semesterId);
    }
  }

  // lọc lại teacherCourses theo các kỳ hoạt động (khi danh sách kỳ vừa được nạp)
  private applySemesterFilterToTeacherCourses(): void {
    if (!this.teacherCourses?.length) return;
    const activeIds = new Set(this.semesters.map(s => (s as any).semesterId));
    this.teacherCourses = this.teacherCourses.filter(tc => activeIds.has(tc.semesterId!));
    this.applySemesterFromSelectedTeacherCourse();
  }

  private loadEnrollmentData(id: number): void {
    this.isLoading = true;
    this.enrollmentService.getEnrollmentById(id)
      .pipe(catchError(() => { this.errorMessage = 'Không tải được đăng ký.'; return of(null as unknown as Enrollment); }))
      .subscribe((en) => {
        if (!en) { this.isLoading = false; return; }

        this.enrollmentForm.patchValue({
          studentId: en.studentId,
          courseId: en.courseId,
          teacherCourseId: en.teacherCourseId ?? null,
          semesterId: en.semesterId,
          status: en.status || 'Enrolled'
        });

        // nạp danh sách teacher course theo môn để select có item
        if (en.courseId) {
          this.loadTeacherCourses(en.courseId);
        }
        this.isLoading = false;
      });
  }

  // trackBy helpers
  trackStudent = (_: number, s: Student) => s.studentId;
  trackCourse = (_: number, c: Course) => c.courseId;
  trackTeacherCourse = (_: number, t: TeacherCourse) => t.teacherCourseId!;
  trackSemester = (_: number, s: Semester) => (s as any).semesterId;

  onSubmit(): void {
    if (this.enrollmentForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    const raw = this.enrollmentForm.getRawValue();
    const payload = {
      studentId: String(raw.studentId).trim(),
      courseId: String(raw.courseId).trim(),
      teacherCourseId: raw.teacherCourseId ? Number(raw.teacherCourseId) : null,
      semesterId: Number(raw.semesterId),
      status: String(raw.status).trim()
    };

    // ❗ Chặn đăng ký nếu học kỳ không hoạt động hoặc TC thuộc kỳ ngưng
    const activeIds = new Set(this.semesters.map(s => (s as any).semesterId));
    if (!activeIds.has(payload.semesterId)) {
      this.errorMessage = 'Học kỳ đã ngưng hoạt động. Vui lòng chọn học kỳ đang hoạt động.';
      this.isLoading = false;
      return;
    }
    if (payload.teacherCourseId) {
      const tc = this.teacherCourses.find(t => t.teacherCourseId === payload.teacherCourseId);
      if (tc && !activeIds.has(tc.semesterId!)) {
        this.errorMessage = 'Lớp giảng dạy thuộc học kỳ ngưng hoạt động. Vui lòng chọn lớp ở kỳ đang hoạt động.';
        this.isLoading = false;
        return;
      }
    }

    const req$ = this.isEditMode && this.enrollmentId
      ? this.enrollmentService.updateEnrollment(this.enrollmentId!, payload as any)
      : this.enrollmentService.createEnrollment(payload as any);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/enrollments']),
      error: (err) => {
        if (err?.status === 400 && err?.error?.errors) {
          const all = Object.values(err.error.errors).flat() as string[];
          this.errorMessage = all.join(' ');
        } else if (typeof err?.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = err?.error?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại.';
        }
        this.isLoading = false;
      }
    });
  }
}
