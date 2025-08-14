// src/app/models/teacher-course.model.ts

import { Teacher } from './teacher.model';
import { Course } from './course.model';
import { Schedule } from './Schedule.model';
import { Enrollment } from './enrollment.model';
import { Semester } from './Semester.model';

export interface TeacherCourse {
  teacherCourseId: number;
  teacherId: string;
  courseId: string;
  semesterId: number;          // thêm
  isActive: boolean;

  // navigation (nếu API include)
  teacher?: { teacherId: string; fullName?: string } | null;
  course?: { courseId: string; courseName?: string } | null;
  semester?: Semester | null;
}
