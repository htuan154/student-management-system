// Schedule.model.ts
import { TeacherCourse } from './teacher-course.model';
export interface Schedule {
  scheduleId: number;
  teacherCourseId: number;
  dayOfWeek: number; // 2-8
  startTime: string; // "HH:mm:ss"
  endTime: string;   // "HH:mm:ss"
  roomNumber?: string | null;
  location?: string | null;
  teacherCourse?: TeacherCourse | null;
}
