import { Routes } from '@angular/router';

export const SEMESTER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./semester-list/semester-list.component').then(c => c.SemesterListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./semester-form/semester-form.component').then(c => c.SemesterFormComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./semester-form/semester-form.component').then(c => c.SemesterFormComponent)
  }
];
