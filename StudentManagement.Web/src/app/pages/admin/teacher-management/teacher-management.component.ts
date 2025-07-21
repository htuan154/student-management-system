import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Teacher } from '../../../models';
import { TeacherService } from '../../../services/teacher.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-teacher-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './teacher-management.component.html',
  styleUrls: ['./teacher-management.component.scss']
})
export class TeacherManagementComponent implements OnInit {
  teachers: Teacher[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private teacherService: TeacherService) { }

  ngOnInit(): void {
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.teacherService.getAllTeachers().subscribe({
      next: (data) => {
        this.teachers = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách giáo viên.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editTeacher(id: string): void {
    // Logic để điều hướng đến trang chỉnh sửa hoặc mở modal
    console.log('Chỉnh sửa giáo viên:', id);
  }

  deleteTeacher(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa giáo viên này không?')) {
      this.teacherService.deleteTeacher(id).subscribe({
        next: () => {
          // Tải lại danh sách sau khi xóa thành công
          this.loadTeachers();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa giáo viên.';
          console.error(err);
        }
      });
    }
  }
}
