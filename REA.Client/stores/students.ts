import { defineStore } from 'pinia'

export const useStudentsStore = defineStore('students', () => {
    const students = ref<any[]>([])
    const isLoading = ref(false)

    const fetchStudents = async () => {
        isLoading.value = true
        // Simulate API call
        setTimeout(() => {
            students.value = [
                { id: 1, firstName: 'John', lastName: 'Doe', grade: '1st', section: 'A' },
                { id: 2, firstName: 'Jane', lastName: 'Smith', grade: '2nd', section: 'B' }
            ]
            isLoading.value = false
        }, 1000)
    }

    return {
        students,
        isLoading,
        fetchStudents
    }
})