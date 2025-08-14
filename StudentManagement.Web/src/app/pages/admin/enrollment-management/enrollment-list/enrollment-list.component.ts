import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { Enrollment, TeacherCourse, Semester } from '../../../../models';
import { EnrollmentService, PagedEnrollmentResponse } from '../../../../services/enrollment.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';

@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './enrollment-list.component.html',
  styleUrls: ['./enrollment-list.component.scss']
})
export class EnrollmentListComponent implements OnInit {
  enrollments: Enrollment[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // Maps để hiển thị UI
  teacherNames: Record<number, string> = {};
  semesterNames: Record<number, { name: string; year: string }> = {};

  constructor(
    private enrollmentService: EnrollmentService,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.enrollmentService.getPagedEnrollments(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedEnrollmentResponse) => {
        this.enrollments = response.enrollments || [];
        this.totalCount = response.totalCount || 0;
        this.hydrateRelated(this.enrollments);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of enrollments.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  private hydrateRelated(list: Enrollment[]) {
    const tcIds = Array.from(new Set(list.map(e => e.teacherCourseId).filter((x): x is number => x != null)));
    const semIds = Array.from(new Set(list.map(e => e.semesterId).filter((x): x is number => x != null)));

    // Observable trả về MẢNG dữ liệu (không phải mảng Observable)
    const tc$ = tcIds.length
      ? forkJoin(
          tcIds.map(id =>
            this.teacherCourseService
              .getTeacherCourseById(id)
              .pipe(catchError(() => of<TeacherCourse | null>(null)))
          )
        )
      : of<(TeacherCourse | null)[]>([]);

    const sem$ = semIds.length
      ? forkJoin(
          semIds.map(id =>
            this.semesterService
              .getSemesterById(id)
              .pipe(catchError(() => of<Semester | null>(null)))
          )
        )
      : of<(Semester | null)[]>([]);

    forkJoin({ tcs: tc$, sems: sem$ }).subscribe({
      next: ({ tcs, sems }) => {
        // Map teacher names
        for (const tc of tcs) {
          if (tc && tc.teacherCourseId != null) {
            const name =
              tc.teacher?.fullName ||
              tc.teacher?.teacherId ||
              tc.teacherId ||                 // fallback khi API trả phẳng teacherId
              `TC#${tc.teacherCourseId}`;
            this.teacherNames[tc.teacherCourseId] = name;
          }
        }
        // Map semester info
        for (const sem of sems) {
          if (sem && sem.semesterId != null) {
            this.semesterNames[sem.semesterId] = {
              name: sem.semesterName || `HK#${sem.semesterId}`,
              year: sem.academicYear || 'N/A'
            };
          }
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  deleteEnrollment(id: number): void {
    if (confirm('Are you sure you want to delete this enrollment?')) {
      this.enrollmentService.deleteEnrollment(id).subscribe({
        next: () => this.loadEnrollments(),
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the enrollment.';
          console.error(err);
        }
      });
    }
  }
}
