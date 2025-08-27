import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Student, User } from '../../../models';
import { StudentService } from '../../../services/student.service';
import { UserService } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-profile-student',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './profilestudent.component.html',
  styleUrls: ['./profilestudent.component.scss']
})
export class ProfileStudentComponent implements OnInit {
  studentInfo: Student | null = null;
  userInfo: User | null = null;
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  isLoading = false;
  isEditing = false;
  isChangingPassword = false;

  successMessage = '';
  errorMessage = '';
  passwordMessage = '';
  passwordError = '';

  // Tab management
  activeTab = 'profile';

  constructor(
    private studentService: StudentService,
    private userService: UserService,
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadStudentProfile();
  }

  /**
   * Khởi tạo các form
   */
  private initializeForms(): void {
    this.profileForm = this.formBuilder.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.pattern(/^[0-9]{10,11}$/)]],
      address: [''],
      dateOfBirth: ['']
    });

    this.passwordForm = this.formBuilder.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  /**
   * Validator để kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
   */
  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');

    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  /**
   * Tải thông tin profile sinh viên
   */
  public loadStudentProfile(): void {
    this.isLoading = true;
    this.clearMessages();

    const tokenPayload = this.authService.getDecodedToken();
    if (!tokenPayload) {
      this.errorMessage = 'Không thể xác thực người dùng. Vui lòng đăng nhập lại.';
      this.isLoading = false;
      return;
    }

    const studentId = tokenPayload.studentId ||
                     tokenPayload.sub ||
                     tokenPayload.userId ||
                     tokenPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    if (!studentId) {
      this.errorMessage = 'Không tìm thấy thông tin sinh viên.';
      this.isLoading = false;
      return;
    }

    // Tải thông tin sinh viên
    this.studentService.getStudentById(studentId).subscribe({
      next: (student) => {
        this.studentInfo = student;
        this.populateProfileForm(student);

        // Tải thông tin user nếu có
        if (student.user) {
          this.userInfo = student.user;

        } else {
          this.userInfo = null;

        }

        this.isLoading = false;
      },
      error: (error) => {
        console.error('Lỗi khi tải thông tin sinh viên:', error);
        this.errorMessage = 'Không thể tải thông tin profile. Vui lòng thử lại.';
        this.isLoading = false;
      }
    });
  }

  /**
   * Điền thông tin sinh viên vào form
   */
  private populateProfileForm(student: Student): void {
    this.profileForm.patchValue({
      fullName: student.fullName || '',
      email: student.email || '',
      phoneNumber: student.phoneNumber || '',
      address: student.address || '',
      dateOfBirth: student.dateOfBirth ? this.formatDateForInput(student.dateOfBirth) : ''
    });
  }

  /**
   * Format date cho input type="date"
   */
  private formatDateForInput(date: string | Date): string {
    if (!date) return '';
    const dateObj = new Date(date);
    return dateObj.toISOString().split('T')[0];
  }

  /**
   * Bật/tắt chế độ chỉnh sửa
   */
  toggleEdit(): void {
    if (this.isEditing) {
      // Hủy chỉnh sửa - reset form về giá trị ban đầu
      if (this.studentInfo) {
        this.populateProfileForm(this.studentInfo);
      }
      this.clearMessages();
    }
    this.isEditing = !this.isEditing;
  }

  /**
   * Lưu thông tin profile
   */
  saveProfile(): void {
    if (this.profileForm.invalid || !this.studentInfo) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.isLoading = true;
    this.clearMessages();

    const formValue = this.profileForm.value;
    const updateData: Partial<Student> = {
      fullName: formValue.fullName,
      email: formValue.email,
      phoneNumber: formValue.phoneNumber,
      address: formValue.address,
      dateOfBirth: formValue.dateOfBirth || null,
      classId: this.studentInfo.classId
    };

    this.studentService.updateStudent(this.studentInfo.studentId, updateData).subscribe({
      next: (updatedStudent) => {
        this.studentInfo = updatedStudent;
        this.isEditing = false;
        this.isLoading = false;
        this.successMessage = 'Cập nhật thông tin thành công!';

        // Tự động ẩn thông báo sau 3 giây
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        console.error('Lỗi khi cập nhật thông tin:', error);
        this.errorMessage = 'Không thể cập nhật thông tin. Vui lòng thử lại.';
        this.isLoading = false;
      }
    });
  }

  /**
   * Đổi mật khẩu
   */
  public onSubmitPassword(): void {
    this.changePassword();
  }

  changePassword(): void {
  if (this.passwordForm.invalid) { this.markFormGroupTouched(this.passwordForm); return; }

  const token = this.authService.getDecodedToken?.() || {};
  const tokenUserId =
    token.userId || token.sub || token['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

  const userId = this.userInfo?.userId || tokenUserId;
  if (!userId) { this.passwordError = 'Không xác định được tài khoản người dùng.'; return; }

  this.isLoading = true;
  const payload = {
    userId,
    currentPassword: this.passwordForm.value.currentPassword,
    newPassword: this.passwordForm.value.newPassword
  };
  this.userService.changePassword(payload).subscribe({
    next: (res) => {
      this.isLoading = false;
      this.passwordMessage = (res?.message || 'Đổi mật khẩu thành công!');
      this.passwordForm.reset();
      this.isChangingPassword = false;

      setTimeout(() => (this.passwordMessage = ''), 3000);
    },
    error: (err) => {
      this.isLoading = false;
      this.passwordError = (err?.error || 'Không thể đổi mật khẩu. Vui lòng thử lại.');
    }
  });
}

  /**
   * Mark tất cả field trong form đã được touch để hiển thị validation
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.controls[key];
      control.markAsTouched();
    });
  }

  /**
   * Xóa tất cả messages
   */
  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
    this.passwordMessage = '';
    this.passwordError = '';
  }

  /**
   * Kiểm tra field có lỗi không
   */
  hasError(formGroup: FormGroup, fieldName: string): boolean {
    const field = formGroup.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  /**
   * Lấy thông báo lỗi cho field
   */
  getErrorMessage(formGroup: FormGroup, fieldName: string): string {
    const field = formGroup.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    const errors = field.errors;

    if (errors['required']) return `${this.getFieldLabel(fieldName)} là bắt buộc`;
    if (errors['email']) return 'Email không hợp lệ';
    if (errors['minlength']) return `${this.getFieldLabel(fieldName)} phải có ít nhất ${errors['minlength'].requiredLength} ký tự`;
    if (errors['pattern']) return `${this.getFieldLabel(fieldName)} không đúng định dạng`;
    if (errors['passwordMismatch']) return 'Mật khẩu xác nhận không khớp';

    return 'Dữ liệu không hợp lệ';
  }

  /**
   * Lấy label hiển thị của field
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      fullName: 'Họ và tên',
      email: 'Email',
      phoneNumber: 'Số điện thoại',
      address: 'Địa chỉ',
      dateOfBirth: 'Ngày sinh',
      currentPassword: 'Mật khẩu hiện tại',
      newPassword: 'Mật khẩu mới',
      confirmPassword: 'Xác nhận mật khẩu'
    };
    return labels[fieldName] || fieldName;
  }

  /**
   * Chuyển tab
   */
  switchTab(tab: string): void {
    this.activeTab = tab;
    this.clearMessages();

    if (tab === 'profile' && this.isChangingPassword) {
      this.isChangingPassword = false;
      this.passwordForm.reset();
    }
  }

  /**
   * Toggle form đổi mật khẩu
   */
  togglePasswordChange(): void {
    this.isChangingPassword = !this.isChangingPassword;
    if (!this.isChangingPassword) {
      this.passwordForm.reset();
      this.passwordError = '';
      this.passwordMessage = '';
    }
  }

  /**
   * Quay về trang chính
   */
  goBack(): void {
    this.router.navigate(['/student']);
  }
}
