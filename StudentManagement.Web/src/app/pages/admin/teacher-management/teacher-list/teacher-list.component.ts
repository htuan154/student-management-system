import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Teacher } from '../../../../models';
import { TeacherService } from '../../../../services/teacher.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-teacher-list',
  templateUrl: './teacher-list.component.html',
  styleUrls: ['./teacher-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule],
})
export class TeacherListComponent implements OnInit {
  teachers: Teacher[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private teacherService: TeacherService) { }

  ngOnInit(): void {
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.teacherService.getAllTeachers().subscribe({
      next: (data) => {
        this.teachers = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of teachers.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }
  trackById = (_: number, t: Teacher) => t.teacherId;

  deleteTeacher(id: string): void {
    if (confirm('Are you sure you want to delete this teacher?')) {
      this.teacherService.deleteTeacher(id).subscribe({
        next: () => {
          this.loadTeachers(); // Reload the list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the teacher.';
          console.error(err);
        }
      });
    }
  }
}
