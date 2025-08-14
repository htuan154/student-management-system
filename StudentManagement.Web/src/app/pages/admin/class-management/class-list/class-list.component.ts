import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ClassService } from '../../../../services/class.service';
import { SemesterService } from '../../../../services/semester.service';

import { of, forkJoin } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-class-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './class-list.component.html',
  styleUrls: ['./class-list.component.scss']
})
export class ClassListComponent implements OnInit {
  classes: any[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private classService: ClassService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.loadClasses();
  }

  /** Tải danh sách lớp và join thông tin Học kỳ theo semesterId (FE-only) */
  loadClasses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.classService.getAllClasses().subscribe({
      next: (data) => {
        const list = (data || []) as any[];
        this.classes = list;

        // Lấy các semesterId duy nhất (bỏ null/undefined)
        const ids = Array.from(
          new Set(
            list.map(c => c?.semesterId ?? c?.semester?.semesterId)
                .filter((x: any) => x != null)
          )
        );

        if (ids.length === 0) {
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

            this.classes = list.map(c => {
              const sid = c?.semesterId ?? c?.semester?.semesterId;
              const semester = sid != null ? (semMap.get(sid) || c.semester || null) : c.semester || null;
              return { ...c, semester };
            });

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

  /** Xoá lớp (giữ nguyên API hiện có) */
  deleteClass(id: number): void {
    if (!confirm('Bạn có chắc muốn xóa lớp này?')) return;
    this.classService.deleteClass(id.toString()).subscribe({
      next: () => this.loadClasses(),
      error: () => alert('Xóa không thành công. Vui lòng thử lại.')
    });
  }

  /** trackBy cho *ngFor để render mượt */
  trackById = (_: number, c: any) => c.classId ?? c.id;
}
