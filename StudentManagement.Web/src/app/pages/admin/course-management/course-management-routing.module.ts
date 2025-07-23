import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CourseListComponent } from './course-list/course-list.component';
import { CourseFormComponent } from './course-form/course-form.component';

const routes: Routes = [
  {
    path: '', // /admin/courses
    component: CourseListComponent,
  },
  {
    path: 'new', // /admin/courses/new
    component: CourseFormComponent,
  },
  {
    path: 'edit/:id', // /admin/courses/edit/C001
    component: CourseFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CourseManagementRoutingModule { }
