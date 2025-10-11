<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div>
        <label for="firstName" class="block text-sm font-medium text-gray-700">
          First Name *
        </label>
        <InputText
            id="firstName"
            v-model="form.firstName"
            class="w-full"
            :class="{ 'p-invalid': errors.firstName }"
        />
        <small class="p-error" v-if="errors.firstName">{{ errors.firstName }}</small>
      </div>

      <div>
        <label for="lastName" class="block text-sm font-medium text-gray-700">
          Last Name *
        </label>
        <InputText
            id="lastName"
            v-model="form.lastName"
            class="w-full"
            :class="{ 'p-invalid': errors.lastName }"
        />
        <small class="p-error" v-if="errors.lastName">{{ errors.lastName }}</small>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div>
        <label for="email" class="block text-sm font-medium text-gray-700">
          Email
        </label>
        <InputText
            id="email"
            v-model="form.email"
            type="email"
            class="w-full"
        />
      </div>

      <div>
        <label for="phoneNumber" class="block text-sm font-medium text-gray-700">
          Phone Number
        </label>
        <InputText
            id="phoneNumber"
            v-model="form.phoneNumber"
            class="w-full"
        />
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div>
        <label for="dateOfBirth" class="block text-sm font-medium text-gray-700">
          Date of Birth *
        </label>
        <Calendar
            id="dateOfBirth"
            v-model="form.dateOfBirth"
            class="w-full"
            :class="{ 'p-invalid': errors.dateOfBirth }"
            dateFormat="yy-mm-dd"
        />
        <small class="p-error" v-if="errors.dateOfBirth">{{ errors.dateOfBirth }}</small>
      </div>

      <div>
        <label for="grade" class="block text-sm font-medium text-gray-700">
          Grade *
        </label>
        <Dropdown
            id="grade"
            v-model="form.grade"
            :options="grades"
            optionLabel="label"
            optionValue="value"
            class="w-full"
            :class="{ 'p-invalid': errors.grade }"
            placeholder="Select Grade"
        />
        <small class="p-error" v-if="errors.grade">{{ errors.grade }}</small>
      </div>
    </div>

    <div>
      <label for="section" class="block text-sm font-medium text-gray-700">
        Section
      </label>
      <InputText
          id="section"
          v-model="form.section"
          class="w-full"
      />
    </div>

    <div class="flex justify-end space-x-2">
      <Button
          type="button"
          label="Cancel"
          severity="secondary"
          @click="$emit('cancel')"
      />
      <Button
          type="submit"
          :label="submitLabel"
          :loading="isLoading"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, computed } from 'vue'

interface Props {
  student?: Student
  isLoading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<{
  submit: [data: CreateStudentRequest]
  cancel: []
}>()

const grades = [
  { label: '1st Grade', value: '1st' },
  { label: '2nd Grade', value: '2nd' },
  { label: '3rd Grade', value: '3rd' },
  { label: '4th Grade', value: '4th' },
  { label: '5th Grade', value: '5th' },
  { label: '6th Grade', value: '6th' }
]

const form = reactive({
  firstName: props.student?.firstName || '',
  lastName: props.student?.lastName || '',
  email: props.student?.email || '',
  phoneNumber: props.student?.phoneNumber || '',
  dateOfBirth: props.student?.dateOfBirth || '',
  grade: props.student?.grade || '',
  section: props.student?.section || ''
})

const errors = reactive({
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  grade: ''
})

const submitLabel = computed(() =>
    props.student ? 'Update Student' : 'Create Student'
)

const validateForm = (): boolean => {
  let isValid = true

  // Reset errors
  Object.keys(errors).forEach(key => {
    errors[key as keyof typeof errors] = ''
  })

  if (!form.firstName.trim()) {
    errors.firstName = 'First name is required'
    isValid = false
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'Last name is required'
    isValid = false
  }

  if (!form.dateOfBirth) {
    errors.dateOfBirth = 'Date of birth is required'
    isValid = false
  }

  if (!form.grade) {
    errors.grade = 'Grade is required'
    isValid = false
  }

  return isValid
}

const handleSubmit = () => {
  if (!validateForm()) return

  const submitData: CreateStudentRequest = {
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email || undefined,
    phoneNumber: form.phoneNumber || undefined,
    dateOfBirth: form.dateOfBirth,
    grade: form.grade,
    section: form.section || undefined
  }

  emit('submit', submitData)
}
</script>