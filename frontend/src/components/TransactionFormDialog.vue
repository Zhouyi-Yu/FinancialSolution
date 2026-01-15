<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import axios from 'axios'

const props = defineProps<{
  budgetSpaceId: string
  mode: 'add' | 'edit'
  transaction?: any
}>()

const emit = defineEmits<{
  (e: 'saved'): void
  (e: 'close'): void
}>()

const form = ref({
  amount: props.transaction?.amount || 0,
  type: props.transaction?.type || 'Expense',
  date: props.transaction?.date ? new Date(props.transaction.date).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
  merchant: props.transaction?.merchant || '',
  note: props.transaction?.note || '',
  currency: 'CAD',
  categoryId: props.transaction?.categoryId || '',
  isTemplate: false // For "Save as Template" checkbox
})

const templates = ref<any[]>([])
const categories = ref<any[]>([])
const selectedTemplateId = ref('')

const isSubmitting = ref(false)
const error = ref('')

onMounted(async () => {
    if (props.budgetSpaceId) {
        try {
            const [tmplRes, catRes] = await Promise.all([
                axios.get(`http://localhost:5194/api/TransactionTemplates?budgetSpaceId=${props.budgetSpaceId}`),
                axios.get(`http://localhost:5194/api/Categories?budgetSpaceId=${props.budgetSpaceId}`)
            ])
            templates.value = tmplRes.data
            categories.value = catRes.data
        } catch (e) {
            console.error("Failed to load dialog data", e)
        }
    }
})

function applyTemplate() {
    const tmpl = templates.value.find(t => t.id === selectedTemplateId.value)
    if (tmpl) {
        // Auto-fill logic (Procore style)
        form.value.type = tmpl.type === 0 ? 'Income' : 'Expense' // Enum handling
        form.value.merchant = tmpl.merchant || ''
        form.value.note = tmpl.note || ''
        if (tmpl.amount) form.value.amount = tmpl.amount
        if (tmpl.categoryId) form.value.categoryId = tmpl.categoryId
        
        // Visual feedback could be added here
    }
}

watch(() => props.transaction, (newVal) => {
  if (newVal) {
    form.value = {
      amount: newVal.amount,
      type: newVal.type,
      date: new Date(newVal.date).toISOString().split('T')[0],
      merchant: newVal.merchant || '',
      note: newVal.note || '',
      currency: 'CAD',
      categoryId: newVal.categoryId || '',
      isTemplate: false
    }
  }
}, { immediate: true })

