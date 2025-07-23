import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseService } from '../../../../services/course.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-course-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './course-form.component.html',
  styleUrls: ['./course-form.component.scss']
})
export class CourseFormComponent implements OnInit {
  courseForm!: FormGroup;
  isEditMode = false;
  courseId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.courseId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.courseId;

    this.initForm();

    if (this.isEditMode && this.courseId) {
      this.loadCourseData(this.courseId);
    }
  }

  initForm(): void {
    this.courseForm = this.fb.group({
      courseId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      courseName: ['', [Validators.required, Validators.maxLength(100)]],
      credits: [0, [Validators.required, Validators.min(1)]],
      department: ['', [Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  loadCourseData(id: string): void {
    this.isLoading = true;
    this.courseService.getCourseById(id).subscribe({
      next: (courseData) => {
        this.courseForm.patchValue(courseData);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load course data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.courseForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.courseForm.getRawValue();

    const operation = this.isEditMode
      ? this.courseService.updateCourse(this.courseId!, formData)
      : this.courseService.createCourse(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/courses']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
