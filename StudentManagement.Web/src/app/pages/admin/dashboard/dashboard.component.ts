import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

// Import các service cần thiết
import { UserService } from '../../../services/user.service';
import { StudentService } from '../../../services/student.service';
import { TeacherService } from '../../../services/teacher.service';
import { CourseService } from '../../../services/course.service';

@Component({
  selector: 'app-dashboard',
  standalone: true, // <-- SỬA LỖI: Thêm thuộc tính này
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule] // Mảng imports chỉ hợp lệ với component standalone
})
export class DashboardComponent implements OnInit {
  // Biến để lưu trữ các số liệu thống kê
  stats = {
    users: 0,
    students: 0,
    teachers: 0,
    courses: 0
  };

  isLoading = false;
  errorMessage: string | null = null;

  // Tiêm tất cả các service cần thiết vào constructor
  constructor(
    private userService: UserService,
    private studentService: StudentService,
    private teacherService: TeacherService,
    private courseService: CourseService
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // Sử dụng forkJoin để thực hiện nhiều lệnh gọi API song song
    forkJoin({
      users: this.userService.getAllUsers().pipe(map(data => data.length)),
      students: this.studentService.getAllStudents().pipe(map(data => data.length)),
      teachers: this.teacherService.getAllTeachers().pipe(map(data => data.length)),
      courses: this.courseService.getAllCourses().pipe(map(data => data.length))
    }).subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
        console.log('Tải dữ liệu dashboard thành công', this.stats);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Lỗi khi tải dữ liệu dashboard', err);
        this.errorMessage = 'Không thể tải được dữ liệu thống kê.';
        this.isLoading = false;
      }
    });
  }
}
