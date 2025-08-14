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

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './schedule-list.component.html',
  styleUrls: ['./schedule-list.component.scss']
})
export class ScheduleListComponent implements OnInit {
  schedules: Schedule[] = [];
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

  /** Tải lịch + join TeacherCourse + fallback lấy Course/Teacher nếu thiếu */
  private loadSchedules(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scheduleService.getAllSchedules().subscribe({
      next: (data) => {
        const schedules = (data || []) as Schedule[];

        // Lấy danh sách teacherCourseId duy nhất
        const tcIds = Array.from(
          new Set(schedules.map(s => s.teacherCourseId).filter((x): x is number => x != null))
        );

        if (tcIds.length === 0) {
          this.schedules = schedules;
          this.isLoading = false;
          return;
        }

        // 1) Lấy TeacherCourse theo từng ID (tránh 405 ở GET /TeacherCourse)
        forkJoin(
          tcIds.map(id =>
            this.teacherCourseService.getTeacherCourseById(id)
              .pipe(catchError(() => of(null as unknown as TeacherCourse)))
          )
        ).subscribe({
          next: (tcList) => {
            const tcMap = new Map<number, TeacherCourse>();
            (tcList || []).forEach(tc => { if (tc && tc.teacherCourseId != null) tcMap.set(tc.teacherCourseId, tc); });

            // 2) Fallback: nếu thiếu course/teacher -> lấy toàn bộ 1 lượt rồi map
            forkJoin({
              courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
              teachers: this.teacherService.getAllTeachers().pipe(catchError(() => of([])))
            }).subscribe(({ courses, teachers }) => {
              const courseMap = new Map<string, any>((courses || []).map(c => [c.courseId, c]));
              const teacherMap = new Map<string, any>((teachers || []).map(t => [t.teacherId, t]));

              // Bổ sung navigation nếu BE không trả
              tcMap.forEach((tc) => {
                if (tc && !tc.course && tc.courseId)  (tc as any).course  = courseMap.get(tc.courseId)  || null;
                if (tc && !tc.teacher && tc.teacherId) (tc as any).teacher = teacherMap.get(tc.teacherId) || null;
              });

              // 3) Gán teacherCourse vào từng schedule để HTML hiển thị
              this.schedules = schedules.map(s => ({
                ...s,
                teacherCourse: s.teacherCourseId != null ? (tcMap.get(s.teacherCourseId) || null) : null
              })) as Schedule[];

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

  /** Định dạng giờ HH:mm (ẩn giây) */
  fmt(t?: string): string {
    return (t || '').slice(0, 5);
  }

  /** Xoá lịch học */
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

  /** trackBy cho *ngFor */
  trackById = (_: number, s: Schedule) => s.scheduleId;
}
