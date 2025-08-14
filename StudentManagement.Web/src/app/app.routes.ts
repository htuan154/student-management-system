import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import các guard để bảo vệ route
import { AuthGuard } from './guards/auth-guard';
import { RoleGuard } from './guards/role-guard';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadChildren: () => import('./pages/login/login.module').then(m => m.LoginModule)
  },
  {
    path: 'admin',
    loadChildren: () => import('./pages/admin/admin.module').then(m => m.AdminModule),
    canActivate: [AuthGuard, RoleGuard],
    data: {
      expectedRole: 'Admin'
    }
  },
  {
    path: 'student',
    loadChildren: () => import('./pages/student/student.module').then(m => m.StudentModule),
    canActivate: [AuthGuard, RoleGuard],
    data: {
      expectedRole: 'Student'
    }
  },
  {
    path: 'teacher',
    loadChildren: () => import('./pages/teacher/teacher.module').then(m => m.TeacherModule),
    canActivate: [AuthGuard, RoleGuard],
    data: {
      expectedRole: 'Teacher'
    }
  },
 
  {
    path: '**',
    redirectTo: 'login'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
