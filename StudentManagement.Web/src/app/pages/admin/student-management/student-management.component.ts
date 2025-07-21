import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Student } from '../../../models';
import { StudentService } from '../../../services/student.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-student-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './student-management.component.html',
  styleUrls: ['./student-management.component.scss']
})
export class StudentManagementComponent implements OnInit {
  students: Student[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private studentService: StudentService) { }

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.studentService.getAllStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách sinh viên.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editStudent(id: string): void {
    console.log('Chỉnh sửa sinh viên:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteStudent(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa sinh viên này không?')) {
      this.studentService.deleteStudent(id).subscribe({
        next: () => {
          this.loadStudents(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa sinh viên.';
          console.error(err);
        }
      });
    }
  }
}
