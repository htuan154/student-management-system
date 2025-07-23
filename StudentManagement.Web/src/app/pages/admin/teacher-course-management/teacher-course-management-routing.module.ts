import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { TeacherCourseListComponent } from './teacher-course-list/teacher-course-list.component';
import { TeacherCourseFormComponent } from './teacher-course-form/teacher-course-form.component';

const routes: Routes = [
  {
    path: '', // /admin/assignments
    component: TeacherCourseListComponent,
  },
  {
    path: 'new', // /admin/assignments/new
    component: TeacherCourseFormComponent,
  },
  {
    path: 'edit/:id', // /admin/assignments/edit/1
    component: TeacherCourseFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TeacherCourseManagementRoutingModule { }
