import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { ScheduleService } from '../../../../services/schedule.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';
import { CourseService } from '../../../../services/course.service';
import { TeacherService } from '../../../../services/teacher.service';

import { TeacherCourse } from '../../../../models/teacher-course.model';
import { Schedule } from '../../../../models/Schedule.model';
import { Semester } from '../../../../models/Semester.model';

import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './schedule-form.component.html',
  styleUrls: ['./schedule-form.component.scss']
})
export class ScheduleFormComponent implements OnInit {
  form!: FormGroup;

  isEdit = false;
  id?: number;
  isLoading = false;
  errorMessage: string | null = null;

  teacherCourses: TeacherCourse[] = [];
  semesters: Semester[] = [];
  coursesMap = new Map<string, any>();
  teachersMap = new Map<string, any>();

  // danh sách thứ
  days = [
    { value: 2, label: 'Thứ 2' },
    { value: 3, label: 'Thứ 3' },
    { value: 4, label: 'Thứ 4' },
    { value: 5, label: 'Thứ 5' },
    { value: 6, label: 'Thứ 6' },
    { value: 7, label: 'Thứ 7' },
    { value: 8, label: 'Chủ nhật' },
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private scheduleService: ScheduleService,
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService,
    private courseService: CourseService,
    private teacherService: TeacherService
  ) {
    this.form = this.fb.group(
      {
        teacherCourseId: [null, Validators.required],
        dayOfWeek: [2, [Validators.required, Validators.min(2), Validators.max(8)]],
        startTime: ['', Validators.required],
        endTime: ['', Validators.required],
        roomNumber: [''],
        location: ['']
      },
      { validators: [this.timeRangeValidator] }
    );
  }

  get teacherCourseId() { return this.form.get('teacherCourseId'); }

  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (param) { this.id = +param; this.isEdit = true; }

    // nạp option cho select
    this.loadTeacherCourseOptions(() => {
      // sau khi có options -> nếu là edit thì tải bản ghi
      if (this.isEdit && this.id) this.loadSchedule(this.id);
    });

    // nếu thay đổi startTime/endTime -> revalidate
    this.form.get('startTime')?.valueChanges.subscribe(() => this.form.updateValueAndValidity({ onlySelf: false }));
    this.form.get('endTime')?.valueChanges.subscribe(() => this.form.updateValueAndValidity({ onlySelf: false }));
  }

  /** Nạp TeacherCourse thuộc các học kỳ đang hoạt động, kèm nhãn GV/Môn */
  private loadTeacherCourseOptions(done?: () => void): void {
    forkJoin({
      semesters: this.semesterService.getActiveSemesters().pipe(catchError(() => of([]))),
      courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
      teachers: this.teacherService.getAllTeachers().pipe(catchError(() => of([])))
    }).subscribe(({ semesters, courses, teachers }) => {
      this.semesters = semesters || [];
      this.coursesMap = new Map((courses || []).map((c: any) => [c.courseId, c]));
      this.teachersMap = new Map((teachers || []).map((t: any) => [t.teacherId, t]));

      if (!this.semesters.length) { this.teacherCourses = []; done?.(); return; }

      // lấy TC theo từng học kỳ (tránh /TeacherCourse root -> 405)
      forkJoin(
        this.semesters.map(s =>
          this.teacherCourseService.getTeacherCoursesBySemesterId((s as any).semesterId)
            .pipe(catchError(() => of([] as TeacherCourse[])))
        )
      ).subscribe((lists) => {
        const map = new Map<number, TeacherCourse>();
        (lists || []).flat().forEach(tc => {
          if (!tc) return;
          // gắn navigation nếu BE không trả
          if (!tc.course && tc.courseId)  (tc as any).course  = this.coursesMap.get(tc.courseId)  || null;
          if (!tc.teacher && tc.teacherId) (tc as any).teacher = this.teachersMap.get(tc.teacherId) || null;
          if (tc.teacherCourseId != null) map.set(tc.teacherCourseId, tc);
        });
        this.teacherCourses = Array.from(map.values());
        done?.();
      });
    });
  }

  /** Nếu đang sửa, tải lịch và patch vào form; đảm bảo option tồn tại */
  private loadSchedule(id: number): void {
    this.isLoading = true;
    this.scheduleService.getScheduleById(id).subscribe({
      next: (s: Schedule) => {
        // nếu TC của lịch không nằm trong kỳ active -> vẫn thêm option để chọn/hiển thị
        if (s.teacherCourseId != null && !this.teacherCourses.some(t => t.teacherCourseId === s.teacherCourseId)) {
          this.teacherCourseService.getTeacherCourseById(s.teacherCourseId)
            .pipe(catchError(() => of(null as unknown as TeacherCourse)))
            .subscribe(tc => {
              if (tc) {
                if (!tc.course && tc.courseId)  (tc as any).course  = this.coursesMap.get(tc.courseId)  || null;
                if (!tc.teacher && tc.teacherId) (tc as any).teacher = this.teachersMap.get(tc.teacherId) || null;
                this.teacherCourses = [tc, ...this.teacherCourses];
              }
            });
        }

        this.form.patchValue({
          teacherCourseId: s.teacherCourseId ?? null,
          dayOfWeek: s.dayOfWeek ?? 2,
          startTime: s.startTime?.slice(0,5) || '',
          endTime: s.endTime?.slice(0,5) || '',
          roomNumber: s.roomNumber || '',
          location: s.location || ''
        });
        this.isLoading = false;
      },
      error: () => { this.errorMessage = 'Không tải được lịch học.'; this.isLoading = false; }
    });
  }

  /** Nhãn hiển thị của option */
  labelTC(tc: TeacherCourse): string {
    const teacher = (tc as any).teacher?.fullName || tc.teacherId || 'GV?';
    const course  = (tc as any).course?.courseName || tc.courseId || 'Môn?';
    const sem     = tc.semesterId ? ` (HK ${tc.semesterId})` : '';
    return `${teacher} — ${course}${sem}`;
  }

  trackTc = (_: number, tc: TeacherCourse) => tc.teacherCourseId!;

  /** Validate: endTime > startTime */
  private timeRangeValidator = (group: AbstractControl): ValidationErrors | null => {
    const s = group.get('startTime')?.value as string;
    const e = group.get('endTime')?.value as string;
    if (!s || !e) return null;
    return e > s ? null : { timeRange: 'Giờ kết thúc phải sau giờ bắt đầu.' };
  };

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading = true; this.errorMessage = null;

    const raw = this.form.getRawValue();
    const payload = {
      teacherCourseId: Number(raw.teacherCourseId),
      dayOfWeek: Number(raw.dayOfWeek),
      startTime: String(raw.startTime), // HH:mm
      endTime: String(raw.endTime),     // HH:mm
      roomNumber: raw.roomNumber ? String(raw.roomNumber).trim() : null,
      location: raw.location ? String(raw.location).trim() : null
    } as any;

    const req$ = this.isEdit && this.id
      ? this.scheduleService.updateSchedule(this.id!, payload)
      : this.scheduleService.createSchedule(payload);

    req$.subscribe({
      next: () => this.router.navigate(['/admin/schedule-management']),
      error: (err) => {
        if (err?.status === 400 && err?.error?.errors) {
          const msgs = Object.values(err.error.errors).flat() as string[];
          this.errorMessage = msgs.join(' ');
        } else if (typeof err?.error === 'string') {
          this.errorMessage = err.error;
        } else {
          this.errorMessage = err?.error?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại.';
        }
        this.isLoading = false;
      }
    });
  }
}
