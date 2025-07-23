import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ScoreService } from '../../../../services/score.service';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { Enrollment } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-score-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './score-form.component.html',
  styleUrls: ['./score-form.component.scss']
})
export class ScoreFormComponent implements OnInit {
  scoreForm!: FormGroup;
  isEditMode = false;
  scoreId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  enrollments: Enrollment[] = []; // Dữ liệu cho dropdown

  constructor(
    private fb: FormBuilder,
    private scoreService: ScoreService,
    private enrollmentService: EnrollmentService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.scoreId = +id;
      this.isEditMode = true;
    }

    this.initForm();
    this.loadEnrollments();

    if (this.isEditMode && this.scoreId) {
      this.loadScoreData(this.scoreId);
    }
  }

  initForm(): void {
    this.scoreForm = this.fb.group({
      enrollmentId: [{ value: null, disabled: this.isEditMode }, Validators.required],
      processScore: [null, [Validators.min(0), Validators.max(10)]],
      midtermScore: [null, [Validators.min(0), Validators.max(10)]],
      finalScore: [null, [Validators.min(0), Validators.max(10)]]
    });
  }

  loadEnrollments(): void {
    // Lấy danh sách đăng ký để hiển thị trong dropdown
    this.enrollmentService.getPagedEnrollments(1, 1000).subscribe(data => {
      this.enrollments = data.enrollments;
    });
  }

  loadScoreData(id: number): void {
    this.isLoading = true;
    this.scoreService.getScoreById(id).subscribe({
      next: (scoreData) => {
        this.scoreForm.patchValue(scoreData);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load score data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.scoreForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.scoreForm.getRawValue();

    const operation = this.isEditMode
      ? this.scoreService.updateScore(this.scoreId!, formData)
      : this.scoreService.createScore(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/scores']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
