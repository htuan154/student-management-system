import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Class } from '../../../models';
import { ClassService } from '../../../services/class.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  standalone: true,
  selector: 'app-class-management',
  imports: [CommonModule, FormsModule],
  templateUrl: './class-management.component.html',
  styleUrls: ['./class-management.component.scss']
})
export class ClassManagementComponent implements OnInit {
  classes: Class[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  formClass: Partial<Class> = {
    className: '',
    major: '',
    academicYear: ''
  };
  editingId: string | null = null;

  constructor(private classService: ClassService) {}

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

  saveClass(): void {
    if (!this.formClass.className || !this.formClass.major || !this.formClass.academicYear) {
      alert('Vui lòng nhập đầy đủ thông tin lớp học.');
      return;
    }

    if (this.editingId) {
      this.classService.updateClass(this.editingId, this.formClass).subscribe({
        next: () => {
          alert('Cập nhật lớp học thành công!');
          this.cancelEdit();
          this.loadClasses();
        },
        error: (err) => {
          console.error(err);
          alert('Cập nhật thất bại.');
        }
      });
    } else {
      this.classService.createClass(this.formClass as any).subscribe({
        next: () => {
          alert('Tạo lớp học mới thành công!');
          this.formClass = {};
          this.loadClasses();
        },
        error: (err) => {
          console.error(err);
          alert('Tạo mới thất bại.');
        }
      });
    }
  }

  editClass(id: string): void {
    const cls = this.classes.find(c => c.classId === id);
    if (cls) {
      this.editingId = id;
      this.formClass = {
        className: cls.className,
        major: cls.major,
        academicYear: cls.academicYear
      };
    }
  }

  cancelEdit(): void {
    this.editingId = null;
    this.formClass = {};
  }

  deleteClass(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa lớp học này không?')) {
      this.classService.deleteClass(id).subscribe({
        next: () => {
          alert('Xóa thành công!');
          this.loadClasses();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa lớp học.';
          console.error(err);
        }
      });
    }
  }
}
