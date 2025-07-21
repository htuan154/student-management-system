// src/app/models/enrollment.model.ts

import { Student } from './student.model';
import { Teacher } from './teacher.model';
import { Course } from './course.model';
import { Score } from './score.model';

export interface Enrollment {
  enrollmentId: number;
  studentId: string;
  courseId: string;
  status: string;
  teacherId?: string | null;
  semester?: string | null;
  year?: number | null;
  student: Student;
  course: Course;
  teacher?: Teacher | null;
  score?: Score | null;
}
