import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';

// Import models
import { Course, Teacher, Score, TeacherCourse } from '../../../../models';

// Import services
import { CourseService } from '../../../../services/course.service';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { ScoreService } from '../../../../services/score.service';
import { TeacherService } from '../../../../services/teacher.service';

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

  courses: Course[] = [];
  selectedCourse: Course | null = null;

  teachers: Teacher[] = [];
  selectedTeacher: Teacher | null = null;

  scores: Score[] = [];

  constructor(
    private courseService: CourseService,
    private teacherCourseService: TeacherCourseService,
    private scoreService: ScoreService,
    private teacherService: TeacherService
  ) { }

  ngOnInit(): void {
    this.loadCourses();
  }

  // --- Tầng 1 ---
  loadCourses(): void {
    this.isLoading = true;
    this.courseService.getAllCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách môn học.')
    });
  }

  selectCourse(course: Course): void {
    this.selectedCourse = course;
    this.level = 2;
    this.loadTeachersForCourse(course.courseId);
  }

  // --- Tầng 2 ---
  loadTeachersForCourse(courseId: string): void {
    this.isLoading = true;
    this.teacherCourseService.getTeacherCoursesByCourseId(courseId).subscribe({
      next: (teacherCourses) => {
        if (!teacherCourses || teacherCourses.length === 0) {
          this.teachers = [];
          this.isLoading = false;
          return;
        }

        const teacherIds = [...new Set(teacherCourses.map(tc => tc.teacherId))];
        const teacherRequests = teacherIds.map(id => this.teacherService.getTeacherById(id));

        forkJoin(teacherRequests).subscribe({
          next: (teachersData) => {
            this.teachers = teachersData;
            this.isLoading = false;
          },
          error: (err) => this.handleError(err, 'Không thể tải thông tin giảng viên.')
        });
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách giảng viên.')
    });
  }

  selectTeacher(teacher: Teacher): void {
    this.selectedTeacher = teacher;
    this.level = 3;
    this.loadScores(this.selectedCourse!.courseId, teacher.teacherId);
  }

  // --- Tầng 3 ---
  loadScores(courseId: string, teacherId: string): void {
    this.isLoading = true;
    this.scoreService.getByTeacherAndSubject(teacherId, courseId).subscribe({
      next: (scores) => {
        this.scores = scores.map(s => ({
          ...s,
          processScore: s.processScore ?? null,
          midtermScore: s.midtermScore ?? null,
          finalScore: s.finalScore ?? null,
          totalScore: s.totalScore ?? this.calculateTotalScore(s),
          isPassed: s.isPassed ?? null
        }));
        this.isLoading = false;
      },
      error: (err) => this.handleError(err, 'Không thể tải danh sách điểm.')
    });
  }

  // Tính tổng điểm nếu cần
  calculateTotalScore(score: Score): number {
    const process = score.processScore ?? 0;
    const midterm = score.midtermScore ?? 0;
    const final = score.finalScore ?? 0;

    const total = +(process * 0.2 + midterm * 0.3 + final * 0.5).toFixed(2);

    // ✅ Tự động cập nhật trạng thái đậu/rớt theo thang 4.0
    score.totalScore = total;
    score.isPassed = total >= 4;

    return total;
  }


  saveAllScores(): void {
    this.isLoading = true;
    const updateRequests: Observable<any>[] = [];

    this.scores.forEach(score => {
      const totalScore = score.totalScore ?? this.calculateTotalScore(score);
      const scoreData: Partial<Score> = {
        scoreId: score.scoreId,
        processScore: score.processScore,
        midtermScore: score.midtermScore,
        finalScore: score.finalScore,
        totalScore: totalScore,
        isPassed: score.isPassed
      };
      updateRequests.push(this.scoreService.updateScore(score.scoreId, scoreData));
    });

    forkJoin(updateRequests).subscribe({
      next: () => {
        alert('Lưu điểm thành công!');
        this.isLoading = false;
        this.loadScores(this.selectedCourse!.courseId, this.selectedTeacher!.teacherId);
      },
      error: (err) => this.handleError(err, 'Đã có lỗi xảy ra khi lưu điểm.')
    });
  }

  deleteScore(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa điểm này không?')) {
      this.scoreService.deleteScore(id).subscribe({
        next: () => {
          this.loadScores(this.selectedCourse!.courseId, this.selectedTeacher!.teacherId);
        },
        error: (err: HttpErrorResponse) => {
          this.handleError(err, 'Lỗi xảy ra khi xóa điểm.');
        }
      });
    }
  }

  goBack(toLevel: 1 | 2): void {
    this.level = toLevel;
    if (toLevel === 1) {
      this.selectedCourse = null;
      this.selectedTeacher = null;
    } else {
      this.selectedTeacher = null;
    }
  }

  private handleError(error: HttpErrorResponse, message: string): void {
    this.errorMessage = message;
    this.isLoading = false;
    console.error(error);
  }
}
