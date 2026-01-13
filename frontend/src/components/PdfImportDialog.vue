<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'

const props = defineProps<{
  budgetSpaceId: string
}>()

const emit = defineEmits<{
  (e: 'imported', count: number): void
  (e: 'close'): void
}>()

const isUploading = ref(false)
const showPreview = ref(false)
const previewData = ref<any>(null)
const selectedFile = ref<File | null>(null)
const error = ref('')
const importDuplicates = ref(false)

function handleFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
    error.value = ''
  }
}

function handleDrop(event: DragEvent) {
  event.preventDefault()
  if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
    const file = event.dataTransfer.files[0]
    if (file.type === 'application/pdf') {
      selectedFile.value = file
      error.value = ''
    } else {
      error.value = 'Please upload a PDF file'
    }
  }
}

function handleDragOver(event: DragEvent) {
  event.preventDefault()
}

async function uploadAndPreview() {
  if (!selectedFile.value) {
    error.value = 'Please select a file'
    return
  }

  isUploading.value = true
  error.value = ''

  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    formData.append('budgetSpaceId', props.budgetSpaceId)

    const response = await axios.post('/api/transactions/import/preview', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    previewData.value = response.data
    showPreview.value = true
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to parse PDF. Please ensure it\'s a valid bank statement.'
  } finally {
    isUploading.value = false
  }
}

async function confirmImport() {
  if (!previewData.value) return

  isUploading.value = true
  error.value = ''

  try {
    const response = await axios.post('/api/transactions/import/confirm', {
      budgetSpaceId: props.budgetSpaceId,
      importDuplicates: importDuplicates.value,
      transactions: previewData.value.transactions
    })

    emit('imported', response.data.importedCount)
    resetForm()
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to import transactions'
  } finally {
    isUploading.value = false
  }
}

function resetForm() {
  selectedFile.value = null
  showPreview.value = false
  previewData.value = null
  error.value = ''
  importDuplicates.value = false
}
</script>

<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
    <div class="bg-finance-card border border-white/10 rounded-3xl p-8 max-w-4xl w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-white">Import Bank Statement</h2>
        <button @click="emit('close')" class="text-finance-muted hover:text-white transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Upload Section -->
      <div v-if="!showPreview">
        <div 
          @drop="handleDrop"
          @dragover="handleDragOver"
          class="border-2 border-dashed border-white/20 rounded-2xl p-12 text-center hover:border-finance-cyan/50 transition-all duration-200"
        >
          <div class="mb-4">
            <svg class="w-16 h-16 mx-auto text-finance-muted" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
          </div>
          <p class="text-white font-semibold mb-2">Drag & drop your bank statement (PDF)</p>
          <p class="text-finance-muted text-sm mb-4">or</p>
          <label class="cursor-pointer inline-block bg-finance-cyan text-black font-bold py-3 px-6 rounded-xl hover:bg-white transition-colors">
            Choose File
            <input type="file" accept=".pdf" @change="handleFileSelect" class="hidden">
          </label>
          <p v-if="selectedFile" class="mt-4 text-finance-green">Selected: {{ selectedFile.name }}</p>
        </div>

        <div v-if="error" class="mt-4 bg-finance-red/10 border border-finance-red/20 text-finance-red px-4 py-3 rounded-2xl text-sm">
          {{ error }}
        </div>

        <button 
          @click="uploadAndPreview"
          :disabled="!selectedFile || isUploading"
          class="w-full mt-6 bg-finance-green hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed text-black font-bold py-4 rounded-2xl transition-all"
        >
          <span v-if="isUploading">Parsing PDF...</span>
          <span v-else>Preview Transactions</span>
        </button>
      </div>

      <!-- Preview Section -->
      <div v-else class="space-y-6">
        <div class="bg-finance-bg/50 rounded-2xl p-6 border border-white/5">
          <div class="grid grid-cols-3 gap-4 text-center">
            <div>
              <p class="text-finance-muted text-sm mb-1">Bank Detected</p>
              <p class="text-white font-bold text-lg">{{ previewData.bankDetected }}</p>
            </div>
            <div>
              <p class="text-finance-muted text-sm mb-1">Total Transactions</p>
              <p class="text-finance-cyan font-bold text-lg">{{ previewData.totalTransactions }}</p>
            </div>
            <div>
              <p class="text-finance-muted text-sm mb-1">Duplicates Found</p>
              <p class="text-finance-yellow font-bold text-lg">{{ previewData.duplicatesDetected }}</p>
            </div>
          </div>
        </div>

        <div class="bg-finance-bg/30 rounded-2xl p-4 max-h-96 overflow-y-auto">
          <table class="w-full">
            <thead class="text-finance-muted text-xs uppercase sticky top-0 bg-finance-bg">
              <tr>
                <th class="text-left p-2">Date</th>
                <th class="text-left p-2">Description</th>
                <th class="text-right p-2">Amount</th>
                <th class="text-center p-2">Status</th>
              </tr>
            </thead>
            <tbody class="text-white text-sm">
              <tr v-for="(tx, idx) in previewData.transactions" :key="idx" 
                  :class="{ 'opacity-50': tx.isDuplicate && !importDuplicates }">
                <td class="p-2">{{ new Date(tx.date).toLocaleDateString() }}</td>
                <td class="p-2">{{ tx.description }}</td>
                <td class="p-2 text-right" :class="tx.type === 'Income' ? 'text-finance-green' : 'text-finance-red'">
                  {{ tx.type === 'Income' ? '+' : '-' }}${{ tx.amount.toFixed(2) }}
                </td>
                <td class="p-2 text-center">
                  <span v-if="tx.isDuplicate" class="text-xs bg-finance-yellow/20 text-finance-yellow px-2 py-1 rounded-full">
                    Duplicate
                  </span>
                  <span v-else class="text finance-green">✓</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="previewData.duplicatesDetected > 0" class="flex items-center space-x-3">
          <input 
            type="checkbox" 
            v-model="importDuplicates" 
            id="import-dupes"
            class="w-5 h-5 rounded border-white/20 bg-finance-bg text-finance-cyan focus:ring-finance-cyan"
          >
          <label for="import-dupes" class="text-finance-muted text-sm cursor-pointer select-none">
            Import duplicate transactions anyway
          </label>
        </div>

        <div class="flex space-x-4">
          <button 
            @click="resetForm"
            class="flex-1 border-2 border-white/10 text-white font-bold py-4 rounded-2xl hover:bg-white/5 transition-all"
          >
            Cancel
          </button>
          <button 
            @click="confirmImport"
            :disabled="isUploading"
            class="flex-1 bg-finance-green hover:bg-white disabled:opacity-50 text-black font-bold py-4 rounded-2xl transition-all"
          >
            <span v-if="isUploading">Importing...</span>
            <span v-else>Confirm Import ({{ previewData.totalTransactions - (importDuplicates ? 0 : previewData.duplicatesDetected) }})</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
