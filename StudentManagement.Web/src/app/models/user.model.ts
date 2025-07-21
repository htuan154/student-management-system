// src/app/models/user.model.ts

import { Role } from './role.model';
import { Student } from './student.model';
import { Employee } from './employee.model';
import { Teacher } from './teacher.model';

export interface User {
  userId: string;
  username: string;
  email: string;
  passwordHash?: string;
  roleId: string;
  isActive: boolean;

  studentId?: string | null;
  employeeId?: string | null;
  teacherId?: string | null;

  refreshToken?: string | null;
  refreshTokenExpiryTime?: Date | string | null;
  role: Role;
  student?: Student | null;
  employee?: Employee | null;
  teacher?: Teacher | null;
}
