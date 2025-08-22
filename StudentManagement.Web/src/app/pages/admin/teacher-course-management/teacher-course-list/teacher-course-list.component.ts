import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { TeacherCourse } from '../../../../models/teacher-course.model';
import { Semester } from '../../../../models/Semester.model';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';
import { FormsModule } from '@angular/forms'; 

@Component({
  selector: 'app-teacher-course-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule], // ⬅️ thêm FormsModule
  templateUrl: './teacher-course-list.component.html',
  styleUrls: ['./teacher-course-list.component.scss']
})
export class TeacherCourseListComponent implements OnInit {
  // Dữ liệu đầy đủ + dữ liệu hiển thị theo trang (client-side paging như Student)
  allAssignments: TeacherCourse[] = [];
  assignments: TeacherCourse[] = [];

  // Phân trang
  pageSize = 10;
  currentPage = 1;

  isLoading = false;
  errorMessage: string | null = null;

  semName: Record<number, string> = {};
  semYear: Record<number, string> = {};

  constructor(
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.loadAssignments();
  }

  // Tổng số bản ghi
  get totalCount(): number {
    return this.allAssignments.length;
  }

  // Tổng số trang
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  // Mảng số trang cho *ngFor
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // Lấy nhiều bản ghi (chunk lớn) rồi phân trang ở FE – giống Student
    this.teacherCourseService.getPagedTeacherCourses(1, 1000).subscribe({
      next: (resp: any) => {
        // Service có thể trả { tcs, totalCount } hoặc { items } / { data }
        const list: TeacherCourse[] = resp?.tcs ?? resp?.items ?? resp?.data ?? [];
        this.allAssignments = list || [];

        // Reset về trang 1 và cắt trang
        this.currentPage = 1;
        this.applyPagination();

        // Preload thông tin học kỳ (cache theo ID) — dùng toàn bộ danh sách để tránh gọi vặt
        const semIds = Array.from(
          new Set(this.allAssignments.map(a => a.semesterId).filter((x): x is number => x != null))
        );
        if (!semIds.length) { this.isLoading = false; return; }

        forkJoin(
          semIds.map(id =>
            this.semesterService.getSemesterById(id).pipe(catchError(() => of(null as unknown as Semester)))
          )
        ).subscribe({
          next: (listSem) => {
            for (const s of listSem) {
              if (s && s.semesterId != null) {
                this.semName[s.semesterId] = s.semesterName ?? 'N/A';
                this.semYear[s.semesterId] = s.academicYear ?? 'N/A';
              }
            }
            this.isLoading = false;
          },
          error: () => { this.isLoading = false; }
        });
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải danh sách phân công.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  /** Cắt mảng theo trang hiện tại */
  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.assignments = this.allAssignments.slice(start, end);
  }

  /** Điều hướng trang */
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }
  prev(): void { this.goToPage(this.currentPage - 1); }
  next(): void { this.goToPage(this.currentPage + 1); }

  /** Đổi số dòng mỗi trang */
  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.applyPagination();
  }

  trackById = (_: number, a: TeacherCourse) => a.teacherCourseId;
}
