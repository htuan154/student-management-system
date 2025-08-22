import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ClassService } from '../../../../services/class.service';
import { SemesterService } from '../../../../services/semester.service';

import { of, forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-class-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './class-list.component.html',
  styleUrls: ['./class-list.component.scss']
})
export class ClassListComponent implements OnInit {
  // dữ liệu đầy đủ + dữ liệu theo trang
  allClasses: any[] = [];
  classes: any[] = [];

  // phân trang (client-side)
  pageSize = 10;
  currentPage = 1;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private classService: ClassService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.loadClasses();
  }

  // tổng số bản ghi
  get totalCount(): number {
    return this.allClasses.length;
  }

  // tổng số trang
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  // mảng số trang cho *ngFor
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  /** Tải danh sách lớp và join thông tin Học kỳ theo semesterId (FE-only) */
  loadClasses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.classService.getAllClasses().subscribe({
      next: (data) => {
        const list = (data || []) as any[];
        // lưu full để phân trang client
        this.allClasses = list;

        // Lấy các semesterId duy nhất (bỏ null/undefined)
        const ids = Array.from(
          new Set(
            list
              .map(c => c?.semesterId ?? c?.semester?.semesterId)
              .filter((x: any) => x != null)
          )
        );

        if (ids.length === 0) {
          this.currentPage = 1;
          this.applyPagination();       // ⬅️ cắt trang ngay khi không có semester
          this.isLoading = false;
          return;
        }

        // Nạp học kỳ theo từng ID rồi map vào từng class
        forkJoin(
          ids.map(id =>
            this.semesterService.getSemesterById(id)
              .pipe(catchError(() => of(null)))
          )
        ).subscribe({
          next: (semList) => {
            const semMap = new Map<number, any>();
            (semList || []).forEach(s => { if (s?.semesterId != null) semMap.set(s.semesterId, s); });

            const merged = list.map(c => {
              const sid = c?.semesterId ?? c?.semester?.semesterId;
              const semester = sid != null ? (semMap.get(sid) || c.semester || null) : c.semester || null;
              return { ...c, semester };
            });

            this.allClasses = merged;
            this.currentPage = 1;        // reset về trang 1 sau khi merge
            this.applyPagination();
            this.isLoading = false;
          },
          error: () => {
            this.errorMessage = 'Không thể tải dữ liệu học kỳ.';
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách lớp học.';
        this.isLoading = false;
      }
    });
  }

  /** Cắt mảng theo trang hiện tại */
  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.classes = this.allClasses.slice(start, end);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }

  prev(): void {
    this.goToPage(this.currentPage - 1);
  }

  next(): void {
    this.goToPage(this.currentPage + 1);
  }

  changePageSize(size: number | string): void {
    this.pageSize = Number(size);
    this.currentPage = 1;
    this.applyPagination();
  }

  /** Xoá lớp (giữ nguyên API hiện có) */
  deleteClass(id: number): void {
    if (!confirm('Bạn có chắc muốn xóa lớp này?')) return;
    this.classService.deleteClass(id.toString()).subscribe({
      next: () => {
        // cập nhật client-side để mượt (không bắt buộc)
        this.allClasses = this.allClasses.filter(c => (c.classId ?? c.id) !== id);
        const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
        if (this.currentPage > maxPage) this.currentPage = maxPage;
        this.applyPagination();
      },
      error: () => alert('Xóa không thành công. Vui lòng thử lại.')
    });
  }

  /** trackBy cho *ngFor để render mượt */
  trackById = (_: number, c: any) => c.classId ?? c.id;
}
