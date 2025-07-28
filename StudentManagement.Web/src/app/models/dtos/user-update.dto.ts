export interface UserUpdateDto {
  userId: string;
  username: string;
  email: string;
  password?: string;
  roleId: string;
  studentId?: string;
  employeeId?: string;
  teacherId?: string;
  isActive: boolean;
}
