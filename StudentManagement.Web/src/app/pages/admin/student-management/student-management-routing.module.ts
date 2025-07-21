import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { StudentListComponent } from './student-list/student-list.component';
import { StudentFormComponent } from './student-form/student-form.component';

const routes: Routes = [
  {
    path: '', // /admin/students
    component: StudentListComponent,
  },
  {
    path: 'new', // /admin/students/new
    component: StudentFormComponent,
  },
  {
    path: 'edit/:id', // /admin/students/edit/S001
    component: StudentFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentManagementRoutingModule { }
