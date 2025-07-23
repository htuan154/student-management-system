import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ClassService } from '../../../../services/class.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-class-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './class-form.component.html',
  styleUrls: ['./class-form.component.scss']
})
export class ClassFormComponent implements OnInit {
  classForm!: FormGroup;
  isEditMode = false;
  classId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private classService: ClassService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.classId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.classId;

    this.initForm();

    if (this.isEditMode && this.classId) {
      this.loadClassData(this.classId);
    }
  }

  initForm(): void {
    this.classForm = this.fb.group({
      classId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      className: ['', [Validators.required, Validators.maxLength(50)]],
      major: ['', [Validators.required, Validators.maxLength(100)]],
      academicYear: ['', [Validators.maxLength(20)]],
      semester: [null],
      isActive: [true]
    });
  }

  loadClassData(id: string): void {
    this.isLoading = true;
    this.classService.getClassById(id).subscribe({
      next: (classData) => {
        this.classForm.patchValue(classData);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load class data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.classForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.classForm.getRawValue();

    const operation = this.isEditMode
      ? this.classService.updateClass(this.classId!, formData)
      : this.classService.createClass(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/classes']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/classes']);
  }
}
