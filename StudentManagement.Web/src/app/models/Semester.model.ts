// Semester.model.ts
import { Class } from './class.model';
import { TeacherCourse } from './teacher-course.model';
import { Enrollment } from './enrollment.model';

export interface Semester {
  semesterId: number;
  semesterName: string;
  academicYear: string;
  startDate: string; // ISO date string
  endDate: string;   // ISO date string
  isActive: boolean;
  classes?: Class[];
  teacherCourses?: TeacherCourse[];
  enrollments?: Enrollment[];
}
