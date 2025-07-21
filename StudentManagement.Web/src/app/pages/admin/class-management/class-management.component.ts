import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Class } from '../../../models';
import { ClassService } from '../../../services/class.service';
import { CommonModule } from '@angular/common'
@Component({
  standalone: true,
  selector: 'app-class-management',
  imports: [CommonModule],
  templateUrl: './class-management.component.html',
  styleUrls: ['./class-management.component.scss']
})
export class ClassManagementComponent implements OnInit {
  classes: Class[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private classService: ClassService) { }

  ngOnInit(): void {
    this.loadClasses();
  }

  loadClasses(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.classService.getAllClasses().subscribe({
      next: (data) => {
        this.classes = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách lớp học.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editClass(id: string): void {
    console.log('Chỉnh sửa lớp học:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteClass(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa lớp học này không?')) {
      this.classService.deleteClass(id).subscribe({
        next: () => {
          this.loadClasses(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa lớp học.';
          console.error(err);
        }
      });
    }
  }
}
