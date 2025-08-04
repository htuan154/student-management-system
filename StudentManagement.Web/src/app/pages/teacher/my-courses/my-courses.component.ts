import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

// Import models and services
import { TeacherCourse } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-courses.component.html',
  styleUrls: ['./my-courses.component.scss']
})
export class MyCoursesComponent implements OnInit {
  assignedCourses: TeacherCourse[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  teacherId: string | null = null;

  constructor(
    private teacherCourseService: TeacherCourseService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Thử lấy teacherId từ nhiều nguồn
    const tokenPayload = this.authService.getDecodedToken();
    console.log('Full token payload:', tokenPayload);

    // Thử các claim khác nhau cho teacherId
    this.teacherId = tokenPayload?.teacherId ||
                    tokenPayload?.sub ||
                    tokenPayload?.userId ||
                    tokenPayload?.id ||
                    tokenPayload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    console.log('Extracted teacherId:', this.teacherId);

    if (this.teacherId) {
      this.loadAssignedCourses();
    } else {
      this.errorMessage = 'Không thể xác định thông tin giảng viên.';
      console.error('Available claims in token:', Object.keys(tokenPayload || {}));
    }
  }

  loadAssignedCourses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    console.log('Loading courses for teacherId:', this.teacherId);

    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).subscribe({
      next: (data) => {
        console.log('Raw API response:', data);

        // Debug từng course
        data.forEach((tc, index) => {
          console.log(`Course ${index}:`, {
            teacherCourseId: tc.teacherCourseId,
            courseId: tc.courseId,
            courseName: tc.course?.courseName,
            courseCode: tc.course?.courseId,
            semester: tc.semester,
            fullCourseObject: tc.course
          });
        });

        this.assignedCourses = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải danh sách môn học.';
        this.isLoading = false;
        console.error('API Error:', err);
        console.error('Error details:', {
          status: err.status,
          message: err.message,
          url: err.url
        });
      }
    });
  }

  /**
   * Lấy tên môn học với fallback
   */
  getCourseName(course: TeacherCourse): string {
    const courseName = course.course?.courseName;
    console.log('Getting course name for:', course, 'Result:', courseName);
    return courseName || 'Tên môn học không có';
  }

  /**
   * Lấy mã môn học với fallback
   */
  getCourseCode(course: TeacherCourse): string {
    const courseCode = course.course?.courseId;
    return courseCode || 'N/A';
  }

  /**
   * Lấy số lượng sinh viên
   */
  getStudentCount(course: TeacherCourse): number {
    return course.course?.enrollments?.length || 0;
  }

  /**
   * Điều hướng đến trang danh sách sinh viên của một lớp học cụ thể.
   * @param courseId ID của môn học
   */
  viewClassList(courseId: string): void {
    console.log('Navigating to class list with courseId:', courseId, 'teacherId:', this.teacherId);
    // Điều hướng đến một route mới, truyền courseId và teacherId
    this.router.navigate(['/teacher/class-list', courseId, this.teacherId]);
  }
}
