import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { Enrollment, TeacherCourse, Semester } from '../../../../models';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';

import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

type PagedEnrollmentResponse = {
  enrollments: Enrollment[];
  totalCount: number;
};

@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule], // ⬅️ FormsModule để dùng ngModel
  templateUrl: './enrollment-list.component.html',
  styleUrls: ['./enrollment-list.component.scss']
})
export class EnrollmentListComponent implements OnInit {
  // Dữ liệu trang hiện tại (server-side pagination)
  enrollments: Enrollment[] = [];

  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  // Cache hiển thị liên quan (giáo viên, học kỳ)
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

  // Tổng số trang cho pager
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  // Tạo mảng số trang [1..N] cho *ngFor
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  // Tải 1 trang từ server (giữ nguyên service hiện có)
  loadEnrollments(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // Nếu service của bạn có thêm tham số tìm kiếm/sắp xếp, truyền thêm nếu cần
    this.enrollmentService.getPagedEnrollments(this.currentPage, this.pageSize /*, search, sortBy, sortOrder */)
      .subscribe({
        next: (res: PagedEnrollmentResponse) => {
          this.enrollments = res?.enrollments ?? [];
          this.totalCount = res?.totalCount ?? 0;
          this.isLoading = false;

          // Nạp dữ liệu liên quan cho trang hiện tại (có cache)
          this.hydrateRelated(this.enrollments);
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Could not load the list of enrollments.';
          this.isLoading = false;
          console.error(err);
        }
      });
  }

  // Điều hướng trang
  goToPage(page: number): void {
    const last = this.totalPages;
    if (page < 1 || page > last) return;
    this.currentPage = page;
    this.loadEnrollments();
  }
  prev(): void { this.goToPage(this.currentPage - 1); }
  next(): void { this.goToPage(this.currentPage + 1); }

  // Đổi số dòng mỗi trang
  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1; // reset về trang 1
    this.loadEnrollments();
  }

  // Xoá 1 đăng ký
  deleteEnrollment(id: number): void {
    if (!confirm('Are you sure you want to delete this enrollment?')) return;
    this.isLoading = true;
    this.enrollmentService.deleteEnrollment(id).subscribe({
      next: () => {
        // Sau khi xoá, nếu trang hiện tại rỗng thì lùi về trang trước (nếu có)
        const maxBefore = this.totalPages;
        // Tải lại trang
        this.loadEnrollments();
        // Nếu sau tải lại mà currentPage > totalPages (trường hợp xoá cuối), lùi về
        setTimeout(() => {
          const maxAfter = this.totalPages;
          if (this.currentPage > maxAfter) {
            this.currentPage = Math.max(1, maxAfter);
            this.loadEnrollments();
          }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage = 'An error occurred while deleting the enrollment.';
        console.error(err);
      }
    });
  }

  // Nạp tên GV / thông tin học kỳ cho bản ghi của TRANG hiện tại (có cache để tránh gọi lại)
  private hydrateRelated(list: Enrollment[]) {
    const tcIds = Array.from(
      new Set(list.map(e => e.teacherCourseId).filter((x): x is number => x != null))
    ).filter(id => this.teacherNames[id] == null);

    const semIds = Array.from(
      new Set(list.map(e => e.semesterId).filter((x): x is number => x != null))
    ).filter(id => this.semesterNames[id] == null);

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
        for (const tc of tcs) {
          if (tc && tc.teacherCourseId != null) {
            const name =
              tc.teacher?.fullName ||
              (tc.teacher as any)?.teacherId ||
              (tc as any).teacherId ||
              `TC#${tc.teacherCourseId}`;
            this.teacherNames[tc.teacherCourseId] = String(name);
          }
        }
        for (const sem of sems) {
          if (sem && sem.semesterId != null) {
            this.semesterNames[sem.semesterId] = {
              name: sem.semesterName || `HK#${sem.semesterId}`,
              year: sem.academicYear || 'N/A'
            };
          }
        }
      },
      error: (err) => console.error(err)
    });
  }
}
