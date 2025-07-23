import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Enrollment } from '../../../../models';
import { EnrollmentService, PagedEnrollmentResponse } from '../../../../services/enrollment.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './enrollment-list.component.html',
  styleUrls: ['./enrollment-list.component.scss']
})
export class EnrollmentListComponent implements OnInit {
  enrollments: Enrollment[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  constructor(private enrollmentService: EnrollmentService) { }

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.enrollmentService.getPagedEnrollments(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedEnrollmentResponse) => {
        this.enrollments = response.enrollments;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of enrollments.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  deleteEnrollment(id: number): void {
    if (confirm('Are you sure you want to delete this enrollment?')) {
      this.enrollmentService.deleteEnrollment(id).subscribe({
        next: () => {
          this.loadEnrollments(); // Reload the list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the enrollment.';
          console.error(err);
        }
      });
    }
  }
}
