import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable,forkJoin, of } from 'rxjs';

import { Enrollment } from '../../../models';
import { EnrollmentService } from '../../../services/enrollment.service';
import { SemesterService } from '../../../services/semester.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrls: ['./student-dashboard.component.scss']
})
export class StudentDashboardComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;
  studentId: string | null = null;

  // Thống kê
  totalCourses = 0;
  completedCourses = 0;
  averageScore: number | string = 'Chưa có';

  // Bảng hoạt động
  recentEnrollments: Enrollment[] = [];

  // cache học kỳ theo id để tránh gọi lại
  private semesterCache = new Map<string | number, any>();

  constructor(
    private enrollmentService: EnrollmentService,
    private semesterService: SemesterService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.studentId =
      tokenPayload?.studentId ??
      tokenPayload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
      tokenPayload?.sub ??
      null;

    if (this.studentId) {
      this.loadDashboardData();
    } else {
      this.errorMessage = 'Không thể xác thực thông tin sinh viên.';
    }
  }

  // ---- Helpers -------------------------------------------------------------

  private asNumber(v: any): number | null {
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }

  private isCompletedStatus(v: any): boolean {
    if (!v) return false;
    const s = String(v).toLowerCase();

    return (
      s.includes('complete') ||
      s.includes('hoàn thành') ||
      s === 'passed' ||
      s === 'đạt'
    );
  }
  private fillMissingSemesters(enrollments: any[]): Observable<any[] | null> {
    const rawIds: Array<string | number> = [];

    for (const e of enrollments) {
      const sid =
        e?.semester?.semesterId ??
        e?.semesterId ??
        e?.teacherCourse?.semesterId;

      if (sid !== undefined && sid !== null && !this.semesterCache.has(sid)) {
        // đánh dấu đã thấy id này để không push trùng
        this.semesterCache.set(sid, undefined);
        rawIds.push(sid);
      }
    }

    if (rawIds.length === 0) {

      return of(null);
    }

    // CHỈNH ở đây: ép về number + loại NaN
    const ids: number[] = rawIds
      .map((v) => Number(v))
      .filter((n) => Number.isFinite(n));

    const calls = ids.map((id) => this.semesterService.getSemesterById(id));
    return forkJoin(calls);
  }

  // ---- Load & process ------------------------------------------------------

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.enrollmentService
      .getStudentEnrollmentsWithScores(this.studentId!)
      .subscribe({
        next: (enrollments) => {
          // bổ sung học kỳ còn thiếu (nếu có)
          this.fillMissingSemesters(enrollments).subscribe({
            next: (semesters: any[] | null) => {
              if (semesters) {
                // lưu cache
                for (const s of semesters) {
                  if (s?.semesterId) this.semesterCache.set(s.semesterId, s);
                }
                // gán lại vào từng enrollment
                for (const e of enrollments) {
                  if (!e.semester) {
                    const sid =
                      e?.semesterId ?? e?.teacherCourse?.semesterId ?? null;
                    if (sid && this.semesterCache.get(sid)) {
                      e.semester = this.semesterCache.get(sid);
                    }
                  }
                }
              }
              // xử lý dữ liệu
              this.processEnrollmentData(enrollments);
              this.isLoading = false;
            },
            error: () => {
              // nếu lỗi khi load semester thì vẫn hiển thị phần còn lại
              this.processEnrollmentData(enrollments);
              this.isLoading = false;
            }
          });
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi tải dữ liệu.';
          this.isLoading = false;
          console.error(err);
        }
      });
  }

  private processEnrollmentData(enrollments: any[]): void {
    // Tổng số môn
    this.totalCourses = enrollments.length;

    let totalScoreTimesCredits = 0;
    let totalCredits = 0;
    let finished = 0;

    for (const e of enrollments) {
      // trạng thái hoàn thành
      const completed =
        this.isCompletedStatus(e?.status) ||
        this.isCompletedStatus(e?.enrollmentStatus) ||
        // nếu có totalScore (>= 0) coi như đã chấm xong
        this.asNumber(
          e?.totalScore ??
            e?.score?.totalScore ??
            e?.scores?.[0]?.totalScore ??
            e?.studentScore?.totalScore
        ) !== null;

      if (completed) finished++;

      // điểm
      const score =
        this.asNumber(e?.totalScore) ??
        this.asNumber(e?.score?.totalScore) ??
        this.asNumber(e?.scores?.[0]?.totalScore) ??
        this.asNumber(e?.studentScore?.totalScore);

      // tín chỉ
      const credits =
        this.asNumber(e?.credits) ??
        this.asNumber(e?.course?.credits) ??
        this.asNumber(e?.teacherCourse?.course?.credits);

      if (completed && score !== null && credits !== null) {
        totalScoreTimesCredits += score * credits;
        totalCredits += credits;
      }


      e.course = e.course ?? e.teacherCourse?.course ?? e.enrollmentCourse ?? e.course;
      e.teacherCourse = e.teacherCourse ?? {};
      e.teacherCourse.teacher = e.teacherCourse.teacher ?? e.teacher ?? null;

      if (!e.semester) {
        const name = e.semesterName ?? '';
        const year = e.academicYear ?? e.year ?? '';
        e.semester = { semesterName: name || '—', academicYear: year || '—' };
      }
      // status text
      e.status =
        e.status ??
        e.enrollmentStatus ??
        (completed ? 'Completed' : 'InProgress');
    }

    this.completedCourses = finished;

    if (totalCredits > 0) {
      this.averageScore = +(totalScoreTimesCredits / totalCredits).toFixed(2);
    } else {
      this.averageScore = 'Chưa có';
    }

    const normSemIndex = (name: string) => {
      const s = (name || '').toLowerCase();
      if (s.includes('1') || s.includes('i')) return 1;
      if (s.includes('2') || s.includes('ii')) return 2;
      if (s.includes('3') || s.includes('iii')) return 3;
      return 0;
    };

    this.recentEnrollments = [...enrollments]
      .sort((a, b) => {
        const ayA = a.semester?.academicYear ?? a.academicYear ?? 0;
        const ayB = b.semester?.academicYear ?? b.academicYear ?? 0;
        if (ayA !== ayB) return (ayB as any) - (ayA as any);

        const sA = normSemIndex(a.semester?.semesterName ?? a.semesterName ?? '');
        const sB = normSemIndex(b.semester?.semesterName ?? b.semesterName ?? '');
        return sB - sA;
      })
      .slice(0, 10);

  }
}
