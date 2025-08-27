import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
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

  // nguồn dữ liệu
  semesters: Semester[] = [];
  allTeacherCourses: TeacherCourse[] = []; // full
  teacherCourses: TeacherCourse[] = [];    // sau khi lọc để hiện ra dropdown
  coursesMap = new Map<string, any>();
  teachersMap = new Map<string, any>();

  // để giữ TC id của bản ghi đang sửa (để không bị lọc mất)
  editingTeacherCourseId: number | null = null;

  // Thứ trong tuần (giữ nguyên quy ước 2..8)
  days = [
    { value: 2, label: 'Thứ 2' },
    { value: 3, label: 'Thứ 3' },
    { value: 4, label: 'Thứ 4' },
    { value: 5, label: 'Thứ 5' },
    { value: 6, label: 'Thứ 6' },
    { value: 7, label: 'Thứ 7' },
    { value: 8, label: 'Chủ nhật' },
  ];

  // 4 khung giờ cố định
  timeSlots = [
    { value: '07:00:00-09:00:00', label: '07:00 – 09:00' },
    { value: '09:00:00-11:00:00', label: '09:00 – 11:00' },
    { value: '13:00:00-15:00:00', label: '13:00 – 15:00' },
    { value: '15:00:00-17:00:00', label: '15:00 – 17:00' },
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
  ) {}

  ngOnInit(): void {
    this.buildForm();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit = true;
      this.id = +idParam;
    }

    // Load dữ liệu cần thiết rồi áp dụng lọc dropdown
    this.loadAllData();
  }

  private buildForm(): void {
    // ❗️Đổi startTime/endTime -> timeSlot
    this.form = this.fb.group({
      teacherCourseId: [null, Validators.required],
      dayOfWeek: [2, [Validators.required, Validators.min(2), Validators.max(8)]],
      timeSlot: ['', Validators.required],   // "HH:mm:ss-HH:mm:ss"
      roomNumber: [''],
      location: ['']
    });
  }

  /** Label hiển thị option phân công */
   labelTC(tc: TeacherCourse): string {
  const teacher = (tc as any).teacher?.fullName || tc.teacherId || 'GV?';
  const course  = (tc as any).course?.courseName || tc.courseId || 'Môn?';

  // tìm semester trong danh sách
  const semObj = this.semesters.find(s => s.semesterId === tc.semesterId);
  const semLabel = semObj ? ` (${semObj.semesterName} - ${semObj.academicYear})` : '';

  return `${teacher} — ${course}${semLabel}`;
}

  trackTc = (_: number, tc: TeacherCourse) => tc.teacherCourseId!;

  /** Nạp semesters/courses/teachers, teacherCourses theo semester, schedules để lọc */
  private loadAllData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // 1) load danh mục + tất cả lịch để lọc TC đã có lịch
    forkJoin({
      semesters: this.semesterService.getActiveSemesters().pipe(catchError(() => of([]))),
      courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
      teachers: this.teacherService.getAllTeachers().pipe(catchError(() => of([]))),
      schedules: this.scheduleService.getAllSchedules().pipe(catchError(() => of([] as Schedule[]))),
      editing: this.isEdit && this.id
        ? this.scheduleService.getScheduleById(this.id!).pipe(catchError(() => of(null as unknown as Schedule)))
        : of(null as unknown as Schedule)
    }).subscribe(({ semesters, courses, teachers, schedules, editing }) => {
      this.semesters = semesters || [];
      this.coursesMap = new Map((courses || []).map((c: any) => [c.courseId, c]));
      this.teachersMap = new Map((teachers || []).map((t: any) => [t.teacherId, t]));

      if (editing && editing.teacherCourseId != null) {
        this.editingTeacherCourseId = editing.teacherCourseId;
      }

      // 2) lấy tất cả teacherCourse theo từng semester (tránh gọi root /TeacherCourse -> 405)
      if (!this.semesters.length) {
        this.allTeacherCourses = [];
        this.teacherCourses = [];
        // nếu đang sửa thì patch form luôn (không có TC để hiển thị)
        if (editing) this.patchFromSchedule(editing);
        this.isLoading = false;
        return;
      }

      forkJoin(
        this.semesters.map(s =>
          this.teacherCourseService
            .getTeacherCoursesBySemesterId((s as any).semesterId)
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
        this.allTeacherCourses = Array.from(map.values());

        // 3) lọc bỏ những TC đã có lịch (ở bất kỳ ngày/giờ nào)
        const busyIds = new Set<number>(
          (schedules || [])
            .map(s => s.teacherCourseId!)
            .filter((x): x is number => x != null)
        );

        this.teacherCourses = this.allTeacherCourses.filter(tc => {
          const id = tc.teacherCourseId!;
          // khi edit: vẫn giữ option của chính bản ghi
          if (this.editingTeacherCourseId && id === this.editingTeacherCourseId) return true;
          return !busyIds.has(id);
        });

        // 4) nếu đang sửa → patch form
        if (editing) this.patchFromSchedule(editing);

        this.isLoading = false;
      });
    });
  }

  /** patch dữ liệu form từ lịch khi edit */
  private patchFromSchedule(s: Schedule | null): void {
    if (!s) return;
    this.form.patchValue({
      teacherCourseId: s.teacherCourseId ?? null,
      dayOfWeek: s.dayOfWeek ?? 2,
      timeSlot: `${s.startTime}-${s.endTime}`, // chuyển về slot
      roomNumber: s.roomNumber || '',
      location: s.location || ''
    });

    // Nếu TC của lịch không nằm trong list (do không thuộc kỳ active) → thêm vào dropdown
    if (
      s.teacherCourseId != null &&
      !this.teacherCourses.some(tc => tc.teacherCourseId === s.teacherCourseId)
    ) {
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
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    this.errorMessage = null;

    const raw = this.form.getRawValue();
    // timeSlot dạng "HH:mm:ss-HH:mm:ss"
    const [start, end] = String(raw.timeSlot).split('-');

    const payload = {
      teacherCourseId: Number(raw.teacherCourseId),
      dayOfWeek: Number(raw.dayOfWeek),
      startTime: start,
      endTime: end,
      roomNumber: raw.roomNumber ? String(raw.roomNumber).trim() : null,
      location: raw.location ? String(raw.location).trim() : null
    };

    const req$ = (this.isEdit && this.id)
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

  cancel(): void {
    this.router.navigate(['/admin/schedule-management']);
  }
}
