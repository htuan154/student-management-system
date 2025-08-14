import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleService } from '../../../services/schedule.service';
import { Schedule } from '../../../models';

type TimeSlot = { start: string; end: string; label: string };

type SemesterOption = {
  semesterId: number;
  semesterName: string;
  academicYear: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
  courseCount: number; // số môn (distinct teacherCourseId)
};

@Component({
  selector: 'app-student-schedule',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './schedule.component.html',
  styleUrls: ['./schedule.component.scss']
})
export class ScheduleComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;

  /** Toàn bộ lịch (lấy 1 lần) */
  private allSchedules: Schedule[] = [];

  /** Lịch đang hiển thị (lọc theo kỳ) */
  schedules: Schedule[] = [];

  /** Danh sách kỳ cho dropdown */
  semesters: SemesterOption[] = [];
  selectedSemesterId: number | null = null;

  // Cột ngang: Thứ 2 -> CN
  days: number[] = [2, 3, 4, 5, 6, 7, 8];

  // Khung giờ mặc định
  readonly DEFAULT_SLOTS: TimeSlot[] = [
    { start: '07:00:00', end: '09:00:00', label: '07:00:00 – 09:00:00' },
    { start: '09:00:00', end: '11:00:00', label: '09:00:00 – 11:00:00' },
    { start: '13:00:00', end: '15:00:00', label: '13:00:00 – 15:00:00' },
    { start: '15:00:00', end: '17:00:00', label: '15:00:00 – 17:00:00' },
  ];

  timeSlots: TimeSlot[] = [];
  private cellIndex = new Map<string, Schedule[]>();

  constructor(private scheduleService: ScheduleService) {}

  ngOnInit(): void {
    // Cách A: lấy toàn bộ lịch cá nhân 1 lần, tự build danh sách kỳ
    this.loadMySemestersFromSchedules();
  }

  /** ===== CÁCH A: Build danh sách kỳ từ dữ liệu lịch ===== */
  private loadMySemestersFromSchedules(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scheduleService.getMySchedule().subscribe({
      next: (data: Schedule[]) => {
        // Lưu toàn bộ lịch
        this.allSchedules = this.dedupeByCompositeKey(data || []);

        // Build danh sách kỳ + đếm số môn
        this.semesters = this.buildSemesterOptions(this.allSchedules);

        // Chọn kỳ hiện tại (active / trong khoảng ngày / mới nhất)
        this.selectedSemesterId = this.pickCurrentSemesterId(this.semesters);

        // Áp dụng lọc từ allSchedules -> schedules + dựng bảng
        this.applyFilterFromAll();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không tải được lịch học cá nhân.';
        this.isLoading = false;
      }
    });
  }

  /** Gom theo semesterId và đếm số môn (distinct teacherCourseId) */
  private buildSemesterOptions(items: Schedule[]): SemesterOption[] {
    const map = new Map<number, { info: SemesterOption; courses: Set<number> }>();

    for (const s of items) {
      const sem = s.teacherCourse?.semester;
      const semId = s.teacherCourse?.semesterId ?? sem?.semesterId;
      if (!semId) continue;

      if (!map.has(semId)) {
        map.set(semId, {
          info: {
            semesterId: semId,
            semesterName: sem?.semesterName ?? 'Kỳ học',
            academicYear: sem?.academicYear ?? '',
            startDate: sem?.startDate,
            endDate: sem?.endDate,
            isActive: (sem as any)?.isActive,
            courseCount: 0
          },
          courses: new Set<number>()
        });
      }
      if (s.teacherCourseId) {
        map.get(semId)!.courses.add(s.teacherCourseId);
      }
    }

    const result = Array.from(map.values()).map(x => ({
      ...x.info,
      courseCount: x.courses.size
    }));

    // sắp xếp mới nhất trước (startDate desc; fallback id desc)
    result.sort((a, b) => {
      const ta = a.startDate ? Date.parse(a.startDate) : 0;
      const tb = b.startDate ? Date.parse(b.startDate) : 0;
      if (ta !== tb) return tb - ta;
      return b.semesterId - a.semesterId;
    });

    return result;
  }

  private pickCurrentSemesterId(list: SemesterOption[]): number | null {
    const now = Date.now();

    const active = list.find(s => s.isActive);
    if (active) return active.semesterId;

    const inRange = list.find(s => {
      if (!s.startDate || !s.endDate) return false;
      const st = Date.parse(s.startDate);
      const en = Date.parse(s.endDate);
      return !Number.isNaN(st) && !Number.isNaN(en) && now >= st && now <= en;
    });
    if (inRange) return inRange.semesterId;

    return list.length ? list[0].semesterId : null;
  }

  onChangeSemester(value: string) {
    this.selectedSemesterId = value ? +value : null;
    this.applyFilterFromAll();
  }

  /** Lọc lịch từ allSchedules theo kỳ đang chọn + dựng index bảng */
  private applyFilterFromAll(): void {
    const filtered = this.selectedSemesterId
      ? this.allSchedules.filter(s =>
          (s.teacherCourse?.semesterId ?? s.teacherCourse?.semester?.semesterId) === this.selectedSemesterId
        )
      : this.allSchedules;

    this.schedules = this.dedupeByCompositeKey(filtered);
    this.rebuildIndex();
  }

  /** Khử trùng bằng khóa tổng hợp (môn + ngày + giờ) */
  private dedupeByCompositeKey(list: Schedule[]): Schedule[] {
    const seen = new Set<string>();
    const out: Schedule[] = [];
    for (const s of list) {
      const k = this.makeKey(s);
      if (!seen.has(k)) {
        seen.add(k);
        out.push(s);
      }
    }
    return out;
  }

  private makeKey(s: Schedule): string {
    return `${s.teacherCourseId}|${s.dayOfWeek}|${s.startTime}|${s.endTime}`;
  }

  /** ===== Dựng bảng theo ô (day × timeSlot) ===== */
  private rebuildIndex() {
    this.cellIndex.clear();

    // build slot map từ default + slot phát sinh từ dữ liệu
    const slotMap = new Map<string, TimeSlot>();
    for (const s of this.DEFAULT_SLOTS) slotMap.set(`${s.start}|${s.end}`, s);

    for (const s of this.schedules) {
      const slotKey = `${s.startTime}|${s.endTime}`;
      if (!slotMap.has(slotKey)) {
        slotMap.set(slotKey, { start: s.startTime, end: s.endTime, label: `${s.startTime} – ${s.endTime}` });
      }

      const cellKey = `${s.dayOfWeek}|${s.startTime}|${s.endTime}`;
      const list = this.cellIndex.get(cellKey) ?? [];
      const comp = this.makeKey(s);
      if (!list.some(x => this.makeKey(x) === comp)) list.push(s);
      this.cellIndex.set(cellKey, list);
    }

    this.timeSlots = Array.from(slotMap.values()).sort((a, b) => a.start.localeCompare(b.start));
  }

  // ===== Helpers cho template =====
  cell(day: number, slot: TimeSlot): Schedule[] {
    return this.cellIndex.get(`${day}|${slot.start}|${slot.end}`) ?? [];
  }

  dayLabel(d: number) {
    const map: Record<number, string> = {
      2: 'Thứ 2', 3: 'Thứ 3', 4: 'Thứ 4', 5: 'Thứ 5', 6: 'Thứ 6', 7: 'Thứ 7', 8: 'CN'
    };
    return map[d] ?? `Thứ ${d}`;
  }

  courseTitle(s: Schedule) {
    return s.teacherCourse?.course?.courseName || s.teacherCourse?.courseId || 'Học phần';
  }

  teacherName(s: Schedule) {
    return s.teacherCourse?.teacher?.fullName;
  }

  trackSem = (_: number, it: SemesterOption) => it.semesterId;
}
