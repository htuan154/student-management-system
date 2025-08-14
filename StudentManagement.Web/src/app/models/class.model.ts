// src/app/models/class.model.ts
import {Semester} from './Semester.model';
import {AnnouncementDetail} from './AnnouncementDetail.model';
import { Student } from './student.model';
export interface Class {
  classId: string;
  className: string;
  major: string;
  isActive: boolean;
  semesterId: number;
  academicYear: string | null;
  students: Student[];
  semester: Semester | null;
  announcementDetails: AnnouncementDetail[];
}
