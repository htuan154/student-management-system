import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { TeacherCourse } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { AuthService } from '../../../services/auth.service';
import { SemesterService } from '../../../services/semester.service';
import { CourseService } from '../../../services/course.service';
import { Semester } from '../../../models/Semester.model';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-courses.component.html',
  styleUrls: ['./my-courses.component.scss']
})
export class MyCoursesComponent implements OnInit {
  assignedCourses: TeacherCourse[] = [];

  // key = teacherCourseId
  tcSemesterName: Record<number, string> = {};
  tcAcademicYear: Record<number, string> = {};

  isLoading = false;
  errorMessage: string | null = null;
  teacherId: string | null = null;

  constructor(
    private teacherCourseService: TeacherCourseService,
    private authService: AuthService,
    private router: Router,
    private semesterService: SemesterService,
    private courseService: CourseService
  ) {}

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.teacherId = tokenPayload?.teacherId
      || tokenPayload?.sub
      || tokenPayload?.userId
      || tokenPayload?.id
      || tokenPayload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    if (!this.teacherId) {
      this.errorMessage = 'Không thể xác định thông tin giảng viên.';
      return;
    }
    this.loadAssignedCourses();
  }

  private loadAssignedCourses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).subscribe({
      next: (tcs) => {
        this.assignedCourses = tcs || [];
        this.hydrateCoursesAndSemesters(this.assignedCourses);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải danh sách môn học.';
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  /** Bổ sung dữ liệu Course (theo courseId) và Semester (theo semesterId) */
  private hydrateCoursesAndSemesters(tcs: TeacherCourse[]): void {
    if (!tcs?.length) { this.isLoading = false; return; }

    const courseIds = Array.from(new Set(tcs.map(tc => tc.courseId).filter((x): x is string => !!x)));
    const semesterIds = Array.from(new Set(tcs.map(tc => tc.semesterId).filter((x): x is number => x != null)));

    forkJoin({
      // Lấy toàn bộ course 1 lần rồi map theo id cho nhanh/gọn
      courses: this.courseService.getAllCourses().pipe(
        map(all => (all || []).filter(c => courseIds.includes(c.courseId))),
        catchError(() => of([] as any[]))
      ),
      semesters: semesterIds.length
        ? forkJoin(semesterIds.map(id =>
            this.semesterService.getSemesterById(id).pipe(catchError(() => of(null as unknown as Semester)))
          ))
        : of([] as (Semester | null)[])
    }).subscribe({
      next: ({ courses, semesters }) => {
        const courseMap = new Map<string, any>((courses || []).map(c => [c.courseId, c]));
        const semMap = new Map<number, Semester>();
        (semesters || []).forEach(s => { if (s?.semesterId != null) semMap.set(s.semesterId, s); });

        // gắn course object để HTML không bị N/A
        this.assignedCourses = tcs.map(tc => ({
          ...tc,
          course: tc.course || courseMap.get(tc.courseId) || null
        }));

        // điền tên kỳ & năm học vào 2 map để render
        this.tcSemesterName = {};
        this.tcAcademicYear = {};
        for (const tc of this.assignedCourses) {
          const s = tc.semesterId != null ? semMap.get(tc.semesterId) : undefined;
          this.tcSemesterName[tc.teacherCourseId!] = s?.semesterName ?? 'N/A';
          this.tcAcademicYear[tc.teacherCourseId!] = s?.academicYear ?? 'N/A';
        }

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể bổ sung dữ liệu môn học/học kỳ.';
        this.isLoading = false;
      }
    });
  }

  /** Điều hướng danh sách SV của 1 môn (nếu bạn dùng) */
  viewClassList(courseId: string): void {
    this.router.navigate(['/teacher/class-list', courseId, this.teacherId]);
  }
}
