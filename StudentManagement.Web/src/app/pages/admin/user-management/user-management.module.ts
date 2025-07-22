import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { UserManagementRoutingModule } from './user-management-routing.module';
import { UserListComponent } from './user-list/user-list.component';
import { UserFormComponent } from './user-form/user-form.component';

@NgModule({

  imports: [
    CommonModule,
    UserManagementRoutingModule,
    UserListComponent,
    UserFormComponent,
    ReactiveFormsModule
  ]
})
export class UserManagementModule { }
