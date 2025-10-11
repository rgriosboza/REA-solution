<template>
  <nav class="bg-white shadow-sm border-b">
    <div class="container mx-auto px-6">
      <div class="flex justify-between items-center h-16">
        <div class="flex items-center space-x-8">
          <NuxtLink to="/" class="text-xl font-bold text-primary-600">
            REA System
          </NuxtLink>

          <div class="hidden md:flex space-x-4">
            <NuxtLink
                to="/students"
                class="px-3 py-2 rounded-md text-sm font-medium transition-colors"
                :class="[$route.path === '/students' ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100']"
            >
              Students
            </NuxtLink>
            <NuxtLink
                to="/ocr"
                class="px-3 py-2 rounded-md text-sm font-medium transition-colors"
                :class="[$route.path === '/ocr' ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100']"
            >
              OCR Processing
            </NuxtLink>
          </div>
        </div>

        <div class="flex items-center space-x-4">
          <span class="text-sm text-gray-700">Welcome, User</span>
          <Button label="Logout" severity="secondary" size="small" />
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
</script>