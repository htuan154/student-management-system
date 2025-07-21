import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { TeacherCourse } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-teacher-course-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './teacher-course-management.component.html',
  styleUrls: ['./teacher-course-management.component.scss']
})
export class TeacherCourseManagementComponent implements OnInit {
  assignments: TeacherCourse[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private teacherCourseService: TeacherCourseService) { }

  ngOnInit(): void {
    this.loadAssignments();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.errorMessage = null;
    // Note: We need a method in the service to get ALL assignments.
    // Assuming getPagedTeacherCourses can be used by providing a large page size for now.
    this.teacherCourseService.getPagedTeacherCourses(1, 100).subscribe({
      next: (data) => {
        this.assignments = data.tcs;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách phân công.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editAssignment(id: number): void {
    console.log('Chỉnh sửa phân công:', id);
    // Logic to open modal or navigate to edit page
  }

  deleteAssignment(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa phân công này không?')) {
      this.teacherCourseService.deleteTeacherCourse(id).subscribe({
        next: () => {
          this.loadAssignments(); // Reload list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa phân công.';
          console.error(err);
        }
      });
    }
  }
}
