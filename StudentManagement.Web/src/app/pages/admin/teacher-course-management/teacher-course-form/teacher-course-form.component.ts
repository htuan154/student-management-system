import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TeacherCourseService } from '../../../../services/teacher-course.service';
import { TeacherService } from '../../../../services/teacher.service';
import { CourseService } from '../../../../services/course.service';
import { Teacher, Course } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-teacher-course-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './teacher-course-form.component.html',
  styleUrls: ['./teacher-course-form.component.scss']
})
export class TeacherCourseFormComponent implements OnInit {
  assignmentForm!: FormGroup;
  isEditMode = false;
  assignmentId: number | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Data for dropdowns
  teachers: Teacher[] = [];
  courses: Course[] = [];

  constructor(
    private fb: FormBuilder,
    private teacherCourseService: TeacherCourseService,
    private teacherService: TeacherService,
    private courseService: CourseService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.assignmentId = +id;
      this.isEditMode = true;
    }

    this.initForm();
    this.loadRelatedData();

    if (this.isEditMode && this.assignmentId) {
      this.loadAssignmentData(this.assignmentId);
    }
  }

  initForm(): void {
    this.assignmentForm = this.fb.group({
      teacherId: ['', [Validators.required]],
      courseId: ['', [Validators.required]],
      semester: ['', [Validators.maxLength(20)]],
      year: [null],
      isActive: [true]
    });
  }

  loadRelatedData(): void {
    this.teacherService.getAllTeachers().subscribe(data => this.teachers = data);
    this.courseService.getAllCourses().subscribe(data => this.courses = data);
  }

  loadAssignmentData(id: number): void {
    this.isLoading = true;
    this.teacherCourseService.getTeacherCourseById(id).subscribe({
      next: (data) => {
        this.assignmentForm.patchValue(data);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load assignment data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.assignmentForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.assignmentForm.value;

    const operation = this.isEditMode
      ? this.teacherCourseService.updateTeacherCourse(this.assignmentId!, formData)
      : this.teacherCourseService.createTeacherCourse(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/assignments']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
