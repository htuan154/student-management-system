// src/app/models/enrollment.model.ts

import { Student } from './student.model';
import { Course } from './course.model';
import { Score } from './score.model';
import { TeacherCourse } from './teacher-course.model';
import { Semester } from './Semester.model';

export interface Enrollment {
  enrollmentId: number;
  studentId: string;
  courseId: string;
  teacherCourseId?: number | null;
  status: string;
  semesterId: number;
  teacherName?: string;
  teacherId?: string;
  // Navigation properties
  student?: Student;
  course?: Course;
  teacherCourse?: TeacherCourse | null;
  score?: Score | null;
  semester?: Semester;
}
