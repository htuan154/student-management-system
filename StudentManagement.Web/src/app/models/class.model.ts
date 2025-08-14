// src/app/models/class.model.ts
import {Semester} from './Semester.model';
import {AnnouncementDetail} from './AnnouncementDetail.model';
import { Student } from './student.model';
export interface Class {
  classId: string;
  className: string;
  major: string;
  isActive: boolean;
  academicYear?: string | null;
  semester?: number | null;
  semesterId?: number | null; // Bổ sung cho khớp backend
  semesterObj?: Semester | null; // Nếu có model Semester
  students?: Student[];
  announcementDetails?: AnnouncementDetail[]; // Bổ sung nếu có model AnnouncementDetail
}
