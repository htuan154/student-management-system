import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Teacher } from '../../../../models';
import { TeacherService } from '../../../../services/teacher.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-teacher-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './teacher-list.component.html',
  styleUrls: ['./teacher-list.component.scss'],
})
export class TeacherListComponent implements OnInit {
  allTeachers: Teacher[] = [];
  teachers: Teacher[] = [];

  pageSize = 10;
  currentPage = 1;

  isLoading = false;
  errorMessage: string | null = null;

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.loadTeachers();
  }

  get totalCount(): number {
    return this.allTeachers.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  loadTeachers(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.teacherService.getAllTeachers().subscribe({
      next: (data) => {
        this.allTeachers = data || [];
        this.currentPage = 1;
        this.applyPagination();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of teachers.';
        this.isLoading = false;
        console.error(err);
      },
    });
  }

  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.teachers = this.allTeachers.slice(start, end);
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

  deleteTeacher(id: string): void {
    if (confirm('Are you sure you want to delete this teacher?')) {
      this.teacherService.deleteTeacher(id).subscribe({
        next: () => {
          this.allTeachers = this.allTeachers.filter(t => t.teacherId !== id);
          const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          if (this.currentPage > maxPage) this.currentPage = maxPage;
          this.applyPagination();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the teacher.';
          console.error(err);
        },
      });
    }
  }

  trackById = (_: number, t: Teacher) => t.teacherId;
}
