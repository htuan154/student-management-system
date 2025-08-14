import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SemesterService } from '../../../../services/semester.service';
import { Semester } from '../../../../models/Semester.model';

@Component({
  selector: 'app-semester-list',
  templateUrl: './semester-list.component.html',
  styleUrls: ['./semester-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
})
export class SemesterListComponent implements OnInit {
  semesters: Semester[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private semesterService: SemesterService, private router: Router) {}

  ngOnInit(): void {
    this.loadSemesters();
  }

  private normalize(d: string | null | undefined): Date | null {
    if (!d) return null;
    // chấp nhận 'YYYY-MM-DD' hoặc ISO
    const dt = new Date(d);
    if (isNaN(dt.getTime())) return null;
    dt.setHours(0, 0, 0, 0);
    return dt;
  }

  private isExpired(endDate: string | null | undefined): boolean {
    const ed = this.normalize(endDate);
    if (!ed) return false;
    const today = new Date(); today.setHours(0,0,0,0);
    return ed.getTime() < today.getTime();
  }

  loadSemesters(): void {
    this.isLoading = true;
    this.semesterService.getAllSemesters().subscribe({
      next: (data) => {
        // FE-only: ép lại hiển thị isActive theo endDate
        const mapped = (data || []).map(s => ({
          ...s,
          isActive: s.isActive && !this.isExpired(s.endDate)
        }));

        // Sắp xếp: Active trước, rồi theo niên khóa, rồi startDate
        mapped.sort((a, b) => {
          if (a.isActive !== b.isActive) return a.isActive ? -1 : 1;
          const ay = (b.academicYear || '').localeCompare(a.academicYear || ''); // desc
          if (ay !== 0) return ay;
          const as = this.normalize(a.startDate)?.getTime() ?? 0;
          const bs = this.normalize(b.startDate)?.getTime() ?? 0;
          return bs - as;
        });

        this.semesters = mapped as Semester[];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách học kỳ.';
        this.isLoading = false;
      }
    });
  }

  deleteSemester(id: number): void {
    if (!confirm('Bạn có chắc muốn xóa học kỳ này?')) return;
    this.semesterService.deleteSemester(id).subscribe({
      next: () => this.loadSemesters(),
      error: () => alert('Xóa không thành công. Vui lòng thử lại.')
    });
  }
}
