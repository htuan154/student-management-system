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
    if (this.authService.isAuthenticated()) {
      this.navigateToDashboard();
    }

    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.navigateToDashboard();

      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        if (err.status === 401) {
          this.errorMessage = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
        } else {
          this.errorMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại.';
        }
        console.error('Login failed', err);
      }
    });
  }

  private navigateToDashboard(): void {
    const role = this.authService.getUserRole();
    console.log('Role from token:', role);
    if (role === 'Admin' || role === 'SuperAdmin') {
      this.router.navigate(['/admin/users']);

    } else if (role === 'Student') {
      this.router.navigate(['/student']);
    } else {
      this.router.navigate(['/']);
    }
  }
}
