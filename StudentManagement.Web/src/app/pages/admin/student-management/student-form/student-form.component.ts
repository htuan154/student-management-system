import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { StudentService } from '../../../../services/student.service';
import { ClassService } from '../../../../services/class.service';
import { Class } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';


@Component({
  selector: 'app-student-form',
  standalone: true,
  templateUrl: './student-form.component.html',
  styleUrls: ['./student-form.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ]
})

export class StudentFormComponent implements OnInit {
  studentForm!: FormGroup;
  isEditMode = false;
  studentId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;
  classes: Class[] = [];

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService,
    private classService: ClassService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.studentId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.studentId;

    this.initForm();
    if (!this.isEditMode) {
      this.studentForm.patchValue({ studentId: this.generateStudentId() });
    }
    this.loadClasses();

    if (this.isEditMode && this.studentId) {
      this.loadStudentData(this.studentId);
    }
  }

  initForm(): void {
    this.studentForm = this.fb.group({
      studentId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      phoneNumber: ['', [Validators.maxLength(15)]],
      dateOfBirth: [null],
      address: ['', [Validators.maxLength(255)]],
      classId: ['', [Validators.required]]
    });
  }
  private generateStudentId(): string {
    const timePart = Date.now().toString(36).toUpperCase().slice(-5);
    const rndPart = Math.random().toString(36).toUpperCase().slice(2, 5);
    return `SV${timePart}${rndPart}`; // tổng 10 ký tự
  }

  loadClasses(): void {
    this.classService.getAllClasses().subscribe(data => this.classes = data);
  }

  loadStudentData(id: string): void {
    this.isLoading = true;
    this.studentService.getStudentById(id).subscribe({
      next: (student) => {
        // Chuyển đổi định dạng ngày tháng nếu cần
        const dob = student.dateOfBirth ? new Date(student.dateOfBirth).toISOString().split('T')[0] : null;
        this.studentForm.patchValue({ ...student, dateOfBirth: dob });
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = "Could not load student data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.studentForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.studentForm.getRawValue();

    const operation = this.isEditMode
      ? this.studentService.updateStudent(this.studentId!, formData)
      : this.studentService.createStudent(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/students']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
