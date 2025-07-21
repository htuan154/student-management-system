import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Score } from '../../../models';
import { ScoreService } from '../../../services/score.service';

@Component({
  selector: 'app-score-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './score-management.component.html',
  styleUrls: ['./score-management.component.scss']
})
export class ScoreManagementComponent implements OnInit {
  scores: Score[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  scoreForm!: FormGroup;
  isEditMode = false;
  selectedScoreId: number | null = null;

  constructor(
    private scoreService: ScoreService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadScores();
    this.initForm();
  }

  initForm(): void {
    this.scoreForm = this.fb.group({
      enrollmentId: [null, Validators.required],
      processScore: [null, [Validators.required, Validators.min(0), Validators.max(10)]],
      midtermScore: [null, [Validators.required, Validators.min(0), Validators.max(10)]],
      finalScore: [null, [Validators.required, Validators.min(0), Validators.max(10)]],
    });
  }

  loadScores(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.scoreService.getAllScores().subscribe({
      next: (data: Score[]) => {
        this.scores = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách điểm.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  submitForm(): void {
    if (this.scoreForm.invalid) return;

    const formValue = this.scoreForm.value;

    if (this.isEditMode && this.selectedScoreId !== null) {
      this.scoreService.updateScore(this.selectedScoreId, formValue).subscribe({
        next: () => {
          this.loadScores();
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Lỗi khi cập nhật điểm.';
          console.error(err);
        }
      });
    } else {
      this.scoreService.createScore(formValue).subscribe({
        next: () => {
          this.loadScores();
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Lỗi khi tạo mới điểm.';
          console.error(err);
        }
      });
    }
  }

  editScore(score: Score): void {
    this.isEditMode = true;
    this.selectedScoreId = score.scoreId;
    this.scoreForm.patchValue({
      enrollmentId: score.enrollmentId,
      processScore: score.processScore,
      midtermScore: score.midtermScore,
      finalScore: score.finalScore
    });
  }

  deleteScore(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa điểm này không?')) {
      this.scoreService.deleteScore(id).subscribe({
        next: () => this.loadScores(),
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Lỗi khi xóa điểm.';
          console.error(err);
        }
      });
    }
  }

  resetForm(): void {
    this.isEditMode = false;
    this.selectedScoreId = null;
    this.scoreForm.reset();
  }
}
