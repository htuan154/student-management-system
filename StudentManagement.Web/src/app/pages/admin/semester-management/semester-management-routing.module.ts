import { Routes } from '@angular/router';
import { SemesterListComponent } from './semester-list/semester-list.component';
import { SemesterFormComponent } from './semester-form/semester-form.component';

export const SEMESTER_ROUTES: Routes = [
  { path: '', component: SemesterListComponent },
  { path: 'new', component: SemesterFormComponent },
  { path: 'edit/:id', component: SemesterFormComponent }
];
