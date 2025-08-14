import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

// Import models and services
import { TeacherCourse, Score } from '../../../models';
import { TeacherCourseService } from '../../../services/teacher-course.service';
import { ScoreService } from '../../../services/score.service';
import { AuthService } from '../../../services/auth.service';

// Interface để định nghĩa cấu trúc dữ liệu thống kê mới
export interface CourseStats {
  courseId: string;
  courseName: string;
  semester: string;
  academicYear: string; // ✅ SỬA: Đổi từ year: number thành academicYear: string
  totalStudents: number;
  passedStudents: number;
  failedStudents: number;
}

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrls: ['./teacher-dashboard.component.scss']
})
export class TeacherDashboardComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;
  teacherId: string | null = null;

  // Dữ liệu thống kê tổng quan
  totalAssignedCourses = 0;
  totalStudents = 0;
  totalPassed = 0;
  totalFailed = 0;

  // Dữ liệu thống kê chi tiết theo từng lớp
  courseStatistics: CourseStats[] = [];

  constructor(
    private teacherCourseService: TeacherCourseService,
    private scoreService: ScoreService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    const tokenPayload = this.authService.getDecodedToken();
    this.teacherId = tokenPayload?.teacherId;

    if (this.teacherId) {
      this.loadDashboardData();
    } else {
      this.errorMessage = 'Không thể tải dữ liệu Bảng điều khiển.';
    }
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.teacherCourseService.getTeacherCoursesByTeacherId(this.teacherId!).pipe(
      switchMap(assignments => {
        this.totalAssignedCourses = assignments.length;
        if (assignments.length === 0) {
          return forkJoin([]); // Trả về observable rỗng nếu không có lớp
        }
        // Với mỗi lớp được phân công, gọi API để lấy danh sách điểm
        const scoreRequests = assignments.map(a =>
          this.scoreService.getByTeacherAndSubject(a.teacherId, a.courseId).pipe(
            map(scores => ({ assignment: a, scores })) // Kết hợp thông tin phân công và điểm
          )
        );
        return forkJoin(scoreRequests);
      })
    ).subscribe({
      next: (results) => {
        this.processStatistics(results);
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Đã có lỗi xảy ra khi tải dữ liệu thống kê.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  private processStatistics(results: { assignment: TeacherCourse, scores: Score[] }[]): void {
    const stats: CourseStats[] = [];
    let totalStudents = 0;
    let totalPassed = 0;
    let totalFailed = 0;

    results.forEach(result => {
      const passed = result.scores.filter(s => s.isPassed === true).length;
      const failed = result.scores.filter(s => s.isPassed === false).length;

      stats.push({
        courseId: result.assignment.courseId,
        courseName: result.assignment.course?.courseName || 'N/A',
        // ✅ SỬA: Lấy từ semester object
        semester: result.assignment.semester?.semesterName || 'N/A',
        academicYear: result.assignment.semester?.academicYear || 'N/A', // ✅ SỬA: Dùng academicYear
        totalStudents: result.scores.length,
        passedStudents: passed,
        failedStudents: failed
      });

      totalStudents += result.scores.length;
      totalPassed += passed;
      totalFailed += failed;
    });

    this.courseStatistics = stats;
    this.totalStudents = totalStudents;
    this.totalPassed = totalPassed;
    this.totalFailed = totalFailed;
  }
}
