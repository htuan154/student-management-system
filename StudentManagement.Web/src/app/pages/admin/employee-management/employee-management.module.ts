import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { EmployeeManagementRoutingModule } from './employee-management-routing.module';
import { EmployeeListComponent } from './employee-list/employee-list.component';
import { EmployeeFormComponent } from './employee-form/employee-form.component';

@NgModule({

  imports: [
    CommonModule,
    EmployeeManagementRoutingModule,
    ReactiveFormsModule,
    EmployeeListComponent,
    EmployeeFormComponent
  ]
})
export class EmployeeManagementModule { }
