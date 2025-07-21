import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Course } from '../../../models';
import { CourseService } from '../../../services/course.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-management.component.html',
  styleUrls: ['./course-management.component.scss']
})
export class CourseManagementComponent implements OnInit {
  courses: Course[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private courseService: CourseService) { }

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách môn học.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editCourse(id: string): void {
    console.log('Chỉnh sửa môn học:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteCourse(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa môn học này không?')) {
      this.courseService.deleteCourse(id).subscribe({
        next: () => {
          this.loadCourses(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa môn học.';
          console.error(err);
        }
      });
    }
  }
}
