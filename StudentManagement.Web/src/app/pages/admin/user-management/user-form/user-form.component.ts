import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { UserService } from '../../../../services/user.service';
import { RoleService } from '../../../../services/role.service';
import { StudentService } from '../../../../services/student.service';
import { EmployeeService } from '../../../../services/employees.service';
import { TeacherService } from '../../../../services/teacher.service';
import { Role, Student, Employee, Teacher, User } from '../../../../models';
import { UserUpdateDto } from '../../../../models/dtos/user-update.dto';
import { UserCreateDto } from '../../../../models/dtos/user-create.dto';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-form',
  standalone: true,
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

  // Dữ liệu gốc từ API
  roles: Role[] = [];
  private allStudents: Student[] = [];
  private allEmployees: Employee[] = [];
  private allTeachers: Teacher[] = [];

  // **CẬP NHẬT: Các mảng đã được lọc để hiển thị trên dropdown**
  availableStudents: Student[] = [];
  availableEmployees: Employee[] = [];
  availableTeachers: Teacher[] = [];

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
    this.loadRelatedData();
    this.setupIdSync();
  }

  initForm(): void {
    this.userForm = this.fb.group({
      userId: [{ value: '', disabled: true }],
      username: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      password: ['', [this.isEditMode ? Validators.nullValidator : Validators.required]],
      roleId: ['', [Validators.required]],
      isActive: [true],
      studentId: [null],
      employeeId: [null],
      teacherId: [null],
      linkType: ['none']
    });

    this.userForm.get('linkType')?.valueChanges.subscribe(type => {
      if (this.userForm.get('linkType')?.dirty) {
        this.userForm.patchValue({ studentId: null, employeeId: null, teacherId: null, userId: '' });
      }
    });
  }

  private setupIdSync(): void {
    if (!this.isEditMode) {
      this.userForm.get('studentId')?.valueChanges.subscribe(id => {
        if (this.userForm.get('linkType')?.value === 'student') this.userForm.patchValue({ userId: id || '' });
      });
      this.userForm.get('employeeId')?.valueChanges.subscribe(id => {
        if (this.userForm.get('linkType')?.value === 'employee') this.userForm.patchValue({ userId: id || '' });
      });
      this.userForm.get('teacherId')?.valueChanges.subscribe(id => {
        if (this.userForm.get('linkType')?.value === 'teacher') this.userForm.patchValue({ userId: id || '' });
      });
    }
  }

  /**
   * **CẬP NHẬT: Tải tất cả dữ liệu cần thiết, bao gồm cả danh sách người dùng hiện có để lọc**
   */
  loadRelatedData(): void {
    this.isLoading = true;
    forkJoin({
      roles: this.roleService.getAllRoles(),
      students: this.studentService.getAllStudents(),
      employees: this.employeeService.getAllEmployees(),
      teachers: this.teacherService.getAllTeachers(),
      users: this.userService.getAllUsers()
    }).subscribe({
      next: ({ roles, students, employees, teachers, users }) => {
        this.roles = roles;
        this.allStudents = students;
        this.allEmployees = employees;
        this.allTeachers = teachers;

        this.filterAvailableEntities(users); // Lọc danh sách sau khi có dữ liệu

        // Nếu là chế độ sửa, tải dữ liệu của user đó
        if (this.isEditMode && this.userId) {
          this.loadUserData(this.userId);
        } else {
          this.isLoading = false;
        }
      },
      error: () => {
        this.errorMessage = "Không thể tải dữ liệu cần thiết.";
        this.isLoading = false;
      }
    });
  }

  /**
   * **CẬP NHẬT: Hàm mới để lọc ra các cá nhân chưa có tài khoản**
   */
  private filterAvailableEntities(existingUsers: User[]): void {
    const linkedStudentIds = new Set(existingUsers.map(u => u.studentId).filter(Boolean));
    const linkedEmployeeIds = new Set(existingUsers.map(u => u.employeeId).filter(Boolean));
    const linkedTeacherIds = new Set(existingUsers.map(u => u.teacherId).filter(Boolean));

    this.availableStudents = this.allStudents.filter(s => !linkedStudentIds.has(s.studentId));
    this.availableEmployees = this.allEmployees.filter(e => !linkedEmployeeIds.has(e.employeeId));
    this.availableTeachers = this.allTeachers.filter(t => !linkedTeacherIds.has(t.teacherId));
  }

  loadUserData(id: string): void {
    // isLoading đã được set là true trong loadRelatedData
    this.userService.getUserById(id).subscribe({
      next: (user) => {
        let type: 'student' | 'employee' | 'teacher' | 'none' = 'none';
        if (user.studentId) {
            type = 'student';
            // Thêm lại sinh viên hiện tại vào danh sách nếu họ không có ở đó
            if (!this.availableStudents.some(s => s.studentId === user.studentId)) {
                const currentStudent = this.allStudents.find(s => s.studentId === user.studentId);
                if (currentStudent) this.availableStudents.unshift(currentStudent);
            }
        }
        else if (user.employeeId) type = 'employee';
        else if (user.teacherId) type = 'teacher';

        const { passwordHash, ...userData } = user;
        this.userForm.patchValue({
            ...userData,
            linkType: type,
            isActive: String(user.isActive) === 'true'
        });

        this.userForm.get('linkType')?.disable();
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
      this.userForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const operation = this.isEditMode
      ? this.handleUpdate()
      : this.handleCreate();

    operation.subscribe({
      next: () => this.router.navigate(['/admin/users']),
      error: (err) => {
        this.errorMessage = `Đã có lỗi xảy ra. ${err.error?.message || 'Vui lòng thử lại.'}`;
        this.isLoading = false;
      }
    });
  }

  private handleCreate() {
    const formValue = this.userForm.getRawValue();
    const createDto: UserCreateDto = { ...formValue, isActive: !!formValue.isActive };
    return this.userService.createUser(createDto);
  }

  private handleUpdate() {
    const formValue = this.userForm.getRawValue();
    const updateDto: UserUpdateDto = {
        userId: this.userId!,
        username: formValue.username,
        email: formValue.email,
        roleId: formValue.roleId,
        isActive: !!formValue.isActive,
        studentId: formValue.studentId,
        employeeId: formValue.employeeId,
        teacherId: formValue.teacherId
    };

    if (formValue.password) {
        updateDto.password = formValue.password;
    }

    return this.userService.updateUser(this.userId!, updateDto);
  }
}
