<script setup lang="ts">
import { ref, watch } from 'vue'
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
  currency: 'CAD'
})

const isSubmitting = ref(false)
const error = ref('')

watch(() => props.transaction, (newVal) => {
  if (newVal) {
    form.value = {
      amount: newVal.amount,
      type: newVal.type,
      date: new Date(newVal.date).toISOString().split('T')[0],
      merchant: newVal.merchant || '',
      note: newVal.note || '',
      currency: 'CAD'
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
      note: form.value.note
    }

    if (props.mode === 'add') {
      await axios.post('http://localhost:5194/api/transactions', payload)
    } else {
      await axios.put(`http://localhost:5194/api/transactions/${props.transaction.id}?budgetSpaceId=${props.budgetSpaceId}`, payload)
    }

    emit('saved')
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to save transaction'
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
