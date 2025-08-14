import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { TeacherCourse } from '../../../../models/teacher-course.model';
import { Semester } from '../../../../models/Semester.model';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { SemesterService } from '../../../../services/semester.service';

@Component({
  selector: 'app-teacher-course-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './teacher-course-list.component.html',
  styleUrls: ['./teacher-course-list.component.scss']
})
export class TeacherCourseListComponent implements OnInit {
  assignments: TeacherCourse[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  semName: Record<number, string> = {};
  semYear: Record<number, string> = {};

  constructor(
    private teacherCourseService: TeacherCourseService,
    private semesterService: SemesterService
  ) {}

  ngOnInit(): void {
    this.loadAssignments();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.teacherCourseService.getPagedTeacherCourses(1, 1000).subscribe({
      next: (resp: any) => {
        // Service trả về { tcs, totalCount, ... }
        this.assignments = resp?.tcs ?? resp?.items ?? resp?.data ?? [];
        const semIds = Array.from(
          new Set(this.assignments.map(a => a.semesterId).filter((x): x is number => x != null))
        );

        if (!semIds.length) { this.isLoading = false; return; }

        forkJoin(
          semIds.map(id =>
            this.semesterService.getSemesterById(id).pipe(catchError(() => of(null as unknown as Semester)))
          )
        ).subscribe({
          next: (list) => {
            for (const s of list) {
              if (s && s.semesterId != null) {
                this.semName[s.semesterId] = s.semesterName ?? 'N/A';
                this.semYear[s.semesterId] = s.academicYear ?? 'N/A';
              }
            }
            this.isLoading = false;
          },
          error: () => this.isLoading = false
        });
      },
      error: (err) => {
        this.errorMessage = 'Không thể tải danh sách phân công.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  trackById = (_: number, a: TeacherCourse) => a.teacherCourseId;
}
