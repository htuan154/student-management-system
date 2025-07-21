// src/app/models/teacher.model.ts

import { User } from './user.model';

import { TeacherCourse } from './teacher-course.model';
import { Enrollment } from './enrollment.model';

export interface Teacher {
  teacherId: string;
  fullName: string;
  email: string;
  hireDate: string | Date;

  phoneNumber?: string | null;
  department?: string | null;
  degree?: string | null;
  dateOfBirth?: string | Date | null;
  salary?: number | null;


  users?: User[];
  teacherCourses?: TeacherCourse[];
  enrollments?: Enrollment[];
}
