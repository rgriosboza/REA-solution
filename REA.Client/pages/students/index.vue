<template>
  <div>
    <!-- Header with OCR Test Button -->
    <div class="flex justify-between items-center mb-8">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 mb-2">
          <i class="pi pi-users mr-3"></i>
          Gestión de Estudiantes
        </h1>
        <p class="text-gray-600">
          {{ students.length }} estudiante{{ students.length !== 1 ? 's' : '' }} registrado{{ students.length !== 1 ? 's' : '' }}
        </p>
      </div>
      <div class="flex space-x-2">
        <Button
            label="Probar OCR"
            icon="pi pi-camera"
            severity="info"
            @click="navigateToOCR"
        />
        <Button
            label="Agregar Estudiante"
            icon="pi pi-plus"
            @click="openCreateDialog"
            :disabled="isLoading"
        />
      </div>
    </div>

    <!-- Rest of the students page content... -->
    <Card class="mb-6">
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <!-- Search -->
          <div class="md:col-span-2">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Buscar
            </label>
            <div class="relative">
              <i class="pi pi-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
              <InputText
                  v-model="searchQuery"
                  placeholder="Buscar por nombre, email..."
                  class="w-full pl-10"
              />
            </div>
          </div>

          <!-- Grade Filter -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Grado
            </label>
            <Dropdown
                v-model="filterGrade"
                :options="gradeOptions"
                optionLabel="label"
                optionValue="value"
                placeholder="Todos los grados"
                class="w-full"
            />
          </div>

          <!-- Status Filter -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Estado
            </label>
            <Dropdown
                v-model="filterStatus"
                :options="statusOptions"
                optionLabel="label"
                optionValue="value"
                placeholder="Todos"
                class="w-full"
            />
          </div>
        </div>
      </template>
    </Card>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center items-center py-12">
      <ProgressSpinner />
    </div>

    <!-- Error State -->
    <Message v-else-if="error" severity="error" :closable="true" @close="error = null">
      {{ error }}
    </Message>

    <!-- Students Table -->
    <Card v-else>
      <template #content>
        <DataTable
            :value="filteredStudents"
            :paginator="true"
            :rows="10"
            :rowsPerPageOptions="[5, 10, 20, 50]"
            responsiveLayout="scroll"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Mostrando {first} a {last} de {totalRecords} estudiantes"
            class="p-datatable-sm"
        >
          <Column field="id" header="ID" :sortable="true" style="width: 80px">
            <template #body="{ data }">
              <span class="font-mono text-sm text-gray-600">#{{ data.id }}</span>
            </template>
          </Column>

          <Column header="Nombre" :sortable="true" sortField="firstName">
            <template #body="{ data }">
              <div class="flex items-center space-x-3">
                <div class="h-10 w-10 bg-primary-100 rounded-full flex items-center justify-center">
                  <span class="text-primary-700 font-semibold">
                    {{ data.firstName.charAt(0) }}{{ data.lastName.charAt(0) }}
                  </span>
                </div>
                <div>
                  <p class="font-medium text-gray-900">
                    {{ data.firstName }} {{ data.lastName }}
                  </p>
                  <p class="text-sm text-gray-500" v-if="data.email">
                    {{ data.email }}
                  </p>
                </div>
              </div>
            </template>
          </Column>

          <Column field="grade" header="Grado" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <Tag :value="data.grade" severity="info" />
            </template>
          </Column>

          <Column field="section" header="Sección" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <span v-if="data.section" class="text-gray-700">{{ data.section }}</span>
              <span v-else class="text-gray-400 italic">N/A</span>
            </template>
          </Column>

          <Column header="Edad" :sortable="true" sortField="dateOfBirth" style="width: 100px">
            <template #body="{ data }">
              {{ calculateAge(data.dateOfBirth) }} años
            </template>
          </Column>

          <Column field="isActive" header="Estado" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <Tag
                  :value="data.isActive ? 'Activo' : 'Inactivo'"
                  :severity="data.isActive ? 'success' : 'danger'"
              />
            </template>
          </Column>

          <Column header="Acciones" style="width: 180px">
            <template #body="{ data }">
              <div class="flex space-x-2">
                <Button
                    icon="pi pi-eye"
                    severity="info"
                    size="small"
                    @click="viewStudent(data)"
                    v-tooltip.top="'Ver detalles'"
                />
                <Button
                    icon="pi pi-pencil"
                    severity="warning"
                    size="small"
                    @click="editStudent(data)"
                    v-tooltip.top="'Editar'"
                />
                <Button
                    icon="pi pi-trash"
                    severity="danger"
                    size="small"
                    @click="confirmDelete(data)"
                    v-tooltip.top="'Eliminar'"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Dialogs remain the same... -->
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

definePageMeta({
  middleware: ['auth']
})

const router = useRouter()
const { $api } = useNuxtApp()

// State
const students = ref<any[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const showDialog = ref(false)
const showViewDialog = ref(false)
const dialogMode = ref<'create' | 'edit'>('create')
const isSubmitting = ref(false)
const selectedStudent = ref<any>(null)

// Filters
const searchQuery = ref('')
const filterGrade = ref<string | null>(null)
const filterStatus = ref<boolean | null>(null)

// Form
const formData = ref({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  dateOfBirth: null as Date | null,
  grade: '',
  section: ''
})

const formErrors = ref({
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  grade: ''
})

// Options
const gradeOptions = [
  { label: 'Todos', value: null },
  { label: '1er Grado', value: '1st' },
  { label: '2do Grado', value: '2nd' },
  { label: '3er Grado', value: '3rd' },
  { label: '4to Grado', value: '4th' },
  { label: '5to Grado', value: '5th' },
  { label: '6to Grado', value: '6th' }
]

const statusOptions = [
  { label: 'Todos', value: null },
  { label: 'Activos', value: true },
  { label: 'Inactivos', value: false }
]

// Computed
const filteredStudents = computed(() => {
  let filtered = students.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(s =>
        s.firstName.toLowerCase().includes(query) ||
        s.lastName.toLowerCase().includes(query) ||
        s.email?.toLowerCase().includes(query)
    )
  }

  if (filterGrade.value) {
    filtered = filtered.filter(s => s.grade === filterGrade.value)
  }

  if (filterStatus.value !== null) {
    filtered = filtered.filter(s => s.isActive === filterStatus.value)
  }

  return filtered
})

// Methods
const navigateToOCR = () => {
  router.push('/ocr')
}

const fetchStudents = async () => {
  isLoading.value = true
  error.value = null

  try {
    const data = await $api('/students')
    students.value = data
  } catch (err: any) {
    error.value = err.data?.message || 'Error al cargar estudiantes'
    console.error('Error fetching students:', err)
  } finally {
    isLoading.value = false
  }
}

const openCreateDialog = () => {
  dialogMode.value = 'create'
  resetForm()
  showDialog.value = true
}

const editStudent = (student: any) => {
  dialogMode.value = 'edit'
  selectedStudent.value = student
  formData.value = {
    firstName: student.firstName,
    lastName: student.lastName,
    email: student.email || '',
    phoneNumber: student.phoneNumber || '',
    dateOfBirth: new Date(student.dateOfBirth),
    grade: student.grade,
    section: student.section || ''
  }
  showDialog.value = true
}

const viewStudent = (student: any) => {
  selectedStudent.value = student
  showViewDialog.value = true
}

const confirmDelete = (student: any) => {
  if (confirm(`¿Está seguro que desea eliminar a ${student.firstName} ${student.lastName}?`)) {
    deleteStudent(student.id)
  }
}

const deleteStudent = async (id: number) => {
  try {
    await $api(`/students/${id}`, { method: 'DELETE' })
    await fetchStudents()
  } catch (err: any) {
    error.value = err.data?.message || 'Error al eliminar estudiante'
  }
}

const resetForm = () => {
  formData.value = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: null,
    grade: '',
    section: ''
  }
  formErrors.value = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    grade: ''
  }
  selectedStudent.value = null
}

const calculateAge = (dateOfBirth: string): number => {
  const today = new Date()
  const birthDate = new Date(dateOfBirth)
  let age = today.getFullYear() - birthDate.getFullYear()
  const monthDiff = today.getMonth() - birthDate.getMonth()
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
    age--
  }
  return age
}

// Lifecycle
onMounted(() => {
  fetchStudents()
})
</script>