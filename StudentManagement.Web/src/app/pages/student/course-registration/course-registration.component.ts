import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { Course, Enrollment, TeacherCourse } from '../../../models';
import { CourseService } from '../../../services/course.service';
import { EnrollmentService } from '../../../services/enrollment.service';
import { SemesterService, StudentSemesterSummary } from '../../../services/semester.service';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-course-registration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-registration.component.html',
  styleUrls: ['./course-registration.component.scss']
})
export class CourseRegistrationComponent implements OnInit {
  availableCourses: Course[] = [];
  myEnrollments: Enrollment[] = [];
  teacherCoursesInSem: TeacherCourse[] = [];

  semesters: StudentSemesterSummary[] = [];
  selectedSemesterId: number | null = null;

  studentId: string | null = null;

  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private semesterService: SemesterService,
    private teacherCourseService: TeacherCourseService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken?.();
    this.studentId = tokenPayload?.studentId ?? tokenPayload?.sub ?? null;

    if (!this.studentId) {
      this.errorMessage = 'Không thể xác định thông tin sinh viên.';
      return;
    }

    this.isLoading = true;

    // Chỉ tải danh sách kỳ học; KHÔNG tự chọn kỳ
    this.semesterService.getActiveSemesters().pipe(
      map(sems =>
        sems.map(
          s =>
            ({
              semesterId: s.semesterId,
              semesterName: s.semesterName,
              academicYear: s.academicYear,
              startDate: s.startDate,
              endDate: s.endDate,
              isActive: s.isActive,
              courseCount: s.teacherCourses?.length ?? 0
            }) as StudentSemesterSummary
        )
      ),
      map((summaries: StudentSemesterSummary[]) => {
        this.semesters = summaries.sort((a, b) => (a.startDate || '').localeCompare(b.startDate || ''));
        // Mặc định không chọn kỳ
        this.selectedSemesterId = null;

        // Dọn dữ liệu bảng
        this.availableCourses = [];
        this.myEnrollments = [];
        this.teacherCoursesInSem = [];
        this.isLoading = false;
        return null;
      })
    ).subscribe({
      error: (err: HttpErrorResponse) => {
        console.error(err);
        this.errorMessage = 'Không tải được dữ liệu kỳ học.';
        this.isLoading = false;
      }
    });
  }

  onSemesterChange(value: string) {
    this.selectedSemesterId = value ? +value : null;
    this.successMessage = null;
    this.errorMessage = null;

    // Chỉ load khi đã có selectedSemesterId
    this.loadInitialData().subscribe();
  }

  private loadInitialData() {
    if (!this.studentId || !this.selectedSemesterId) {
      this.availableCourses = [];
      this.myEnrollments = [];
      this.teacherCoursesInSem = [];
      return of(null);
    }

    this.isLoading = true;
    this.errorMessage = null;

    return forkJoin({
      courses: this.courseService.getAllCourses(),
      enrollments: this.enrollmentService.getEnrollmentsByStudentId(this.studentId!),
      tcs: this.teacherCourseService.getTeacherCoursesBySemesterId(this.selectedSemesterId)
    }).pipe(
      map(({ courses, enrollments, tcs }) => {
        this.teacherCoursesInSem = tcs;
        this.myEnrollments = enrollments.filter(e => e.semesterId === this.selectedSemesterId);

        const tcCourseIds = new Set(tcs.map(x => x.courseId));
        this.availableCourses = courses.filter(c => tcCourseIds.has(c.courseId));

        this.isLoading = false;
        return null;
      })
    );
  }

  isAlreadyEnrolled(courseId: string): boolean {
    return this.myEnrollments.some(e => e.courseId === courseId);
  }

  registerForCourse(courseId: string): void {
    if (!this.studentId || !this.selectedSemesterId) {
      this.errorMessage = 'Vui lòng chọn kỳ học trước khi đăng ký.';
      return;
    }

    const teacherCourseId = this.findTeacherCourseId(courseId, this.selectedSemesterId);
    if (!teacherCourseId) {
      this.errorMessage = 'Môn này chưa được phân công trong kỳ đã chọn.';
      return;
    }

    const enrollmentData = {
      studentId: this.studentId,
      courseId,
      semesterId: this.selectedSemesterId,
      teacherCourseId,
      status: 'Enrolled'
    };

    this.enrollmentService.createEnrollment(enrollmentData).subscribe({
      next: () => {
        this.successMessage = `Đăng ký môn ${courseId} thành công!`;
        this.loadInitialData().subscribe();
        setTimeout(() => (this.successMessage = null), 3000);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = `Lỗi khi đăng ký: ${err.error?.message || 'Vui lòng thử lại.'}`;
        console.error(err);
      }
    });
  }

  private findTeacherCourseId(courseId: string, semesterId: number): number | null {
    const tc = this.teacherCoursesInSem.find(x => x.courseId === courseId && x.semesterId === semesterId);
    return tc?.teacherCourseId ?? null;
  }

  hasTeacherCourseInSemester(courseId: string, semesterId: number): boolean {
    return this.teacherCoursesInSem.some(tc => tc.courseId === courseId && tc.semesterId === semesterId);
  }

  trackBySemesterId = (_: number, s: StudentSemesterSummary) => s.semesterId;
  trackByCourseId   = (_: number, c: Course) => c.courseId;
}
