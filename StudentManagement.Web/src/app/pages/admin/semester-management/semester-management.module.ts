import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SEMESTER_ROUTES } from './semester-management.routes';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(SEMESTER_ROUTES)
  ]
})
export class SemesterManagementModule { }
