import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SemesterService } from '../../../../services/semester.service';
import { Semester } from '../../../../models/Semester.model';

@Component({
  selector: 'app-semester-list',
  templateUrl: './semester-list.component.html',
  styleUrls: ['./semester-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
})
export class SemesterListComponent implements OnInit {
  semesters: Semester[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private semesterService: SemesterService, private router: Router) {}

  ngOnInit(): void {
    this.loadSemesters();
  }

  loadSemesters(): void {
    this.isLoading = true;
    this.semesterService.getAllSemesters().subscribe({
      next: (data) => {
        this.semesters = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải danh sách học kỳ.';
        this.isLoading = false;
      }
    });
  }

  deleteSemester(id: number): void {
    if (confirm('Bạn có chắc muốn xóa học kỳ này?')) {
      this.semesterService.deleteSemester(id).subscribe(() => this.loadSemesters());
    }
  }
}
