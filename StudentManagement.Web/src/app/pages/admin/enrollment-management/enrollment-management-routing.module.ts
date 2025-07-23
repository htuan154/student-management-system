import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { EnrollmentListComponent } from './enrollment-list/enrollment-list.component';
import { EnrollmentFormComponent } from './enrollment-form/enrollment-form.component';

const routes: Routes = [
  {
    path: '', // /admin/enrollments
    component: EnrollmentListComponent,
  },
  {
    path: 'new', // /admin/enrollments/new
    component: EnrollmentFormComponent,
  },
  {
    path: 'edit/:id', // /admin/enrollments/edit/1
    component: EnrollmentFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EnrollmentManagementRoutingModule { }
