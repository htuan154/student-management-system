import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { EmployeeListComponent } from './employee-list/employee-list.component';
import { EmployeeFormComponent } from './employee-form/employee-form.component';

const routes: Routes = [
  {
    path: '', // /admin/employees
    component: EmployeeListComponent,
  },
  {
    path: 'new', // /admin/employees/new
    component: EmployeeFormComponent,
  },
  {
    path: 'edit/:id', // /admin/employees/edit/E001
    component: EmployeeFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmployeeManagementRoutingModule { }
