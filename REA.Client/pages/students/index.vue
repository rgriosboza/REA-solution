<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Header -->
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
      <Button
          label="Agregar Estudiante"
          icon="pi pi-plus"
          @click="openCreateDialog"
          :disabled="isLoading"
      />
    </div>

    <!-- Filters and Search -->
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
          <!-- ID Column -->
          <Column field="id" header="ID" :sortable="true" style="width: 80px">
            <template #body="{ data }">
              <span class="font-mono text-sm text-gray-600">#{{ data.id }}</span>
            </template>
          </Column>

          <!-- Name Column -->
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

          <!-- Grade Column -->
          <Column field="grade" header="Grado" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <Tag :value="data.grade" severity="info" />
            </template>
          </Column>

          <!-- Section Column -->
          <Column field="section" header="Sección" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <span v-if="data.section" class="text-gray-700">{{ data.section }}</span>
              <span v-else class="text-gray-400 italic">N/A</span>
            </template>
          </Column>

          <!-- Age Column -->
          <Column header="Edad" :sortable="true" sortField="dateOfBirth" style="width: 100px">
            <template #body="{ data }">
              {{ calculateAge(data.dateOfBirth) }} años
            </template>
          </Column>

          <!-- Status Column -->
          <Column field="isActive" header="Estado" :sortable="true" style="width: 120px">
            <template #body="{ data }">
              <Tag
                  :value="data.isActive ? 'Activo' : 'Inactivo'"
                  :severity="data.isActive ? 'success' : 'danger'"
              />
            </template>
          </Column>

          <!-- Actions Column -->
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

    <!-- Create/Edit Dialog -->
    <Dialog
        v-model:visible="showDialog"
        :header="dialogMode === 'create' ? 'Agregar Estudiante' : 'Editar Estudiante'"
        :modal="true"
        :closable="!isSubmitting"
        :closeOnEscape="!isSubmitting"
        style="width: 600px"
    >
      <form @submit.prevent="handleSubmit" class="space-y-4 pt-4">
        <!-- First Name -->
        <div>
          <label for="firstName" class="block text-sm font-medium text-gray-700 mb-2">
            Nombre *
          </label>
          <InputText
              id="firstName"
              v-model="formData.firstName"
              class="w-full"
              :class="{ 'p-invalid': formErrors.firstName }"
              :disabled="isSubmitting"
          />
          <small class="p-error" v-if="formErrors.firstName">
            {{ formErrors.firstName }}
          </small>
        </div>

        <!-- Last Name -->
        <div>
          <label for="lastName" class="block text-sm font-medium text-gray-700 mb-2">
            Apellido *
          </label>
          <InputText
              id="lastName"
              v-model="formData.lastName"
              class="w-full"
              :class="{ 'p-invalid': formErrors.lastName }"
              :disabled="isSubmitting"
          />
          <small class="p-error" v-if="formErrors.lastName">
            {{ formErrors.lastName }}
          </small>
        </div>

        <!-- Email -->
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
            Correo Electrónico
          </label>
          <InputText
              id="email"
              v-model="formData.email"
              type="email"
              class="w-full"
              :disabled="isSubmitting"
          />
        </div>

        <!-- Phone -->
        <div>
          <label for="phoneNumber" class="block text-sm font-medium text-gray-700 mb-2">
            Teléfono
          </label>
          <InputText
              id="phoneNumber"
              v-model="formData.phoneNumber"
              class="w-full"
              :disabled="isSubmitting"
          />
        </div>

        <!-- Date of Birth -->
        <div>
          <label for="dateOfBirth" class="block text-sm font-medium text-gray-700 mb-2">
            Fecha de Nacimiento *
          </label>
          <Calendar
              id="dateOfBirth"
              v-model="formData.dateOfBirth"
              class="w-full"
              :class="{ 'p-invalid': formErrors.dateOfBirth }"
              dateFormat="yy-mm-dd"
              :disabled="isSubmitting"
              :maxDate="new Date()"
          />
          <small class="p-error" v-if="formErrors.dateOfBirth">
            {{ formErrors.dateOfBirth }}
          </small>
        </div>

        <!-- Grade and Section -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label for="grade" class="block text-sm font-medium text-gray-700 mb-2">
              Grado *
            </label>
            <Dropdown
                id="grade"
                v-model="formData.grade"
                :options="gradeOptions"
                optionLabel="label"
                optionValue="value"
                class="w-full"
                :class="{ 'p-invalid': formErrors.grade }"
                placeholder="Seleccionar"
                :disabled="isSubmitting"
            />
            <small class="p-error" v-if="formErrors.grade">
              {{ formErrors.grade }}
            </small>
          </div>

          <div>
            <label for="section" class="block text-sm font-medium text-gray-700 mb-2">
              Sección
            </label>
            <InputText
                id="section"
                v-model="formData.section"
                class="w-full"
                placeholder="A, B, C..."
                :disabled="isSubmitting"
            />
          </div>
        </div>

        <!-- Form Actions -->
        <div class="flex justify-end space-x-3 pt-4 border-t">
          <Button
              type="button"
              label="Cancelar"
              severity="secondary"
              @click="closeDialog"
              :disabled="isSubmitting"
          />
          <Button
              type="submit"
              :label="dialogMode === 'create' ? 'Crear' : 'Actualizar'"
              :loading="isSubmitting"
          />
        </div>
      </form>
    </Dialog>

    <!-- View Dialog -->
    <Dialog
        v-model:visible="showViewDialog"
        header="Detalles del Estudiante"
        :modal="true"
        style="width: 500px"
    >
      <div v-if="selectedStudent" class="space-y-4">
        <!-- Avatar -->
        <div class="flex justify-center mb-6">
          <div class="h-24 w-24 bg-primary-100 rounded-full flex items-center justify-center">
            <span class="text-primary-700 font-bold text-3xl">
              {{ selectedStudent.firstName.charAt(0) }}{{ selectedStudent.lastName.charAt(0) }}
            </span>
          </div>
        </div>

        <!-- Info Grid -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-sm text-gray-500">Nombre Completo</label>
            <p class="font-medium">{{ selectedStudent.firstName }} {{ selectedStudent.lastName }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Edad</label>
            <p class="font-medium">{{ calculateAge(selectedStudent.dateOfBirth) }} años</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Correo</label>
            <p class="font-medium">{{ selectedStudent.email || 'N/A' }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Teléfono</label>
            <p class="font-medium">{{ selectedStudent.phoneNumber || 'N/A' }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Grado</label>
            <p class="font-medium">{{ selectedStudent.grade }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Sección</label>
            <p class="font-medium">{{ selectedStudent.section || 'N/A' }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Fecha de Inscripción</label>
            <p class="font-medium">{{ formatDate(selectedStudent.enrollmentDate) }}</p>
          </div>
          <div>
            <label class="text-sm text-gray-500">Estado</label>
            <Tag
                :value="selectedStudent.isActive ? 'Activo' : 'Inactivo'"
                :severity="selectedStudent.isActive ? 'success' : 'danger'"
            />
          </div>
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

definePageMeta({
  middleware: ['auth']
})

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

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(s =>
        s.firstName.toLowerCase().includes(query) ||
        s.lastName.toLowerCase().includes(query) ||
        s.email?.toLowerCase().includes(query)
    )
  }

  // Grade filter
  if (filterGrade.value) {
    filtered = filtered.filter(s => s.grade === filterGrade.value)
  }

  // Status filter
  if (filterStatus.value !== null) {
    filtered = filtered.filter(s => s.isActive === filterStatus.value)
  }

  return filtered
})

// Methods
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

const validateForm = (): boolean => {
  formErrors.value = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    grade: ''
  }

  let isValid = true

  if (!formData.value.firstName.trim()) {
    formErrors.value.firstName = 'El nombre es requerido'
    isValid = false
  }

  if (!formData.value.lastName.trim()) {
    formErrors.value.lastName = 'El apellido es requerido'
    isValid = false
  }

  if (!formData.value.dateOfBirth) {
    formErrors.value.dateOfBirth = 'La fecha de nacimiento es requerida'
    isValid = false
  }

  if (!formData.value.grade) {
    formErrors.value.grade = 'El grado es requerido'
    isValid = false
  }

  return isValid
}

const handleSubmit = async () => {
  if (!validateForm()) return

  isSubmitting.value = true

  try {
    const payload = {
      firstName: formData.value.firstName,
      lastName: formData.value.lastName,
      email: formData.value.email || undefined,
      phoneNumber: formData.value.phoneNumber || undefined,
      dateOfBirth: formData.value.dateOfBirth?.toISOString() || '',
      grade: formData.value.grade,
      section: formData.value.section || undefined
    }

    if (dialogMode.value === 'create') {
      await $api('/students', {
        method: 'POST',
        body: payload
      })
    } else {
      await $api(`/students/${selectedStudent.value.id}`, {
        method: 'PUT',
        body: payload
      })
    }

    await fetchStudents()
    closeDialog()
  } catch (err: any) {
    error.value = err.data?.message || 'Error al guardar estudiante'
  } finally {
    isSubmitting.value = false
  }
}

const closeDialog = () => {
  showDialog.value = false
  resetForm()
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

const formatDate = (date: string): string => {
  return new Date(date).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// Lifecycle
onMounted(() => {
  fetchStudents()
})
</script>