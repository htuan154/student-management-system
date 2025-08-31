import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleService } from '../../../../services/role.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-role-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './role-form.component.html',
  styleUrls: ['./role-form.component.scss']
})
export class RoleFormComponent implements OnInit {
  roleForm!: FormGroup;
  isEditMode = false;
  roleId: string | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.roleId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.roleId;

    this.initForm();
    if (!this.isEditMode) {
      this.roleForm.patchValue({ roleId: this.generateRoleId() });
    }
    if (this.isEditMode && this.roleId) {
      this.loadRoleData(this.roleId);
    }
  }

  initForm(): void {
    this.roleForm = this.fb.group({
      roleId: [{ value: '', disabled: this.isEditMode }, [Validators.required, Validators.maxLength(10)]],
      roleName: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(255)]]
    });
  }
  private generateRoleId(): string {
    const timePart = Date.now().toString(36).toUpperCase().slice(-5);
    const rndPart = Math.random().toString(36).toUpperCase().slice(2, 3);
    return `RL${timePart}${rndPart}`; // 'RL' + 5 + 3 = 10 ký tự
  }

  loadRoleData(id: string): void {
    this.isLoading = true;
    this.roleService.getRoleById(id).subscribe({
      next: (roleData) => {
        this.roleForm.patchValue(roleData);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not load role data.";
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.roleForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formData = this.roleForm.getRawValue();

    const operation = this.isEditMode
      ? this.roleService.updateRole(this.roleId!, formData)
      : this.roleService.createRole(formData);

    operation.subscribe({
      next: () => {
        this.router.navigate(['/admin/roles']);
      },
      error: (err) => {
        this.errorMessage = `An error occurred. ${err.error?.message || ''}`;
        this.isLoading = false;
      }
    });
  }


}
