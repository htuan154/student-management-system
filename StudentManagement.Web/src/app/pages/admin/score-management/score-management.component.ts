import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { Score } from '../../../models'; // Import từ barrel nếu đã cấu hình index.ts
import { ScoreService, PagedScoreResponse } from '../../../services/score.service'; // Đã đúng chuẩn

@Component({
  selector: 'app-score-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './score-management.component.html',
  styleUrls: ['./score-management.component.scss']
})
export class ScoreManagementComponent implements OnInit {

  // Danh sách điểm và trạng thái
  scores: Score[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  constructor(private scoreService: ScoreService) {}

  ngOnInit(): void {
    this.loadScores();
  }

  // Tải danh sách điểm có phân trang
  loadScores(): void {
  this.isLoading = true;
  this.errorMessage = null;

  this.scoreService.getAllScores().subscribe({
    next: (data: Score[]) => {
      this.scores = data;
      this.totalCount = data.length; // Tổng số nếu cần hiển thị
      this.isLoading = false;
    },
    error: (err: HttpErrorResponse) => {
      this.errorMessage = 'Không thể tải danh sách điểm.';
      this.isLoading = false;
      console.error(err);
    }
  });
}


  // Chỉnh sửa điểm (logic tuỳ chỉnh hoặc mở modal)
  editScore(id: number): void {
    console.log('Chỉnh sửa điểm:', id);
    // TODO: Mở modal hoặc chuyển trang chỉnh sửa
  }

  // Xoá điểm sau khi xác nhận
  deleteScore(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa điểm này không?')) {
      this.scoreService.deleteScore(id).subscribe({
        next: () => {
          this.loadScores(); // Refresh danh sách
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa điểm.';
          console.error(err);
        }
      });
    }
  }
}
