import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { EnrollmentManagementRoutingModule } from './enrollment-management-routing.module';
import { EnrollmentListComponent } from './enrollment-list/enrollment-list.component';
import { EnrollmentFormComponent } from './enrollment-form/enrollment-form.component';

@NgModule({
  
  imports: [
    CommonModule,
    EnrollmentManagementRoutingModule,
    ReactiveFormsModule,
    EnrollmentListComponent,
    EnrollmentFormComponent
  ]
})
export class EnrollmentManagementModule { }
