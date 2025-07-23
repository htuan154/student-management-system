import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { ClassManagementRoutingModule } from './class-management-routing.module';
import { ClassListComponent } from './class-list/class-list.component';
import { ClassFormComponent } from './class-form/class-form.component';

@NgModule({

  imports: [
    CommonModule,
    ClassManagementRoutingModule,
    ReactiveFormsModule,
    ClassListComponent,
    ClassFormComponent
  ]
})
export class ClassManagementModule { }
