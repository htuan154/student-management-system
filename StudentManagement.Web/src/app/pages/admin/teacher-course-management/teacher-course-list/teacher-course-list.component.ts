import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { TeacherCourse } from '../../../../models';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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

  constructor(private teacherCourseService: TeacherCourseService) { }

  ngOnInit(): void {
    this.loadAssignments();
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.teacherCourseService.getPagedTeacherCourses(1, 1000).subscribe({
      next: (data) => {
        this.assignments = data.tcs;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of assignments.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  deleteAssignment(id: number): void {
    if (confirm('Are you sure you want to delete this assignment?')) {
      this.teacherCourseService.deleteTeacherCourse(id).subscribe({
        next: () => {
          this.loadAssignments(); // Reload the list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the assignment.';
          console.error(err);
        }
      });
    }
  }
}