async function handleSubmit() {
  if (!props.budgetSpaceId) {
    error.value = 'System error: No budget space found. Please refresh the page.'
    return
  }

  isSubmitting.value = true
  error.value = ''

  try {
    const payload = {
      budgetSpaceId: props.budgetSpaceId,
      type: form.value.type === 'Income' ? 0 : 1,
      amount: parseFloat(form.value.amount.toString()),
      currency: form.value.currency,
      // Fix: Send Noon UTC to prevent timezone shifts (e.g. Jan 10 -> Jan 9 17:00)
      date: new Date(`${form.value.date}T12:00:00Z`).toISOString(),
      merchant: form.value.merchant,
      note: form.value.note,
      categoryId: form.value.categoryId || null
    }

    if (props.mode === 'add') {
      await axios.post('http://localhost:5194/api/transactions', payload)
      
      // Save as Template Logic (New Feature) - Non-blocking
      if (form.value.isTemplate) {
          try {
              await axios.post('http://localhost:5194/api/TransactionTemplates', {
                  budgetSpaceId: props.budgetSpaceId,
                  templateName: payload.merchant, 
                  type: payload.type,
                  amount: payload.amount,
                  merchant: payload.merchant,
                  note: payload.note,
                  categoryId: payload.categoryId
              })
          } catch (tmplErr) {
              console.warn("Failed to save template, but transaction was saved.", tmplErr)
              // We don't throw here so the transaction emission still happens
          }
      }
    } else {
      await axios.put(`http://localhost:5194/api/transactions/${props.transaction.id}?budgetSpaceId=${props.budgetSpaceId}`, payload)
    }

    emit('saved')
  } catch (err: any) {
    console.error("Save error detail:", err.response?.data || err.message)
    error.value = err.response?.data?.message || err.response?.data || err.message || 'Failed to save transaction'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
    <div class="bg-finance-card border border-white/10 rounded-3xl p-8 max-w-2xl w-full">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-white">{{ mode === 'add' ? 'Add Transaction' : 'Edit Transaction' }}</h2>
        <button @click="emit('close')" class="text-finance-muted hover:text-white transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Template Loader (Procore "Cost Catalog" Style) -->
      <div v-if="mode === 'add'" class="mb-6 bg-finance-bg border border-white/5 p-4 rounded-xl">
          <div v-if="templates.length > 0">
              <label class="block text-xs font-bold text-finance-cyan uppercase tracking-wider mb-2">
                  Load from Catalog (Templates)
              </label>
              <div class="flex space-x-2">
                <select 
                    v-model="selectedTemplateId"
                    class="flex-1 bg-finance-card border border-white/10 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-finance-cyan"
                >
                    <option value="">Select a recurring bill...</option>
                    <option v-for="t in templates" :key="t.id" :value="t.id">
                        {{ t.templateName }} ({{ t.amount ? '$' + t.amount : 'Variable' }})
                    </option>
                </select>
                <button 
                    @click="applyTemplate"
                    :disabled="!selectedTemplateId"
                    class="px-4 py-2 bg-white/5 hover:bg-finance-cyan hover:text-black text-white text-xs font-bold rounded-lg transition-colors disabled:opacity-50"
                >
                    Apply
                </button>
              </div>
          </div>
          <div v-else class="text-xs text-finance-muted flex items-center space-x-2">
              <svg class="w-4 h-4 text-finance-cyan" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span>Your Recurring Catalog is empty. Add a transaction below and check "Save to Catalog" to create templates.</span>
          </div>
      </div>

      <form @submit.prevent="handleSubmit" class="space-y-6">
        <!-- Type Selection -->
        <div>
          <label class="block text-sm font-semibold text-finance-muted mb-3">Type</label>
          <div class="flex space-x-4">
            <label class="flex items-center space-x-2 cursor-pointer">
              <input type="radio" v-model="form.type" value="Expense" class="w-4 h-4 text-finance-red">
              <span class="text-white">Expense</span>
            </label>
            <label class="flex items-center space-x-2 cursor-pointer">
              <input type="radio" v-model="form.type" value="Income" class="w-4 h-4 text-finance-green">
              <span class="text-white">Income</span>
            </label>
          </div>
        </div>

        <!-- Amount and Date Row -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label for="amount" class="block text-sm font-semibold text-finance-muted mb-2">Amount</label>
            <div class="relative">
              <span class="absolute left-4 top-1/2 -translate-y-1/2 text-finance-muted">$</span>
              <input 
                id="amount"
                v-model="form.amount"
                type="number"
                step="0.01"
                min="0.01"
                required
                class="w-full bg-finance-bg border border-white/10 rounded-xl pl-8 pr-4 py-3 text-white focus:outline-none focus:ring-2 focus:ring-finance-cyan/40"
              >
            </div>
          </div>

          <div>
            <label for="date" class="block text-sm font-semibold text-finance-muted mb-2">Date</label>
            <input 
              id="date"
              v-model="form.date"
              type="date"
              required
              class="w-full bg-finance-bg border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:ring-2 focus:ring-finance-cyan/40"
            >
          </div>
        </div>

        <!-- Category Dropdown -->
        <div>
          <label for="category" class="block text-sm font-semibold text-finance-muted mb-2">Category</label>
          <select 
            id="category"
            v-model="form.categoryId"
            class="w-full bg-finance-bg border border-white/10 rounded-xl px-4 py-3 text-white focus:outline-none focus:ring-2 focus:ring-finance-cyan/40"
          >
            <option value="">No Category</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">
              {{ cat.name }}
            </option>
          </select>
        </div>

        <!-- Merchant -->
        <div>
          <label for="merchant" class="block text-sm font-semibold text-finance-muted mb-2">Merchant / Description</label>
          <input 
            id="merchant"
            v-model="form.merchant"
            type="text"
            required
            placeholder="e.g., Starbucks, Salary Deposit"
            class="w-full bg-finance-bg border border-white/10 rounded-xl px-4 py-3 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-cyan/40"
          >
        </div>

        <!-- Notes -->
        <div>
          <label for="note" class="block text-sm font-semibold text-finance-muted mb-2">Notes (Optional)</label>
          <textarea 
            id="note"
            v-model="form.note"
            rows="3"
            placeholder="Add any additional details..."
            class="w-full bg-finance-bg border border-white/10 rounded-xl px-4 py-3 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-cyan/40 resize-none"
          ></textarea>
        </div>

        <!-- Save as Template Option -->
        <div v-if="mode === 'add'" class="flex items-center space-x-2 pt-2">
            <input 
                type="checkbox" 
                id="saveTemplate" 
                v-model="form.isTemplate"
                class="w-4 h-4 rounded border-gray-600 bg-finance-bg text-finance-cyan focus:ring-finance-cyan"
            >
            <label for="saveTemplate" class="text-sm text-finance-muted cursor-pointer hover:text-white transition-colors">
                Save to "Recurring Catalog" for future use
            </label>
        </div>

        <!-- Error Message -->
        <div v-if="error" class="bg-finance-red/10 border border-finance-red/20 text-finance-red px-4 py-3 rounded-xl text-sm">
          {{ error }}
        </div>

        <!-- Action Buttons -->
        <div class="flex space-x-4 pt-4">
          <button 
            type="button"
            @click="emit('close')"
            class="flex-1 border-2 border-white/10 text-white font-bold py-3 rounded-xl hover:bg-white/5 transition-all"
          >
            Cancel
          </button>
          <button 
            type="submit"
            :disabled="isSubmitting"
            class="flex-1 bg-finance-cyan hover:bg-white disabled:opacity-50 text-black font-bold py-3 rounded-xl transition-all"
          >
            {{ isSubmitting ? 'Saving...' : mode === 'add' ? 'Add Transaction' : 'Save Changes' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
