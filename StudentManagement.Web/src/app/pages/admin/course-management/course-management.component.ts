import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Course } from '../../../models';
import { CourseService } from '../../../services/course.service';

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-management.component.html',
  styleUrls: ['./course-management.component.scss']
})
export class CourseManagementComponent implements OnInit {
  courses: Course[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  formCourse: Partial<Course> = {
    courseName: '',
    department: '',
    description: '',
    credits: 0,
    isActive: true
  };

  editingCourseId: string | null = null;

  constructor(private courseService: CourseService) {}

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

  saveCourse(): void {
    if (!this.formCourse.courseName || !this.formCourse.department) {
      alert('Vui lòng nhập đầy đủ tên môn học và khoa.');
      return;
    }

    if (this.editingCourseId) {
      // Update
      this.courseService.updateCourse(this.editingCourseId, this.formCourse).subscribe({
        next: () => {
          alert('Cập nhật môn học thành công!');
          this.cancelEdit();
          this.loadCourses();
        },
        error: (err) => {
          console.error('Lỗi cập nhật:', err);
          alert('Cập nhật thất bại.');
        }
      });
    } else {
      // Create
      this.courseService.createCourse(this.formCourse as any).subscribe({
        next: () => {
          alert('Tạo môn học thành công!');
          this.formCourse = {};
          this.loadCourses();
        },
        error: (err) => {
          console.error('Lỗi tạo:', err);
          alert('Tạo môn học thất bại.');
        }
      });
    }
  }

  editCourse(id: string): void {
    const course = this.courses.find(c => c.courseId === id);
    if (course) {
      this.editingCourseId = id;
      this.formCourse = { ...course };
    }
  }

  cancelEdit(): void {
    this.editingCourseId = null;
    this.formCourse = {};
  }

  deleteCourse(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa môn học này không?')) {
      this.courseService.deleteCourse(id).subscribe({
        next: () => {
          alert('Xóa thành công!');
          this.loadCourses();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa môn học.';
          console.error(err);
        }
      });
    }
  }
}
