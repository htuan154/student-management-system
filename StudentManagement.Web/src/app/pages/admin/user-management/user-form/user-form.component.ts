import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, debounceTime, distinctUntilChanged, switchMap, of, EMPTY } from 'rxjs';
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
  // Form và state properties
  userForm!: FormGroup;
  isEditMode = false;
  userId: string | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Data properties
  roles: Role[] = [];
  allStudents: Student[] = [];
  allEmployees: Employee[] = [];
  allTeachers: Teacher[] = [];
  availableStudents: Student[] = [];
  availableEmployees: Employee[] = [];
  availableTeachers: Teacher[] = [];

  // Search properties
  studentSearchTerm = '';
  employeeSearchTerm = '';
  teacherSearchTerm = '';
  isSearchingStudents = false;
  isSearchingEmployees = false;
  isSearchingTeachers = false;

  // Validation state
  emailExists = false;
  usernameExists = false;
  studentIdExists = false;
  employeeIdExists = false;
  teacherIdExists = false;

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
    this.loadInitialData();
    this.setupFormListeners();
  }

  // ✅ Initialize form with validators
  private initForm(): void {
    this.userForm = this.fb.group({
      userId: [{ value: '', disabled: this.isEditMode }],
      username: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern(/^[a-zA-Z0-9_]+$/) // Only alphanumeric and underscore
      ]],
      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(100)
      ]],
      password: ['', [
        this.isEditMode ? Validators.nullValidator : Validators.required,
        Validators.minLength(6)
      ]],
      confirmPassword: [''],
      roleId: ['', [Validators.required]],
      isActive: [true],
      studentId: [null],
      employeeId: [null],
      teacherId: [null],
      linkType: ['none'] // none, student, employee, teacher
    });

    // Add password confirmation validator
    this.userForm.setValidators(this.passwordMatchValidator);
  }


  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    if (!control || !control.get) {
      return null;
    }

    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    // If no values, no validation needed
    if (!password?.value || !confirmPassword?.value) {
      return null;
    }

    // Check if passwords match
    if (password.value !== confirmPassword.value) {
      // Set error on confirmPassword field specifically
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    // Clear error if passwords match
    if (confirmPassword.hasError('passwordMismatch')) {
      const errors = confirmPassword.errors;
      delete errors?.['passwordMismatch'];
      confirmPassword.setErrors(Object.keys(errors || {}).length ? errors : null);
    }

    return null;
  }

  // ✅ Setup form listeners with validation
  private setupFormListeners(): void {
    // Link type changes
    this.userForm.get('linkType')?.valueChanges.subscribe(type => {
      this.onLinkTypeChange(type);
    });

    // Username validation
    this.userForm.get('username')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(username => {
        if (!username || username.length < 3) return of(false);
        // TODO: Add username existence check service call
        return of(false); // Placeholder
      })
    ).subscribe(exists => {
      this.usernameExists = exists;
      if (exists) {
        this.userForm.get('username')?.setErrors({ usernameExists: true });
      }
    });

    // Email validation
    this.userForm.get('email')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(email => {
        if (!email || !this.userForm.get('email')?.valid) return of(false);
        // TODO: Add email existence check service call
        return of(false); // Placeholder
      })
    ).subscribe(exists => {
      this.emailExists = exists;
      if (exists) {
        this.userForm.get('email')?.setErrors({ emailExists: true });
      }
    });

    // ID sync for create mode
    if (!this.isEditMode) {
      this.setupIdSync();
    }
  }

  // ✅ Load initial data using available services
  private loadInitialData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    forkJoin({
      roles: this.roleService.getAllRoles(),
      students: this.studentService.getAllStudents(),
      employees: this.employeeService.getAllEmployees(),
      teachers: this.teacherService.getAllTeachers(),
      users: this.userService.getAllUsers()
    }).subscribe({
      next: (data) => {
        this.roles = data.roles;
        this.allStudents = data.students;
        this.allEmployees = data.employees;
        this.allTeachers = data.teachers;

        this.filterAvailableEntities(data.users);

        if (this.isEditMode && this.userId) {
          this.loadUserData();
        } else {
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        this.errorMessage = 'Không thể tải dữ liệu ban đầu. Vui lòng thử lại.';
        this.isLoading = false;
      }
    });
  }

  // ✅ Filter available entities (those without user accounts)
  private filterAvailableEntities(existingUsers: User[]): void {
    const linkedStudentIds = new Set(existingUsers.map(u => u.studentId).filter(Boolean));
    const linkedEmployeeIds = new Set(existingUsers.map(u => u.employeeId).filter(Boolean));
    const linkedTeacherIds = new Set(existingUsers.map(u => u.teacherId).filter(Boolean));

    this.availableStudents = this.allStudents.filter(s => !linkedStudentIds.has(s.studentId));
    this.availableEmployees = this.allEmployees.filter(e => !linkedEmployeeIds.has(e.employeeId));
    this.availableTeachers = this.allTeachers.filter(t => !linkedTeacherIds.has(t.teacherId));
  }

  // ✅ Load user data for edit mode
  private loadUserData(): void {
    if (!this.userId) return;

    this.userService.getUserById(this.userId).subscribe({
      next: (user) => {
        this.populateForm(user);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading user data:', error);
        this.errorMessage = 'Không thể tải thông tin người dùng. Vui lòng thử lại.';
        this.isLoading = false;
      }
    });
  }

  // ✅ Populate form with user data
  private populateForm(user: User): void {
    let linkType: 'student' | 'employee' | 'teacher' | 'none' = 'none';

    // Determine link type and add current linked entity to available list
    if (user.studentId) {
      linkType = 'student';
      const currentStudent = this.allStudents.find(s => s.studentId === user.studentId);
      if (currentStudent && !this.availableStudents.some(s => s.studentId === user.studentId)) {
        this.availableStudents.unshift(currentStudent);
      }
    } else if (user.employeeId) {
      linkType = 'employee';
      const currentEmployee = this.allEmployees.find(e => e.employeeId === user.employeeId);
      if (currentEmployee && !this.availableEmployees.some(e => e.employeeId === user.employeeId)) {
        this.availableEmployees.unshift(currentEmployee);
      }
    } else if (user.teacherId) {
      linkType = 'teacher';
      const currentTeacher = this.allTeachers.find(t => t.teacherId === user.teacherId);
      if (currentTeacher && !this.availableTeachers.some(t => t.teacherId === user.teacherId)) {
        this.availableTeachers.unshift(currentTeacher);
      }
    }

    // Patch form values
    this.userForm.patchValue({
      userId: user.userId,
      username: user.username,
      email: user.email,
      roleId: user.roleId,
      isActive: user.isActive,
      studentId: user.studentId,
      employeeId: user.employeeId,
      teacherId: user.teacherId,
      linkType: linkType
    });

    // Disable link type change in edit mode
    this.userForm.get('linkType')?.disable();
  }

  // ✅ Handle link type changes
  private onLinkTypeChange(type: string): void {
    // Clear all link IDs
    this.userForm.patchValue({
      studentId: null,
      employeeId: null,
      teacherId: null,
      userId: ''
    });

    // Enable/disable userId based on link type
    const userIdControl = this.userForm.get('userId');
    if (type === 'none') {
      userIdControl?.enable();
    } else if (!this.isEditMode) {
      userIdControl?.disable();
    }
  }

  // ✅ Setup ID synchronization for create mode
  private setupIdSync(): void {
    this.userForm.get('studentId')?.valueChanges.subscribe(id => {
      if (this.userForm.get('linkType')?.value === 'student' && id) {
        this.userForm.patchValue({ userId: id });
      }
    });

    this.userForm.get('employeeId')?.valueChanges.subscribe(id => {
      if (this.userForm.get('linkType')?.value === 'employee' && id) {
        this.userForm.patchValue({ userId: id });
      }
    });

    this.userForm.get('teacherId')?.valueChanges.subscribe(id => {
      if (this.userForm.get('linkType')?.value === 'teacher' && id) {
        this.userForm.patchValue({ userId: id });
      }
    });
  }

  // ✅ Search methods using services
  searchStudents(): void {
    if (!this.studentSearchTerm.trim()) {
      this.filterAvailableEntities([]);
      return;
    }

    this.isSearchingStudents = true;
    this.studentService.searchStudents(this.studentSearchTerm).subscribe({
      next: (students) => {
        // Filter out students that already have user accounts
        this.availableStudents = students.filter(s =>
          !this.allStudents.some(existing => existing.studentId === s.studentId && existing.studentId !== this.userForm.get('studentId')?.value)
        );
        this.isSearchingStudents = false;
      },
      error: () => {
        this.isSearchingStudents = false;
      }
    });
  }

  searchEmployees(): void {
    if (!this.employeeSearchTerm.trim()) {
      this.filterAvailableEntities([]);
      return;
    }

    this.isSearchingEmployees = true;
    // Since EmployeeService doesn't have search method, use getAllEmployees and filter
    this.employeeService.getAllEmployees().subscribe({
      next: (employees) => {
        const term = this.employeeSearchTerm.toLowerCase();
        this.availableEmployees = employees.filter(e =>
          (e.fullName?.toLowerCase().includes(term) || e.employeeId.toLowerCase().includes(term))
        );
        this.isSearchingEmployees = false;
      },
      error: () => {
        this.isSearchingEmployees = false;
      }
    });
  }

  searchTeachers(): void {
    if (!this.teacherSearchTerm.trim()) {
      this.filterAvailableEntities([]);
      return;
    }

    this.isSearchingTeachers = true;
    this.teacherService.searchTeachers(this.teacherSearchTerm).subscribe({
      next: (teachers) => {
        this.availableTeachers = teachers.filter(t =>
          !this.allTeachers.some(existing => existing.teacherId === t.teacherId && existing.teacherId !== this.userForm.get('teacherId')?.value)
        );
        this.isSearchingTeachers = false;
      },
      error: () => {
        this.isSearchingTeachers = false;
      }
    });
  }

  // ✅ Form submission
  onSubmit(): void {
    if (this.userForm.invalid) {
      this.markFormGroupTouched();
      this.scrollToFirstError();
      return;
    }

    this.isSaving = true;
    this.errorMessage = null;
    this.successMessage = null;

    const operation = this.isEditMode ? this.updateUser() : this.createUser();

    operation.subscribe({
      next: () => {
        this.successMessage = `${this.isEditMode ? 'Cập nhật' : 'Tạo'} người dùng thành công!`;
        setTimeout(() => {
          this.router.navigate(['/admin/users']);
        }, 1500);
      },
      error: (error) => {
        console.error('Error saving user:', error);
        this.errorMessage = this.getErrorMessage(error);
        this.isSaving = false;
      }
    });
  }

  // ✅ Create user
  private createUser() {
    const formValue = this.userForm.getRawValue();
    const createDto: UserCreateDto = {
      userId: formValue.userId,
      username: formValue.username,
      email: formValue.email,
      password: formValue.password,
      roleId: formValue.roleId,
      isActive: !!formValue.isActive,
      studentId: formValue.studentId,
      employeeId: formValue.employeeId,
      teacherId: formValue.teacherId
    };

    return this.userService.createUser(createDto);
  }

  // ✅ Update user
  private updateUser() {
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

    // Only include password if provided
    if (formValue.password?.trim()) {
      updateDto.password = formValue.password;
    }

    return this.userService.updateUser(this.userId!, updateDto);
  }

  // ✅ Utility methods
  private markFormGroupTouched(): void {
    Object.keys(this.userForm.controls).forEach(key => {
      const control = this.userForm.get(key);
      control?.markAsTouched();
      if (control && 'controls' in control) {
        this.markFormGroupTouched();
      }
    });
  }

  private scrollToFirstError(): void {
    const firstError = document.querySelector('.is-invalid');
    if (firstError) {
      firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  private getErrorMessage(error: any): string {
    if (error?.error?.message) {
      return error.error.message;
    }
    if (error?.message) {
      return error.message;
    }
    return 'Đã có lỗi xảy ra. Vui lòng thử lại.';
  }

  // ✅ Navigation
  onCancel(): void {
    if (this.userForm.dirty) {
      if (confirm('Bạn có chắc chắn muốn hủy? Tất cả thay đổi sẽ bị mất.')) {
        this.router.navigate(['/admin/users']);
      }
    } else {
      this.router.navigate(['/admin/users']);
    }
  }

  // ✅ Getter methods for template
  get linkType() {
    return this.userForm.get('linkType')?.value;
  }

  get isStudentLinkType() {
    return this.linkType === 'student';
  }

  get isEmployeeLinkType() {
    return this.linkType === 'employee';
  }

  get isTeacherLinkType() {
    return this.linkType === 'teacher';
  }

  get isNoneLinkType() {
    return this.linkType === 'none';
  }

  // ✅ Form control getters for validation
  get username() { return this.userForm.get('username'); }
  get email() { return this.userForm.get('email'); }
  get password() { return this.userForm.get('password'); }
  get confirmPassword() { return this.userForm.get('confirmPassword'); }
  get roleId() { return this.userForm.get('roleId'); }
  get userId_() { return this.userForm.get('userId'); }
  get studentId() { return this.userForm.get('studentId'); }
  get employeeId() { return this.userForm.get('employeeId'); }
  get teacherId() { return this.userForm.get('teacherId'); }

  // ✅ Validation helper methods
  hasError(fieldName: string, errorType: string): boolean {
    const field = this.userForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }

  getErrorMessage_(fieldName: string): string {
    const field = this.userForm.get(fieldName);
    if (!field?.errors || !field?.touched) return '';

    const errors = field.errors;

    switch (fieldName) {
      case 'username':
        if (errors['required']) return 'Tên đăng nhập là bắt buộc';
        if (errors['minlength']) return 'Tên đăng nhập phải có ít nhất 3 ký tự';
        if (errors['maxlength']) return 'Tên đăng nhập không được vượt quá 50 ký tự';
        if (errors['pattern']) return 'Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới';
        if (errors['usernameExists']) return 'Tên đăng nhập đã tồn tại';
        break;

      case 'email':
        if (errors['required']) return 'Email là bắt buộc';
        if (errors['email']) return 'Email không hợp lệ';
        if (errors['maxlength']) return 'Email không được vượt quá 100 ký tự';
        if (errors['emailExists']) return 'Email đã tồn tại';
        break;

      case 'password':
        if (errors['required']) return 'Mật khẩu là bắt buộc';
        if (errors['minlength']) return 'Mật khẩu phải có ít nhất 6 ký tự';
        break;

      case 'confirmPassword':
        if (errors['passwordMismatch']) return 'Xác nhận mật khẩu không khớp';
        break;

      case 'roleId':
        if (errors['required']) return 'Vai trò là bắt buộc';
        break;

      case 'userId':
        if (errors['required']) return 'Mã người dùng là bắt buộc';
        break;
    }

    return 'Trường này không hợp lệ';
  }

  // ✅ Display helper methods
  getRoleName(roleId: string): string {
    const role = this.roles.find(r => r.roleId === roleId);
    return role?.roleName || roleId;
  }

  getStudentDisplayName(student: Student): string {
    return `${student.studentId} - ${student.fullName}`;
  }

  getEmployeeDisplayName(employee: Employee): string {
    return `${employee.employeeId} - ${employee.fullName}`;
  }

  getTeacherDisplayName(teacher: Teacher): string {
    return `${teacher.teacherId} - ${teacher.fullName}`;
  }
}
