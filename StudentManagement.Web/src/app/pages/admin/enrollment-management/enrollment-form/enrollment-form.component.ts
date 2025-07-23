import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EnrollmentService } from '../../../../services/enrollment.service';
import { StudentService } from '../../../../services/student.service';
import { CourseService } from '../../../../services/course.service';
import { TeacherService } from '../../../../services/teacher.service';
import { Student, Course, Teacher } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-enrollment-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './enrollment-form.component.html',
  styleUrls: ['./enrollment-form.component.scss']
})
export class EnrollmentFormComponent implements OnInit {
  enrollmentForm!: FormGroup;
  isEditMode = false;
  enrollmentId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Data for dropdowns
  students: Student[] = [];
  courses: Course[] = [];
  teachers: Teacher[] = [];

  constructor(
    private fb: FormBuilder,
    private enrollmentService: EnrollmentService,
    private studentService: StudentService,
    private courseService: CourseService,
    private teacherService: TeacherService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.enrollmentId = +id;
      this.isEditMode = true;
    }

    this.initForm();
    this.loadRelatedData();

    if (this.isEditMode && this.enrollmentId) {
      this.loadEnrollmentData(this.enrollmentId);
    }
  }

  initForm(): void {
    this.enrollmentForm = this.fb.group({
      studentId: [{ value: '', disabled: this.isEditMode }, [Validators.required]],
      courseId: [{ value: '', disabled: this.isEditMode }, [Validators.required]],
      teacherId: [null],
      semester: ['', [Validators.maxLength(20)]],
      year: [null],
      status: ['Enrolled', [Validators.required]]
    });
  }

  loadRelatedData(): void {
    this.studentService.getAllStudents().subscribe(data => this.students = data);
    this.courseService.getAllCourses().subscribe(data => this.courses = data);
    this.teacherService.getAllTeachers().subscribe(data => this.teachers = data);
  }

  loadEnrollmentData(id: number): void {
    this.isLoading = true;
    this.enrollmentService.getEnrollmentById(id).subscribe({
      next: (data) => {
        this.enrollmentForm.patchValue(data);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load enrollment data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.enrollmentForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.enrollmentForm.getRawValue();

    const operation = this.isEditMode
      ? this.enrollmentService.updateEnrollment(this.enrollmentId!, formData)
      : this.enrollmentService.createEnrollment(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/enrollments']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
