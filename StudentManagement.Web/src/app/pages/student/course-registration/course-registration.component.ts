import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Course, Enrollment } from '../../../models';
import { CourseService } from '../../../services/course.service';
import { EnrollmentService } from '../../../services/enrollment.service';
import { AuthService } from '../../../services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-course-registration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-registration.component.html',
  styleUrls: ['./course-registration.component.scss']
})
export class CourseRegistrationComponent implements OnInit {
  availableCourses: Course[] = [];
  myEnrollments: Enrollment[] = [];
  studentId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.studentId = tokenPayload?.studentId;

    if (this.studentId) {
      this.loadInitialData();
    } else {
      this.errorMessage = 'Không thể xác định thông tin sinh viên.';
    }
  }

  loadInitialData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // Tải đồng thời danh sách môn học và các môn sinh viên đã đăng ký
    forkJoin({
      courses: this.courseService.getAllCourses(),
      enrollments: this.enrollmentService.getEnrollmentsByStudentId(this.studentId!)
    }).subscribe({
      next: ({ courses, enrollments }) => {
        this.availableCourses = courses;
        this.myEnrollments = enrollments;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải dữ liệu.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  /**
   * Kiểm tra xem sinh viên đã đăng ký môn học này chưa
   */
  isAlreadyEnrolled(courseId: string): boolean {
    return this.myEnrollments.some(e => e.courseId === courseId);
  }

  /**
   * Xử lý việc đăng ký môn học
   */
  registerForCourse(courseId: string): void {
    if (!this.studentId) {
      this.errorMessage = 'Không thể thực hiện đăng ký.';
      return;
    }

    const enrollmentData = {
      studentId: this.studentId,
      courseId: courseId,
      status: 'Enrolled' // Trạng thái mặc định khi đăng ký
    };

    this.enrollmentService.createEnrollment(enrollmentData).subscribe({
      next: () => {
        this.successMessage = `Đăng ký môn học ${courseId} thành công!`;
        // Tải lại danh sách đăng ký để cập nhật trạng thái nút bấm
        this.loadInitialData();
        // Tự động ẩn thông báo thành công sau 3 giây
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = `Lỗi khi đăng ký: ${err.error?.message || 'Vui lòng thử lại.'}`;
        console.error(err);
      }
    });
  }
}
