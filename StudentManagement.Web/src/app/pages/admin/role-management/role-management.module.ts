import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { RoleManagementRoutingModule } from './role-management-routing.module';
import { RoleListComponent } from './role-list/role-list.component';
import { RoleFormComponent } from './role-form/role-form.component';

@NgModule({
  imports: [
    CommonModule,
    RoleManagementRoutingModule,
    ReactiveFormsModule,
    RoleListComponent,
    RoleFormComponent
  ]
})
export class RoleManagementModule { }
