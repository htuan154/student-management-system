import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherCourse } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';

@Component({
  selector: 'app-teacher-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './teacher-course-management.component.html',
  styleUrls: ['./teacher-course-management.component.scss']
})
export class TeacherCourseManagementComponent implements OnInit {
  assignments: TeacherCourse[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  formAssignment: Partial<TeacherCourse> = {
    teacherId: '',
    courseId: ''
  };

  editingId: number | null = null;

  constructor(private teacherCourseService: TeacherCourseService) {}

  ngOnInit(): void {
    this.loadAssignments();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.teacherCourseService.getPagedTeacherCourses(1, 1000).subscribe({
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

  saveAssignment(): void {
    if (!this.formAssignment.teacherId || !this.formAssignment.courseId) {
      alert('Vui lòng nhập đầy đủ thông tin giáo viên và môn học.');
      return;
    }

    if (this.editingId != null) {
      this.teacherCourseService.updateTeacherCourse(this.editingId, this.formAssignment).subscribe({
        next: () => {
          alert('Cập nhật phân công thành công!');
          this.cancelEdit();
          this.loadAssignments();
        },
        error: (err) => {
          console.error('Lỗi cập nhật:', err);
          alert('Cập nhật thất bại.');
        }
      });
    } else {
      this.teacherCourseService.createTeacherCourse(this.formAssignment as any).subscribe({
        next: () => {
          alert('Tạo phân công thành công!');
          this.formAssignment = {};
          this.loadAssignments();
        },
        error: (err) => {
          console.error('Lỗi tạo:', err);
          alert('Tạo phân công thất bại.');
        }
      });
    }
  }

  editAssignment(id: number): void {
    const assignment = this.assignments.find(a => a.teacherCourseId === id);
    if (assignment) {
      this.editingId = id;
      this.formAssignment = {
        teacherId: assignment.teacherId,
        courseId: assignment.courseId
      };
    }
  }

  cancelEdit(): void {
    this.editingId = null;
    this.formAssignment = {};
  }

  deleteAssignment(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa phân công này không?')) {
      this.teacherCourseService.deleteTeacherCourse(id).subscribe({
        next: () => {
          alert('Xóa thành công!');
          this.loadAssignments();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa phân công.';
          console.error(err);
        }
      });
    }
  }
}
