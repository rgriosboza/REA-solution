// Student Types
export interface Student {
    id: number
    firstName: string
    lastName: string
    email?: string
    phoneNumber?: string
    dateOfBirth: string
    grade: string
    section?: string
    enrollmentDate: string
    isActive: boolean
}

export interface CreateStudentRequest {
    firstName: string
    lastName: string
    email?: string
    phoneNumber?: string
    dateOfBirth: string
    grade: string
    section?: string
}

export interface UpdateStudentRequest {
    firstName?: string
    lastName?: string
    email?: string
    phoneNumber?: string
    grade?: string
    section?: string
    isActive?: boolean
}

// Academic Record Types
export interface AcademicRecord {
    id: number
    studentId: number
    studentName: string
    teacherId: number
    teacherName: string
    subject: string
    grade: number
    term: string
    schoolYear: number
    recordDate: string
    comments?: string
}

export interface CreateAcademicRecordRequest {
    studentId: number
    teacherId: number
    subject: string
    grade: number
    term: string
    schoolYear: number
    comments?: string
}

// OCR Types
export interface OCRProcessRequest {
    imageBase64: string
    documentType: string
}

export interface OCRProcessResponse {
    success: boolean
    extractedText: string
    data?: ProcessedData
    error?: string
}

export interface ProcessedData {
    student?: Student
    academicRecord?: AcademicRecord
    payment?: any
}

// API Response Types
export interface ApiResponse<T> {
    data: T
    success: boolean
    message?: string
}

export interface PaginatedResponse<T> {
    items: T[]
    totalCount: number
    pageSize: number
    currentPage: number
    totalPages: number
}