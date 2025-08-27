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
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.navigateToDashboard();
      return;
    }

    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.navigateToDashboard();
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        this.handleLoginError(err);
      }
    });
  }

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

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  private navigateToDashboard(): void {
    const role = this.authService.getUserRole();
    if (!role) {
      this.router.navigate(['/']);
      return;
    }

    const roleRoutes: { [key: string]: string } = {
      Admin: '/admin/dashboard',
      SuperAdmin: '/admin/dashboard',
      Student: '/student/dashboard',
      Teacher: '/teacher/dashboard',
      Employee: '/employee/dashboard'
    };

    const targetRoute = roleRoutes[role] || '/';
    this.router.navigate([targetRoute]).catch(() => this.router.navigate(['/']));
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field?.hasError(errorType) && field?.touched);
  }

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

  clearError(): void {
    this.errorMessage = null;
  }
}
