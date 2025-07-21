import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Employee } from '../../../models';
import { EmployeeService } from '../../../services/employees.service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-management.component.html',
  styleUrls: ['./employee-management.scss']
})
export class EmployeeManagementComponent implements OnInit {
  employees: Employee[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  employeeForm: FormGroup;
  editingEmployeeId: string | null = null;

  constructor(
    private employeeService: EmployeeService,
    private fb: FormBuilder
  ) {
    this.employeeForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      hireDate: ['', Validators.required],
      phoneNumber: [''],
      department: [''],
      position: [''],
      dateOfBirth: [''],
      salary: [null],
    });
  }

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.employeeService.getAllEmployees().subscribe({
      next: (data) => {
        this.employees = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Không thể tải danh sách nhân viên.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  submitForm(): void {
    if (this.employeeForm.invalid) return;

    const formValue = this.employeeForm.value;

    if (this.editingEmployeeId) {
      this.employeeService.updateEmployee(this.editingEmployeeId, formValue).subscribe({
        next: () => {
          this.resetForm();
          this.loadEmployees();
        },
        error: (err) => {
          this.errorMessage = 'Lỗi khi cập nhật nhân viên.';
          console.error(err);
        }
      });
    } else {
      this.employeeService.createEmployee(formValue).subscribe({
        next: () => {
          this.resetForm();
          this.loadEmployees();
        },
        error: (err) => {
          this.errorMessage = 'Lỗi khi tạo nhân viên.';
          console.error(err);
        }
      });
    }
  }

  editEmployee(employee: Employee): void {
    this.editingEmployeeId = employee.employeeId;
    this.employeeForm.patchValue({
      fullName: employee.fullName,
      email: employee.email,
      hireDate: new Date(employee.hireDate).toISOString().substring(0, 10),
      phoneNumber: employee.phoneNumber || '',
      department: employee.department || '',
      position: employee.position || '',
      dateOfBirth: employee.dateOfBirth ? new Date(employee.dateOfBirth).toISOString().substring(0, 10) : '',
      salary: employee.salary
    });
  }

  deleteEmployee(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa nhân viên này không?')) {
      this.employeeService.deleteEmployee(id).subscribe({
        next: () => {
          this.loadEmployees();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'Đã có lỗi xảy ra khi xóa nhân viên.';
          console.error(err);
        }
      });
    }
  }

  resetForm(): void {
    this.employeeForm.reset();
    this.editingEmployeeId = null;
  }
}
