import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ScheduleService } from '../../../../services/schedule.service';
import { Schedule } from '../../../../models/Schedule.model';

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

  // Mapping cho ngày trong tuần
  dayOfWeekMap: { [key: number]: string } = {
    2: 'Thứ 2',
    3: 'Thứ 3',
    4: 'Thứ 4',
    5: 'Thứ 5',
    6: 'Thứ 6',
    7: 'Thứ 7',
    8: 'Chủ nhật'
  };

  constructor(private scheduleService: ScheduleService) {}

  ngOnInit(): void {
    this.loadSchedules();
  }

  loadSchedules(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scheduleService.getAllSchedules().subscribe({
      next: (data) => {
        this.schedules = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải danh sách lịch học.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  deleteSchedule(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa lịch học này?')) {
      this.scheduleService.deleteSchedule(id).subscribe({
        next: () => {
          this.loadSchedules();
        },
        error: (err) => {
          console.error('Lỗi khi xóa lịch học:', err);
          alert('Có lỗi xảy ra khi xóa lịch học.');
        }
      });
    }
  }

  trackById = (_: number, schedule: Schedule) => schedule.scheduleId;
}
