import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../../../services/employees.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-employee-form',
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ]
})
export class EmployeeFormComponent implements OnInit {
  employeeForm!: FormGroup;
  isEditMode = false;
  employeeId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.employeeId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.employeeId;

    this.initForm();

    if (this.isEditMode && this.employeeId) {
      this.loadEmployeeData(this.employeeId);
    }
  }

  initForm(): void {
    this.employeeForm = this.fb.group({
      employeeId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      fullName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      hireDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      phoneNumber: ['', [Validators.maxLength(15)]],
      department: ['', [Validators.maxLength(50)]],
      position: ['', [Validators.maxLength(50)]],
      dateOfBirth: [null],
      salary: [null]
    });
  }

  loadEmployeeData(id: string): void {
    this.isLoading = true;
    this.employeeService.getEmployeeById(id).subscribe({
      next: (employee) => {
        const dob = employee.dateOfBirth ? new Date(employee.dateOfBirth).toISOString().split('T')[0] : null;
        const hireDate = new Date(employee.hireDate).toISOString().split('T')[0];
        this.employeeForm.patchValue({ ...employee, dateOfBirth: dob, hireDate: hireDate });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load employee data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.employeeForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.employeeForm.getRawValue();

    const operation = this.isEditMode
      ? this.employeeService.updateEmployee(this.employeeId!, formData)
      : this.employeeService.createEmployee(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/employees']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }
}
