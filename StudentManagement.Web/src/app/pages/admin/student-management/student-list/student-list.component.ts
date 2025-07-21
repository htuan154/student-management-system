import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Student } from '../../../../models';
import { StudentService } from '../../../../services/student.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterModule],

  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss']
})
export class StudentListComponent implements OnInit {
  students: Student[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private studentService: StudentService) { }

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.studentService.getAllStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of students.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  deleteStudent(id: string): void {
    if (confirm('Are you sure you want to delete this student?')) {
      this.studentService.deleteStudent(id).subscribe({
        next: () => {
          this.loadStudents(); // Reload the list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the student.';
          console.error(err);
        }
      });
    }
  }
}
