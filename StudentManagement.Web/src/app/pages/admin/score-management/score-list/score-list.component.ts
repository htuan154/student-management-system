import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router'; // ⬅ Thêm Router
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';

import { Course, Score, TeacherCourse, Enrollment } from '../../../../models';
import { CourseService } from '../../../../services/course.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { ScoreService } from '../../../../services/score.service';

@Component({
  selector: 'app-score-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './score-list.component.html',
  styleUrls: ['./score-list.component.scss']
})
export class ScoreListComponent implements OnInit {
  level: 1 | 2 | 3 = 1;
  isLoading = false;
  errorMessage: string | null = null;

  // Tầng 1: Danh sách môn học
  courses: Course[] = [];
  selectedCourse: Course | null = null;

  // Tầng 2: Danh sách giảng viên được phân công dạy môn đó
  assignedTeachers: { teacherId: string; teacherName: string; teacherCourseId: number }[] = [];
  selectedTeacherId: string | null = null;
  selectedTeacherCourseId: number | null = null;

  // Tầng 3: Danh sách điểm sinh viên
  scores: Score[] = [];

  constructor(
    private courseService: CourseService,
    private teacherCourseService: TeacherCourseService,
    private enrollmentService: EnrollmentService,
    private scoreService: ScoreService,
    private router: Router // ⬅ Thêm Router injection
  ) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  // === TẦNG 1: CHỌN MÔN HỌC ===
  loadCourses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data || [];
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách môn học.')
    });
  }

  selectCourse(course: Course): void {
    this.selectedCourse = course;
    this.resetLowerLevels();
    this.level = 2;
    this.loadAssignedTeachers(course.courseId);
  }

  // === TẦNG 2: CHỌN GIẢNG VIÊN ĐƯỢC PHÂN CÔNG ===
  loadAssignedTeachers(courseId: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.teacherCourseService.getTeacherCoursesByCourseId(courseId).subscribe({
      next: (teacherCourses) => {
        // Map ra danh sách giảng viên với thông tin cần thiết
        this.assignedTeachers = (teacherCourses || [])
          .filter(tc => tc.isActive) // Chỉ lấy phân công đang hoạt động
          .map(tc => ({
            teacherId: tc.teacherId,
            teacherName: tc.teacher?.fullName || tc.teacher?.teacherId || tc.teacherId,
            teacherCourseId: tc.teacherCourseId
          }));

        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách giảng viên được phân công.')
    });
  }

  selectTeacher(teacherId: string, teacherCourseId: number): void {
    this.selectedTeacherId = teacherId;
    this.selectedTeacherCourseId = teacherCourseId;
    this.level = 3;
    // Dùng teacherId + courseId thay vì teacherCourseId
    this.loadScoresByTeacherAndCourse(teacherId, this.selectedCourse!.courseId);
  }

  // === TẦNG 3: DANH SÁCH ĐIỂM SINH VIÊN ===
  loadScoresByTeacherAndCourse(teacherId: string, courseId: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scoreService.getScoresByTeacherAndCourse(teacherId, courseId).subscribe({
      next: (scores) => {
        console.log('Scores loaded:', scores); // Debug log
        this.scores = (scores || []).map(score => {
          const total = this.calculateTotalScore(score);
          return {
            ...score,
            totalScore: total,
            isPassed: total >= 4,
            // Fallback cho thông tin sinh viên
            studentId: score.studentId || score.enrollment?.studentId || score.enrollment?.student?.studentId || 'N/A',
            fullName: score.fullName || score.enrollment?.student?.fullName || 'N/A'
          };
        });

        if (this.scores.length === 0) {
          console.warn('No scores found for teacher:', teacherId, 'course:', courseId);
        }

        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Failed to load scores:', err);
        this.errorMessage = 'Không thể tải danh sách điểm. Server error.';
        this.scores = [];
        this.isLoading = false;
      }
    });
  }

  // === HELPER METHODS ===
  calculateTotalScore(score: Score): number {
    const process = score.processScore || 0;
    const midterm = score.midtermScore || 0;
    const final = score.finalScore || 0;
    return +(process * 0.2 + midterm * 0.3 + final * 0.5).toFixed(2);
  }

  // Thêm getter cho tên giảng viên được chọn
  get selectedTeacherName(): string {
    if (!this.selectedTeacherId || !this.assignedTeachers.length) return 'N/A';
    const teacher = this.assignedTeachers.find(t => t.teacherId === this.selectedTeacherId);
    return teacher?.teacherName || 'N/A';
  }

  saveAllScores(): void {
    if (!this.scores.length) return;

    this.isLoading = true;
    const updateRequests: Observable<any>[] = [];

    this.scores.forEach(score => {
      const totalScore = this.calculateTotalScore(score);
      const scoreData = {
        scoreId: score.scoreId,
        processScore: score.processScore,
        midtermScore: score.midtermScore,
        finalScore: score.finalScore,
        totalScore: totalScore,
        isPassed: totalScore >= 4
      };

      updateRequests.push(this.scoreService.updateScore(score.scoreId, scoreData));
    });

    forkJoin(updateRequests).subscribe({
      next: () => {
        alert('Lưu điểm thành công!');
        this.loadScoresByTeacherAndCourse(this.selectedTeacherId!, this.selectedCourse!.courseId);
      },
      error: (err) => this.handleError(err, 'Có lỗi xảy ra khi lưu điểm.')
    });
  }

  deleteScore(scoreId: number): void {
    if (!confirm('Bạn có chắc chắn muốn xóa điểm này không?')) return;

    this.scoreService.deleteScore(scoreId).subscribe({
      next: () => {
        this.loadScoresByTeacherAndCourse(this.selectedTeacherId!, this.selectedCourse!.courseId);
      },
      error: (err) => this.handleError(err, 'Có lỗi xảy ra khi xóa điểm.')
    });
  }

  // === NAVIGATION ===
  goBack(): void {
    if (this.level === 3) {
      this.level = 2;
      this.selectedTeacherId = null;
      this.selectedTeacherCourseId = null;
      this.scores = [];
    } else if (this.level === 2) {
      this.level = 1;
      this.resetLowerLevels();
      this.selectedCourse = null;
    }
  }

  private resetLowerLevels(): void {
    this.assignedTeachers = [];
    this.selectedTeacherId = null;
    this.selectedTeacherCourseId = null;
    this.scores = [];
  }

  private handleError(error: HttpErrorResponse, message: string): void {
    this.errorMessage = message;
    this.isLoading = false;
    console.error(error);
  }

  // ⬅ THÊM METHOD navigateToAddScore()
  navigateToAddScore(): void {
    if (this.selectedCourse && this.selectedTeacherId) {
      // Navigate to add score form với courseId và teacherId
      this.router.navigate(['/admin/scores/add'], {
        queryParams: {
          courseId: this.selectedCourse.courseId,
          teacherId: this.selectedTeacherId,
          courseName: this.selectedCourse.courseName,
          teacherName: this.selectedTeacherName
        }
      });
    } else {
      console.error('Missing course or teacher information');
      alert('Lỗi: Thiếu thông tin môn học hoặc giảng viên.');
    }
  }

  // === TRACK BY FUNCTIONS ===
  trackByCourseId = (_: number, course: Course) => course.courseId;
  trackByTeacherId = (_: number, teacher: any) => teacher.teacherId;
  trackByScoreId = (_: number, score: Score) => score.scoreId;
}

