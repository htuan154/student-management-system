import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { StudentService } from '../../../../services/student.service';
import { CourseService } from '../../../../services/course.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';

import { Student, Course, TeacherCourse, Semester, Enrollment } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

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
  enrollmentId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Dropdown data
  students: Student[] = [];
  courses: Course[] = [];
  teacherCourses: TeacherCourse[] = [];
  semesters: Semester[] = [];

  constructor(
    private fb: FormBuilder,
    private enrollmentService: EnrollmentService,
    private studentService: StudentService,
    private courseService: CourseService,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.enrollmentId = +id;
      this.isEditMode = true;
    }

    this.initForm();
    this.loadRelatedData();

    // Tải dữ liệu chi tiết khi sửa
    if (this.isEditMode && this.enrollmentId) {
      this.loadEnrollmentData(this.enrollmentId);
    }

    // Tự động nạp danh sách lớp (teacher courses) khi chọn course
    this.enrollmentForm.get('courseId')?.valueChanges.subscribe((courseId: string) => {
      if (courseId) this.loadTeacherCourses(courseId);
      // Reset selection teacherCourseId khi đổi course
      this.enrollmentForm.get('teacherCourseId')?.setValue(null);
    });
  }

  private initForm(): void {
    this.enrollmentForm = this.fb.group({
      studentId: [{ value: '', disabled: this.isEditMode }, [Validators.required]],
      courseId: [{ value: '', disabled: this.isEditMode }, [Validators.required]],
      teacherCourseId: [null], // optional
      semesterId: [null, [Validators.required]],
      status: ['Enrolled', [Validators.required]]
    });
  }

  private loadRelatedData(): void {
    this.studentService.getAllStudents().subscribe(data => (this.students = data || []));
    this.courseService.getAllCourses().subscribe(data => (this.courses = data || []));
    this.semesterService.getAllSemesters().subscribe(data => (this.semesters = data || []));
  }

  private loadTeacherCourses(courseId: string): void {
    this.teacherCourses = [];
    this.teacherCourseService.getByCourseId(courseId)
      .pipe(catchError(() => of<TeacherCourse[]>([])))
      .subscribe((data) => {
        if (data && data.length) {
          this.teacherCourses = data.map(tc => ({
            ...tc,
            teacherId: tc.teacherId || tc.teacher?.teacherId || ''
          }));
        } else {
          // Fallback khi không có endpoint by-course hoặc trả rỗng
          this.loadTeacherCoursesViaEnrollments(courseId);
        }
      });
  }

  private loadTeacherCoursesViaEnrollments(courseId: string): void {
    this.teacherCourses = [];
    this.enrollmentService.getEnrollmentsByCourseId(courseId)
      .pipe(catchError(() => of([] as Enrollment[])))
      .subscribe(enrs => {
        const ids = Array.from(new Set(
          (enrs || [])
            .map(e => e.teacherCourseId)
            .filter((x): x is number => x != null)
        ));
        if (!ids.length) {
          this.teacherCourses = [];
          return;
        }
        forkJoin(
          ids.map(id =>
            this.teacherCourseService.getTeacherCourseById(id)
              .pipe(catchError(() => of(null as unknown as TeacherCourse)))
          )
        ).subscribe(list => {
          this.teacherCourses = (list || [])
            .filter((tc): tc is TeacherCourse => !!tc)
            .map(tc => ({
              ...tc,
              teacherId: tc.teacherId || tc.teacher?.teacherId || ''
            }));
        });
      });
  }

  private loadEnrollmentData(id: number): void {
    this.isLoading = true;
    this.enrollmentService.getEnrollmentById(id).subscribe({
      next: (data: Enrollment) => {
        // Khi edit, cần nạp teacherCourses của course hiện có để hiển thị đúng dropdown
        if (data.courseId) this.loadTeacherCourses(data.courseId);
        this.enrollmentForm.patchValue({
          studentId: data.studentId,
          courseId: data.courseId,
          teacherCourseId: data.teacherCourseId ?? null,
          semesterId: data.semesterId,
          status: data.status
        });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Could not load enrollment data.';
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.enrollmentForm.invalid) {
      this.enrollmentForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    const payload = this.enrollmentForm.getRawValue(); // lấy cả các control disabled khi edit

    const op$ = this.isEditMode && this.enrollmentId
      ? this.enrollmentService.updateEnrollment(this.enrollmentId, { ...payload, enrollmentId: this.enrollmentId })
      : this.enrollmentService.createEnrollment(payload);

    op$.subscribe({
      next: () => this.router.navigate(['/admin/enrollments']),
      error: (err) => {
        this.errorMessage = `An error occurred. ${err?.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
