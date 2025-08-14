import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ScheduleService } from '../../../../services/schedule.service';
import { Schedule } from '../../../../models/Schedule.model';

@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './schedule-form.component.html',
  styleUrls: ['./schedule-form.component.scss']
})
export class ScheduleFormComponent implements OnInit {
  form: ReturnType<FormBuilder['group']>; // Khai báo thuộc tính form

  id?: number;
  mode: 'create' | 'edit' = 'create';
  loading = false;
  saving = false;

  // Mapping ngày trong tuần
  daysOfWeek = [
    { value: 2, label: 'Thứ 2' },
    { value: 3, label: 'Thứ 3' },
    { value: 4, label: 'Thứ 4' },
    { value: 5, label: 'Thứ 5' },
    { value: 6, label: 'Thứ 6' },
    { value: 7, label: 'Thứ 7' },
    { value: 8, label: 'Chủ nhật' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private scheduleService: ScheduleService
  ) {
    // Khởi tạo form trong constructor
    this.form = this.fb.group({
      teacherCourseId: [0, [Validators.required, Validators.min(1)]],
      dayOfWeek: [2, [Validators.required, Validators.min(2), Validators.max(8)]],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      roomNumber: [''],
      location: ['']
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = parseInt(idParam);
      this.mode = 'edit';
      this.loadSchedule();
    }
  }

  loadSchedule(): void {
    if (!this.id) return;

    this.loading = true;
    this.scheduleService.getScheduleById(this.id).subscribe({
      next: (schedule) => {
        this.form.patchValue({
          teacherCourseId: schedule.teacherCourseId,
          dayOfWeek: schedule.dayOfWeek,
          startTime: schedule.startTime,
          endTime: schedule.endTime,
          roomNumber: schedule.roomNumber,
          location: schedule.location
        });
        this.loading = false;
      },
      error: (err) => {
        console.error('Lỗi khi tải lịch học:', err);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    const formValue = {
      ...this.form.value,
      teacherCourseId: Number(this.form.value.teacherCourseId),
      dayOfWeek: Number(this.form.value.dayOfWeek),
      startTime: this.form.value.startTime || '',
      endTime: this.form.value.endTime || '',
      roomNumber: this.form.value.roomNumber || null,
      location: this.form.value.location || null
    };

    if (this.mode === 'edit' && this.id) {
      this.scheduleService.updateSchedule(this.id, formValue).subscribe({
        next: () => {
          this.router.navigate(['/admin/schedule-management']);
        },
        error: (err) => {
          console.error('Lỗi khi cập nhật:', err);
          this.saving = false;
        }
      });
    } else {
      this.scheduleService.createSchedule(formValue).subscribe({
        next: () => {
          this.router.navigate(['/admin/schedule-management']);
        },
        error: (err) => {
          console.error('Lỗi khi tạo mới:', err);
          this.saving = false;
        }
      });
    }
  }

  // Helper methods for validation
  get teacherCourseId() { return this.form.get('teacherCourseId'); }
  get dayOfWeek() { return this.form.get('dayOfWeek'); }
  get startTime() { return this.form.get('startTime'); }
  get endTime() { return this.form.get('endTime'); }
}
