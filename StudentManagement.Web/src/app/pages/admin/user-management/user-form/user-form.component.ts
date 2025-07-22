import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../../services/user.service';
import { RoleService } from '../../../../services/role.service';
import { StudentService } from '../../../../services/student.service';
import { EmployeeService } from '../../../../services/employees.service';
import { TeacherService } from '../../../../services/teacher.service';
import { Role, Student, Employee, Teacher } from '../../../../models';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit {
  userForm!: FormGroup;
  isEditMode = false;
  userId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Các mảng để chứa danh sách lựa chọn
  roles: Role[] = [];
  students: Student[] = [];
  employees: Employee[] = [];
  teachers: Teacher[] = [];

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private roleService: RoleService,
    private studentService: StudentService,
    private employeeService: EmployeeService,
    private teacherService: TeacherService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.userId;

    this.initForm();
    this.loadRelatedData(); // Tải tất cả dữ liệu cho các dropdown

    if (this.isEditMode && this.userId) {
      this.loadUserData(this.userId);
    }
  }

  initForm(): void {
    this.userForm = this.fb.group({
      userId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      username: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      passwordHash: ['', [this.isEditMode ? Validators.nullValidator : Validators.required]],
      roleId: ['', [Validators.required]],
      isActive: [true],
      studentId: [null, [Validators.maxLength(10)]],
      employeeId: [null, [Validators.maxLength(10)]],
      teacherId: [null, [Validators.maxLength(10)]],
      // Thêm một control để quản lý việc chọn loại liên kết
      linkType: ['none']
    });

    // Lắng nghe sự thay đổi của linkType để reset các ID khác
    this.userForm.get('linkType')?.valueChanges.subscribe(type => {
      this.userForm.patchValue({
        studentId: null,
        employeeId: null,
        teacherId: null
      });
    });
  }

  loadRelatedData(): void {
    this.roleService.getAllRoles().subscribe(data => this.roles = data);
    this.studentService.getAllStudents().subscribe(data => this.students = data);
    this.employeeService.getAllEmployees().subscribe(data => this.employees = data);
    this.teacherService.getAllTeachers().subscribe(data => this.teachers = data);
  }

  loadUserData(id: string): void {
    this.isLoading = true;
    this.userService.getUserById(id).subscribe({
      next: (user) => {
        // Xác định loại liên kết dựa trên dữ liệu của user
        let type: 'student' | 'employee' | 'teacher' | 'none' = 'none';
        if (user.studentId) type = 'student';
        else if (user.employeeId) type = 'employee';
        else if (user.teacherId) type = 'teacher';

        this.userForm.patchValue({ ...user, linkType: type });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Không thể tải dữ liệu người dùng.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.userForm.getRawValue();

    if (this.isEditMode && !formData.passwordHash) {
      delete formData.passwordHash;
    }

    const operation = this.isEditMode
      ? this.userService.updateUser(this.userId!, formData)
      : this.userService.createUser(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/users']);
      },
      error: (err) => {
        this.errorMessage = `Đã có lỗi xảy ra. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
