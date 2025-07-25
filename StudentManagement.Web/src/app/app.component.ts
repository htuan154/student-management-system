import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrls: ['./app.component.scss']

})
export class AppComponent {
  protected readonly title = signal('StudentManagement.Web');
}
