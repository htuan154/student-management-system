import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // ✅ Kiểm tra nếu đã đăng nhập
    if (this.authService.isAuthenticated()) {
      console.log('User already authenticated, redirecting...');
      this.navigateToDashboard();
      return;
    }

    // ✅ Khởi tạo form với validation
    this.loginForm = this.fb.group({
      username: ['admin', [Validators.required, Validators.minLength(3)]], // Default for testing
      password: ['123456@', [Validators.required, Validators.minLength(6)]] // Default for testing
    });
  }

  onSubmit(): void {
    console.log('Form submitted with values:', this.loginForm.value);

    // ✅ Kiểm tra form validation
    if (this.loginForm.invalid) {
      console.log('Form is invalid');
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    console.log('Sending login request...');

    // ✅ Gọi API login
    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.isLoading = false;
        this.navigateToDashboard();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Login error:', err);
        this.isLoading = false;
        this.handleLoginError(err);
      }
    });
  }

  // ✅ Xử lý lỗi đăng nhập chi tiết
  private handleLoginError(err: HttpErrorResponse): void {
    switch (err.status) {
      case 401:
        this.errorMessage = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
        break;
      case 400:
        this.errorMessage = 'Dữ liệu đăng nhập không hợp lệ.';
        break;
      case 0:
        this.errorMessage = 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.';
        break;
      case 500:
        this.errorMessage = 'Lỗi server. Vui lòng thử lại sau.';
        break;
      default:
        this.errorMessage = `Đã có lỗi xảy ra (${err.status}). Vui lòng thử lại.`;
    }
  }

  // ✅ Mark tất cả controls là touched để hiện validation
  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  // ✅ Điều hướng theo role
  private navigateToDashboard(): void {
    const role = this.authService.getUserRole();
    console.log('User role from token:', role);

    // ✅ Kiểm tra role null/undefined trước
    if (!role) {
      console.warn('No role found, redirecting to home');
      this.router.navigate(['/']);
      return;
    }

    // ✅ Mapping role với route
    const roleRoutes: { [key: string]: string } = {
      'Admin': '/admin/dashboard',
      'SuperAdmin': '/admin/dashboard',
      'Student': '/student/dashboard',
      'Teacher': '/teacher/dashboard',
      'Employee': '/employee/dashboard'
    };

    // ✅ Sử dụng role sau khi đã check null
    const targetRoute = roleRoutes[role] || '/';
    console.log('Navigating to:', targetRoute);

    // ✅ Navigate với error handling
    this.router.navigate([targetRoute]).then(
      (success) => {
        if (success) {
          console.log('Navigation successful to:', targetRoute);
        } else {
          console.warn('Navigation failed to:', targetRoute);
          this.router.navigate(['/']); // Fallback route
        }
      },
      (error) => {
        console.error('Navigation error:', error);
        this.router.navigate(['/']); // Fallback route
      }
    );
  }

  // ✅ Getter methods cho template validation
  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }

  // ✅ Helper method để kiểm tra field có lỗi không
  hasError(fieldName: string, errorType: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }

  // ✅ Helper method để lấy error message
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${fieldName === 'username' ? 'Tên đăng nhập' : 'Mật khẩu'} là bắt buộc`;
    }
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `${fieldName === 'username' ? 'Tên đăng nhập' : 'Mật khẩu'} phải có ít nhất ${minLength} ký tự`;
    }
    return '';
  }

  // ✅ Method clear error message
  clearError(): void {
    this.errorMessage = null;
  }
}
