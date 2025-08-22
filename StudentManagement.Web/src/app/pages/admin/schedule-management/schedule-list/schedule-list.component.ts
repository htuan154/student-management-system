import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ScheduleService } from '../../../../services/schedule.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { CourseService } from '../../../../services/course.service';
import { TeacherService } from '../../../../services/teacher.service';

import { Schedule } from '../../../../models/Schedule.model';
import { TeacherCourse } from '../../../../models/teacher-course.model';
import { of, forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FormsModule } from '@angular/forms'; // ⬅️ dùng ngModel

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './schedule-list.component.html',
  styleUrls: ['./schedule-list.component.scss']
})
export class ScheduleListComponent implements OnInit {
  allSchedules: Schedule[] = [];   // toàn bộ dữ liệu
  schedules: Schedule[] = [];      // dữ liệu hiển thị theo trang

  // phân trang
  currentPage = 1;
  pageSize = 10;

  isLoading = false;
  errorMessage: string | null = null;

  // Map hiển thị thứ
  dayOfWeekMap: Record<number, string> = {
    2: 'Thứ 2', 3: 'Thứ 3', 4: 'Thứ 4', 5: 'Thứ 5', 6: 'Thứ 6', 7: 'Thứ 7', 8: 'Chủ nhật'
  };

  constructor(
    private scheduleService: ScheduleService,
    private teacherCourseService: TeacherCourseService,
    private courseService: CourseService,
    private teacherService: TeacherService
  ) {}

  ngOnInit(): void {
    this.loadSchedules();
  }

  get totalCount(): number {
    return this.allSchedules.length;
  }
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.schedules = this.allSchedules.slice(start, end);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }
  prev(): void { this.goToPage(this.currentPage - 1); }
  next(): void { this.goToPage(this.currentPage + 1); }
  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.applyPagination();
  }

  /** Tải lịch + join TeacherCourse + fallback lấy Course/Teacher nếu thiếu */
  private loadSchedules(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scheduleService.getAllSchedules().subscribe({
      next: (data) => {
        const source = (data || []) as Schedule[];

        const tcIds = Array.from(
          new Set(source.map(s => s.teacherCourseId).filter((x): x is number => x != null))
        );

        if (tcIds.length === 0) {
          this.allSchedules = source;
          this.currentPage = 1;
          this.applyPagination();
          this.isLoading = false;
          return;
        }

        forkJoin(
          tcIds.map(id =>
            this.teacherCourseService.getTeacherCourseById(id)
              .pipe(catchError(() => of(null as unknown as TeacherCourse)))
          )
        ).subscribe({
          next: (tcList) => {
            const tcMap = new Map<number, TeacherCourse>();
            (tcList || []).forEach(tc => { if (tc && tc.teacherCourseId != null) tcMap.set(tc.teacherCourseId, tc); });

            forkJoin({
              courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
              teachers: this.teacherService.getAllTeachers().pipe(catchError(() => of([])))
            }).subscribe(({ courses, teachers }) => {
              const courseMap = new Map<string, any>((courses || []).map(c => [c.courseId, c]));
              const teacherMap = new Map<string, any>((teachers || []).map(t => [t.teacherId, t]));

              tcMap.forEach((tc) => {
                if (tc && !tc.course && tc.courseId)  (tc as any).course  = courseMap.get(tc.courseId)  || null;
                if (tc && !tc.teacher && tc.teacherId) (tc as any).teacher = teacherMap.get(tc.teacherId) || null;
              });

              this.allSchedules = source.map(s => ({
                ...s,
                teacherCourse: s.teacherCourseId != null ? (tcMap.get(s.teacherCourseId) || null) : null
              })) as Schedule[];

              this.currentPage = 1;
              this.applyPagination();
              this.isLoading = false;
            });
          },
          error: (e) => {
            console.error(e);
            this.errorMessage = 'Không thể tải dữ liệu phân công để hiển thị môn/giảng viên.';
            this.isLoading = false;
          }
        });
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Không thể tải danh sách lịch học.';
        this.isLoading = false;
      }
    });
  }

  fmt(t?: string): string {
    return (t || '').slice(0, 5);
  }

  deleteSchedule(id: number): void {
    if (!confirm('Bạn có chắc chắn muốn xóa lịch học này?')) return;
    this.scheduleService.deleteSchedule(id).subscribe({
      next: () => this.loadSchedules(),
      error: (err) => {
        console.error('Lỗi khi xóa lịch học:', err);
        alert('Có lỗi xảy ra khi xóa lịch học.');
      }
    });
  }

  trackById = (_: number, s: Schedule) => s.scheduleId;
}
