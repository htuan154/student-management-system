import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Score } from '../../../models';
import { ScoreService, PagedScoreResponse } from '../../../services/score.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-score-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './score-management.component.html',
  styleUrls: ['./score-management.component.scss']
})
export class ScoreManagementComponent implements OnInit {
  scores: Score[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Thuộc tính cho phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  constructor(private scoreService: ScoreService) { }

  ngOnInit(): void {
    this.loadScores();
  }

  loadScores(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.scoreService.getPagedScores(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedScoreResponse) => {
        this.scores = response.scores;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách điểm.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  editScore(id: number): void {
    console.log('Chỉnh sửa điểm:', id);
    // Logic để mở modal hoặc điều hướng đến trang chỉnh sửa
  }

  deleteScore(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa điểm này không?')) {
      this.scoreService.deleteScore(id).subscribe({
        next: () => {
          this.loadScores(); // Tải lại danh sách sau khi xóa
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa điểm.';
          console.error(err);
        }
      });
    }
  }
}
