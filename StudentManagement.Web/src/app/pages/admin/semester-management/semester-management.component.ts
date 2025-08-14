import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-semester-management',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="page">
      <router-outlet></router-outlet>
    </div>
  `,
})
export class SemesterManagementComponent {}
