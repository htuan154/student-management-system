import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; // ⬅️ dùng ngModel cho pager
import { SemesterService } from '../../../../services/semester.service';
import { Semester } from '../../../../models/Semester.model';

@Component({
  selector: 'app-semester-list',
  templateUrl: './semester-list.component.html',
  styleUrls: ['./semester-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe, FormsModule], // ⬅️ thêm FormsModule
})
export class SemesterListComponent implements OnInit {
  // dữ liệu đầy đủ + dữ liệu hiển thị theo trang
  allSemesters: Semester[] = [];
  semesters: Semester[] = [];

  // phân trang (client-side)
  pageSize = 10;
  currentPage = 1;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private semesterService: SemesterService, private router: Router) {}

  ngOnInit(): void {
    this.loadSemesters();
  }

  // ===== helpers gốc (giữ nguyên) =====
  private normalize(d: string | null | undefined): Date | null {
    if (!d) return null;
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

  // ===== phân trang =====
  get totalCount(): number {
    return this.allSemesters.length;
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
    this.semesters = this.allSemesters.slice(start, end);
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

  // ===== load dữ liệu (giữ logic gốc, chỉ bổ sung cắt trang) =====
  loadSemesters(): void {
    this.isLoading = true;
    this.semesterService.getAllSemesters().subscribe({
      next: (data) => {
        const mapped = (data || []).map(s => ({
          ...s,
          isActive: s.isActive && !this.isExpired(s.endDate)
        }));

        // Active trước → theo niên khóa (desc) → startDate (desc)
        mapped.sort((a, b) => {
          if (a.isActive !== b.isActive) return a.isActive ? -1 : 1;
          const ay = (b.academicYear || '').localeCompare(a.academicYear || '');
          if (ay !== 0) return ay;
          const as = this.normalize(a.startDate)?.getTime() ?? 0;
          const bs = this.normalize(b.startDate)?.getTime() ?? 0;
          return bs - as;
        });

        this.allSemesters = mapped as Semester[];  // ⬅️ lưu full
        this.currentPage = 1;                      // ⬅️ reset trang
        this.applyPagination();                    // ⬅️ cắt trang
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
