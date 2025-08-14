import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { switchMap, map, catchError } from 'rxjs/operators';

import { TeacherCourse, Score } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { ScoreService } from '../../../services/score.service';
import { AuthService } from '../../../services/auth.service';
import { CourseService } from '../../../services/course.service';
import { SemesterService } from '../../../services/semester.service';
import { Semester } from '../../../models/Semester.model';

export interface CourseStats {
  courseId: string;
  courseName: string;
  semester: string;
  academicYear: string;
  totalStudents: number;
  passedStudents: number;
  failedStudents: number;
}

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss']
})
export class TeacherDashboardComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;
  teacherId: string | null = null;

  totalAssignedCourses = 0;
  totalStudents = 0;
  totalPassed = 0;
  totalFailed = 0;

  courseStatistics: CourseStats[] = [];

  constructor(
    private teacherCourseService: TeacherCourseService,
    private scoreService: ScoreService,
    private authService: AuthService,
    private courseService: CourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.teacherId = tokenPayload?.teacherId
      || tokenPayload?.sub || tokenPayload?.userId || tokenPayload?.id;

    if (!this.teacherId) {
      this.errorMessage = 'Không thể tải dữ liệu Bảng điều khiển.';
      return;
    }
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).pipe(
      switchMap(assignments => {
        this.totalAssignedCourses = assignments.length;
        if (assignments.length === 0) {
          return of({ results: [], courses: [], semesters: [] });
        }

        // id duy nhất để lookup
        const courseIds = Array.from(new Set(assignments.map(a => a.courseId).filter(Boolean))) as string[];
        const semesterIds = Array.from(new Set(assignments.map(a => a.semesterId).filter((x): x is number => x != null)));

        // 1) Điểm từng lớp
        const results$ = forkJoin(
          assignments.map(a =>
            this.scoreService.getByTeacherAndSubject(a.teacherId, a.courseId).pipe(
              map(scores => ({ assignment: a, scores } as { assignment: TeacherCourse; scores: Score[] }))
            )
          )
        );

        // 2) Courses (lấy tất cả rồi lọc theo courseIds cho đơn giản)
        const courses$ = this.courseService.getAllCourses().pipe(
          map(all => (all || []).filter((c: any) => courseIds.includes(c.courseId))),
          catchError(() => of([] as any[]))
        );

        // 3) Semesters theo id
        const semesters$ = semesterIds.length
          ? forkJoin(semesterIds.map(id =>
              this.semesterService.getSemesterById(id).pipe(catchError(() => of(null as unknown as Semester)))
            ))
          : of([] as (Semester | null)[]);

        return forkJoin({ results: results$, courses: courses$, semesters: semesters$ });
      })
    ).subscribe({
      next: ({ results, courses, semesters }) => {
        // Map nhanh
        const courseMap = new Map<string, any>((courses || []).map((c: any) => [c.courseId, c]));
        const semMap = new Map<number, Semester>();
        (semesters || []).forEach(s => { if (s?.semesterId != null) semMap.set(s.semesterId, s); });

        // Tính thống kê + gắn tên môn/học kỳ/năm học
        let totalStudents = 0, totalPassed = 0, totalFailed = 0;
        const stats: CourseStats[] = (results || []).map((r: { assignment: TeacherCourse; scores: Score[] }) => {
          const a = r.assignment;
          const passed = r.scores.filter(s => s.isPassed === true).length;
          const failed = r.scores.filter(s => s.isPassed === false).length;

          const courseName = courseMap.get(a.courseId)?.courseName || 'N/A';
          const s = a.semesterId != null ? semMap.get(a.semesterId) : undefined;

          totalStudents += r.scores.length;
          totalPassed += passed;
          totalFailed += failed;

          return {
            courseId: a.courseId,
            courseName,
            semester: s?.semesterName ?? 'N/A',
            academicYear: s?.academicYear ?? 'N/A',
            totalStudents: r.scores.length,
            passedStudents: passed,
            failedStudents: failed
          };
        });

        this.courseStatistics = stats;
        this.totalStudents = totalStudents;
        this.totalPassed = totalPassed;
        this.totalFailed = totalFailed;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải dữ liệu thống kê.';
        console.error(err);
        this.isLoading = false;
      }
    });
  }
}
