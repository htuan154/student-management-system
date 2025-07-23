import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Class } from '../../../../models';
import { ClassService } from '../../../../services/class.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-class-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './class-list.component.html',
  styleUrls: ['./class-list.component.scss']
})
export class ClassListComponent implements OnInit {
  classes: Class[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private classService: ClassService) { }

  ngOnInit(): void {
    this.loadClasses();
  }

  loadClasses(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.classService.getAllClasses().subscribe({
      next: (data) => {
        this.classes = data;
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = 'Could not load the list of classes.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  deleteClass(id: string): void {
    if (confirm('Are you sure you want to delete this class?')) {
      this.classService.deleteClass(id).subscribe({
        next: () => {
          this.loadClasses(); // Reload the list after deletion
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = 'An error occurred while deleting the class.';
          console.error(err);
        }
      });
    }
  }
}
