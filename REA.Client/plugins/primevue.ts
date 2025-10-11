import { defineNuxtPlugin } from '#app'
import PrimeVue from 'primevue/config'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import Calendar from 'primevue/calendar'
import Tag from 'primevue/tag'
import ProgressBar from 'primevue/progressbar'
import ProgressSpinner from 'primevue/progressspinner'
import Card from 'primevue/card'
import Message from 'primevue/message'

export default defineNuxtPlugin((nuxtApp) => {
    nuxtApp.vueApp.use(PrimeVue)

    // Register PrimeVue components
    nuxtApp.vueApp.component('Button', Button)
    nuxtApp.vueApp.component('InputText', InputText)
    nuxtApp.vueApp.component('DataTable', DataTable)
    nuxtApp.vueApp.component('Column', Column)
    nuxtApp.vueApp.component('Dialog', Dialog)
    nuxtApp.vueApp.component('Dropdown', Dropdown)
    nuxtApp.vueApp.component('Calendar', Calendar)
    nuxtApp.vueApp.component('Tag', Tag)
    nuxtApp.vueApp.component('ProgressBar', ProgressBar)
    nuxtApp.vueApp.component('ProgressSpinner', ProgressSpinner)
    nuxtApp.vueApp.component('Card', Card)
    nuxtApp.vueApp.component('Message', Message)
})