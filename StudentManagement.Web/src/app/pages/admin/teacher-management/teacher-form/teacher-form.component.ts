import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TeacherService } from '../../../../services/teacher.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-teacher-form',
  standalone: true,
  templateUrl: './teacher-form.component.html',
  styleUrls: ['./teacher-form.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ]
})
export class TeacherFormComponent implements OnInit {
  teacherForm!: FormGroup;
  isEditMode = false;
  teacherId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private teacherService: TeacherService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.teacherId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.teacherId;

    this.initForm();
    if (!this.isEditMode) {
      this.teacherForm.patchValue({ teacherId: this.generateTeacherId() });
    }

    if (this.isEditMode && this.teacherId) {
      this.loadTeacherData(this.teacherId);
    }
  }

  initForm(): void {
    this.teacherForm = this.fb.group({
      teacherId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      phoneNumber: ['', [Validators.maxLength(15)]],
      department: ['', [Validators.maxLength(50)]],
      degree: ['', [Validators.maxLength(50)]],
      dateOfBirth: [null],
      hireDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      salary: [null]
    });
  }
  private generateTeacherId(): string {
    const timePart = Date.now().toString(36).toUpperCase().slice(-5);
    const rndPart = Math.random().toString(36).toUpperCase().slice(2, 5);
    return `GV${timePart}${rndPart}`; // tổng 10 ký tự
  }

  loadTeacherData(id: string): void {
    this.isLoading = true;
    this.teacherService.getTeacherById(id).subscribe({
      next: (teacher) => {
        const dob = teacher.dateOfBirth ? new Date(teacher.dateOfBirth).toISOString().split('T')[0] : null;
        const hireDate = new Date(teacher.hireDate).toISOString().split('T')[0];
        this.teacherForm.patchValue({ ...teacher, dateOfBirth: dob, hireDate: hireDate });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load teacher data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.teacherForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.teacherForm.getRawValue();

    const operation = this.isEditMode
      ? this.teacherService.updateTeacher(this.teacherId!, formData)
      : this.teacherService.createTeacher(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/teachers']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
