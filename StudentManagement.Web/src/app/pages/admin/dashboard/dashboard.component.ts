import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

// Đổi đường dẫn nếu dự án bạn khác
import { UserService } from '../../../services/user.service';
import { StudentService } from '../../../services/student.service';
import { TeacherService } from '../../../services/teacher.service';
import { CourseService } from '../../../services/course.service';

import { AnalyticsService, StudentAvg } from '../../../services/analytics.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule]
})
export class DashboardComponent implements OnInit {
  // Số liệu tổng quan
  stats = { users: 0, students: 0, teachers: 0, courses: 0 };

  // Top sinh viên
  topStudents: StudentAvg[] = [];
  topTotal = 0;
  topPage = 1;
  topSize = 5;
  topLoading = false;
  topError: string | null = null;

  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private userService: UserService,
    private studentService: StudentService,
    private teacherService: TeacherService,
    private courseService: CourseService,
    private analyticsService: AnalyticsService
  ) {}

  ngOnInit(): void {
    // Tách call để nếu Top lỗi thì counters vẫn hiện
    this.loadCounters();
    this.loadTop(this.topPage);
  }

  /** Tổng số trang cho template (tránh gọi Math trong HTML) */
  get totalPages(): number {
    if (!this.topSize) return 1;
    const pages = Math.ceil(this.topTotal / this.topSize);
    return pages > 0 ? pages : 1;
  }

  loadCounters(): void {
    this.isLoading = true;
    this.errorMessage = null;

    forkJoin({
      users: this.userService.getAllUsers().pipe(map((d: any[]) => d.length)),
      students: this.studentService.getAllStudents().pipe(map((d: any[]) => d.length)),
      teachers: this.teacherService.getAllTeachers().pipe(map((d: any[]) => d.length)),
      courses: this.courseService.getAllCourses().pipe(map((d: any[]) => d.length)),
    }).subscribe({
      next: (res) => {
        this.stats = {
          users: res.users,
          students: res.students,
          teachers: res.teachers,
          courses: res.courses
        };
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Dashboard counters error', err);
        this.errorMessage = 'Không thể tải dữ liệu thống kê.';
        this.isLoading = false;
      }
    });
  }

  loadTop(page = 1): void {
    this.topLoading = true;
    this.topError = null;

    this.analyticsService.getTopStudents(page, this.topSize).subscribe({
      next: (res) => {
        this.applyTop(res);
        this.topLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Top students error', err);
        this.topError = 'Không thể tải Top sinh viên.';
        this.topLoading = false;
      }
    });
  }

  private applyTop(res: { data: StudentAvg[]; totalCount: number; page: number; pageSize: number }): void {
    this.topStudents = res.data;
    this.topTotal = res.totalCount;
    this.topPage = res.page;
    this.topSize = res.pageSize;
  }

  canPrev(): boolean { return this.topPage > 1; }
  canNext(): boolean { return this.topPage < this.totalPages; }
  goPrev(): void { if (this.canPrev()) this.loadTop(this.topPage - 1); }
  goNext(): void { if (this.canNext()) this.loadTop(this.topPage + 1); }
}
