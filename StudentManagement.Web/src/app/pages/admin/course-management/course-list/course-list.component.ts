import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Course } from '../../../../models';
import { CourseService } from '../../../../services/course.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './course-list.component.html',
  styleUrls: ['./course-list.component.scss']
})
export class CourseListComponent implements OnInit {
  // dữ liệu đầy đủ + dữ liệu theo trang
  allCourses: Course[] = [];
  courses: Course[] = [];

  // phân trang (client-side)
  pageSize = 10;
  currentPage = 1;

  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private courseService: CourseService) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  // tổng số bản ghi
  get totalCount(): number {
    return this.allCourses.length;
  }

  // tổng số trang
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  // mảng số trang cho *ngFor
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  loadCourses(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.allCourses = data || [];
        this.currentPage = 1;           // reset về trang 1
        this.applyPagination();
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of courses.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.courses = this.allCourses.slice(start, end);
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

  deleteCourse(id: string): void {
    if (confirm('Are you sure you want to delete this course?')) {
      this.courseService.deleteCourse(id).subscribe({
        next: () => {
          // cập nhật mảng tại client, không cần gọi lại server nếu muốn
          this.allCourses = this.allCourses.filter(c => c.courseId !== id);
          const maxPage = Math.max(1, Math.ceil(this.totalCount / this.pageSize));
          if (this.currentPage > maxPage) this.currentPage = maxPage;
          this.applyPagination();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the course.';
          console.error(err);
        }
      });
    }
  }
}
