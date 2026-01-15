<script setup lang="ts">
import { useAuthStore } from '../../stores/auth'
import { useRouter } from 'vue-router'
import DashboardCard from '../../components/DashboardCard.vue'
import LineChart from '../../components/LineChart.vue'
import PdfImportDialog from '../../components/PdfImportDialog.vue'
import TransactionFormDialog from '../../components/TransactionFormDialog.vue'
import { ref, onMounted, watch, computed } from 'vue'
import axios from 'axios'

const auth = useAuthStore()
const router = useRouter()
const showImportDialog = ref(false)
const showTransactionDialog = ref(false)
const transactionDialogMode = ref<'add' | 'edit'>('add')
const editingTransaction = ref<any>(null)
const currentBudgetSpaceId = ref<string>('')

const monthlyData = ref({ income: 0, expense: 0, net: 0 })
const recentTransactions = ref<any[]>([])
const trendLabels = ref<string[]>([])
const incomeTrend = ref<number[]>([])
const expenseTrend = ref<number[]>([])
const isLoading = ref(true)

const timeRanges = ['1D', '1W', '1M', '3M', '6M', 'Max']
const selectedTimeRange = ref('6M')
const isChartsMerged = ref(false)

const netIncomeTrend = computed(() => {
  return incomeTrend.value.map((inc, i) => {
    const exp = expenseTrend.value[i] || 0
    return inc - exp
  })
})

watch(selectedTimeRange, async () => {
  if (currentBudgetSpaceId.value) {
    await fetchTrends()
  }
})

async function fetchTrends() {
   try {
      const trendsResp = await axios.get(`http://localhost:5194/api/Analytics/trends?budgetSpaceId=${currentBudgetSpaceId.value}&range=${selectedTimeRange.value}`)
      if (trendsResp.data.points) {
        trendLabels.value = trendsResp.data.points.map((p: any) => p.month)
        incomeTrend.value = trendsResp.data.points.map((p: any) => p.income)
        expenseTrend.value = trendsResp.data.points.map((p: any) => p.expense)
      }
   } catch (e) {
     console.error("Failed to fetch trends", e)
   }
}

onMounted(async () => {
  isLoading.value = true
  
  try {
    const spaceIdResp = await axios.get('http://localhost:5194/api/BudgetSpaces')
    if(spaceIdResp.data.length > 0) {
      const spaceId = spaceIdResp.data[0].id
      currentBudgetSpaceId.value = spaceId
      
      const now = new Date()
      const monthStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
      
      // Parallel fetch
      const [summaryResp, transResp] = await Promise.all([
        axios.get(`http://localhost:5194/api/Analytics/monthlySummary?budgetSpaceId=${spaceId}&month=${monthStr}`),
        axios.get(`http://localhost:5194/api/transactions?budgetSpaceId=${spaceId}&pageSize=5`)
      ])

      // Fix: Map backend DTO (IncomeTotal) to frontend state (income)
      monthlyData.value = {
        income: summaryResp.data.incomeTotal,
        expense: summaryResp.data.expenseTotal,
        net: summaryResp.data.net
      }
      recentTransactions.value = transResp.data.items || []
      
      // Fetch trends separately to use the shared function
      await fetchTrends()
    }
  } catch (e) {
    console.error("Failed to load dashboard data", e)
  } finally {
    isLoading.value = false
  }
})

async function refreshDashboard() {
  isLoading.value = true
  try {
    const now = new Date()
    const monthStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
    
      // Parallel requests
      const [summaryResp, transResp] = await Promise.all([
        axios.get(`http://localhost:5194/api/Analytics/monthlySummary?budgetSpaceId=${currentBudgetSpaceId.value}&month=${monthStr}`),
        axios.get(`http://localhost:5194/api/transactions?budgetSpaceId=${currentBudgetSpaceId.value}&pageSize=5`)
      ])

      // Fix: Map backend DTO (IncomeTotal) to frontend state (income)
      monthlyData.value = {
        income: summaryResp.data.incomeTotal,
        expense: summaryResp.data.expenseTotal,
        net: summaryResp.data.net
      }
      recentTransactions.value = transResp.data.items || []
    
    await fetchTrends()
  } finally {
    isLoading.value = false
  }
}

function handleAddTransaction() {
  transactionDialogMode.value = 'add'
  editingTransaction.value = null
  showTransactionDialog.value = true
}

function handleEditTransaction(transaction: any) {
  transactionDialogMode.value = 'edit'
  editingTransaction.value = transaction
  showTransactionDialog.value = true
}

async function handleDeleteTransaction(transactionId: string) {
  if (!confirm('Are you sure you want to delete this transaction? This cannot be undone.')) {
    return
  }

  try {
    await axios.delete(`http://localhost:5194/api/transactions/${transactionId}?budgetSpaceId=${currentBudgetSpaceId.value}`)
    await refreshDashboard()
  } catch (err) {
    alert('Failed to delete transaction')
  }
}

function onTransactionSaved() {
  showTransactionDialog.value = false
  refreshDashboard()
}

function onImportComplete(count: number) {
  alert(`Successfully imported ${count} transactions!`)
  showImportDialog.value = false
  // Optionally reload dashboard data here
}

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="p-8">
    <!-- Header -->
    <div class="flex justify-between items-end mb-8">
      <div>
        <h1 class="text-3xl font-bold text-finance-text tracking-tight">CFO Dashboard</h1>
        <p class="text-finance-muted mt-1">Financial overview and KPI tracking</p>
      </div>
      <div class="flex items-center space-x-4">
        <span class="text-finance-text text-sm font-medium">Hello, {{ auth.user?.displayName || 'User' }}</span>
        <button 
          @click="handleAddTransaction" 
          class="px-5 py-2.5 bg-finance-cyan hover:bg-white text-black font-bold rounded-xl transition-all duration-200 text-sm flex items-center space-x-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          <span>Add Transaction</span>
        </button>
        <button 
          @click="showImportDialog = true" 
          class="px-5 py-2.5 bg-finance-green hover:bg-white text-black font-bold rounded-xl transition-all duration-200 text-sm flex items-center space-x-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
          <span>Import Statement</span>
        </button>
        <button @click="router.push('/categories')" class="px-4 py-2 bg-finance-card border border-gray-700 text-finance-text rounded hover:bg-gray-800 transition-colors text-sm">
          Categories
        </button>
        <button @click="logout" class="px-4 py-2 bg-finance-card border border-gray-700 text-finance-text rounded hover:bg-gray-800 transition-colors text-sm">
          Logout
        </button>
      </div>
    </div>
    
    <!-- Grid Layout -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
      <!-- KPI Cards -->
      <DashboardCard title="Monthly Income">
        <div class="flex items-baseline space-x-2">
          <span class="text-4xl font-bold text-finance-green">${{ monthlyData.income.toLocaleString() }}</span>
        </div>
        <p class="text-finance-muted text-xs mt-4">Total received this month</p>
      </DashboardCard>

      <DashboardCard title="Monthly Expenses">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold text-finance-red">${{ monthlyData.expense.toLocaleString() }}</span>
        </div>
        <p class="text-finance-muted text-xs mt-4">Total spent this month</p>
      </DashboardCard>

      <DashboardCard title="Net Burn">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold" :class="monthlyData.net >= 0 ? 'text-finance-green' : 'text-finance-red'">
             {{ monthlyData.net >= 0 ? '+' : '' }}${{ Math.abs(monthlyData.net).toLocaleString() }}
           </span>
        </div>
         <p class="text-finance-muted text-xs mt-4">Income - Expenses</p>
      </DashboardCard>

      <DashboardCard title="Total Transactions">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold text-finance-cyan">{{ recentTransactions.length }}</span>
        </div>
        <p class="text-finance-muted text-xs mt-4">Tracked this period</p>
      </DashboardCard>
    </div>

    <!-- Charts -->
    <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-white">Financial Trends</h2>
        <div class="bg-finance-card rounded-lg p-1 flex space-x-1 border border-white/5">
          <button 
            v-for="range in timeRanges" 
            :key="range"
            @click="selectedTimeRange = range"
            class="px-3 py-1 text-xs font-bold rounded-md transition-all"
            :class="selectedTimeRange === range ? 'bg-finance-cyan text-black shadow-lg' : 'text-finance-muted hover:text-white hover:bg-white/5'"
          >
            {{ range }}
          </button>
        </div>
    </div>
    <div class="relative mb-6">
      
      <!-- Unified Interactions Button -->
      <button 
        @click="isChartsMerged = !isChartsMerged"
        class="absolute left-1/2 z-20 transition-all duration-500 ease-in-out
               bg-finance-card border border-white/10 rounded-full p-2 
               hover:bg-finance-cyan hover:text-black hover:scale-110 shadow-xl group"
        :class="isChartsMerged ? 'top-0 -translate-y-1/2' : 'top-1/2 -translate-y-1/2 -translate-x-1/2'"
        :title="isChartsMerged ? 'Split View' : 'Merge View'"
      >
        <svg v-if="!isChartsMerged" class="w-5 h-5 text-finance-muted group-hover:text-black transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
        </svg>
        <svg v-else class="w-5 h-5 text-finance-muted group-hover:text-black transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
           <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1v4m0-4h-4m4 4l-5-5" />
        </svg>
      </button>

      <!-- Charts Grid -->
      <div 
        class="grid gap-6 transition-all duration-500 ease-in-out"
        :class="isChartsMerged ? 'grid-cols-1 pt-6' : 'grid-cols-1 lg:grid-cols-2'"
      >
        <!-- Cash Flow (Hidden in Merged) -->
        <DashboardCard v-if="!isChartsMerged" title="Cash Flow">
          <div v-if="isLoading" class="h-64 flex items-center justify-center">
             <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-finance-cyan"></div>
          </div>
          <LineChart 
            v-else
            :data="incomeTrend"
            :labels="trendLabels" 
            color="#10B981"
          />
        </DashboardCard>
        
        <!-- Expenses (Hidden in Merged) -->
        <DashboardCard v-if="!isChartsMerged" title="Expenses">
          <div v-if="isLoading" class="h-64 flex items-center justify-center">
             <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-finance-cyan"></div>
          </div>
          <LineChart 
            v-else
            :data="expenseTrend"
            :labels="trendLabels" 
            color="#EF4444"
          />
        </DashboardCard>

        <!-- Net Income (Visible Only in Merged) -->
        <DashboardCard v-if="isChartsMerged" title="Net Income">
           <div v-if="isLoading" class="h-64 flex items-center justify-center">
             <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-finance-cyan"></div>
           </div>
           <LineChart 
             v-else
             :data="netIncomeTrend"
             :labels="trendLabels" 
             color="#3B82F6"
           />
        </DashboardCard>
      </div>
    </div>

    <!-- Recent Transactions (Full Width) -->
    <div class="mb-6">
      <DashboardCard title="Recent Transactions">
        <template #extra>
          <router-link to="/transactions" class="text-xs text-finance-cyan hover:underline font-bold">View All →</router-link>
        </template>
        <div v-if="isLoading" class="text-center text-finance-muted py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-finance-cyan mx-auto"></div>
          <p class="mt-4">Loading transactions...</p>
        </div>
        <div v-else-if="recentTransactions.length === 0" class="text-center text-finance-muted py-12">
          <svg class="w-16 h-16 mx-auto mb-4 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-lg font-semibold">No transactions yet</p>
          <p class="text-sm mt-2">Import your bank statement or add transactions manually</p>
          <button 
            @click="showImportDialog = true"
            class="mt-6 px-6 py-3 bg-finance-green hover:bg-white text-black font-bold rounded-xl transition-all inline-block"
          >
            Import Your First Statement
          </button>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm text-left">
            <thead>
              <tr class="text-finance-muted border-b border-gray-800">
                <th class="pb-3 font-normal">Merchant</th>
                <th class="pb-3 font-normal">Date</th>
                <th class="pb-3 font-normal">Category</th>
                <th class="pb-3 font-normal text-right">Amount</th>
                <th class="pb-3 font-normal text-center">Actions</th>
              </tr>
            </thead>
            <tbody class="text-finance-text">
              <tr v-for="tx in recentTransactions" :key="tx.id" class="border-b border-gray-800/50 last:border-0">
                <td class="py-3">{{ tx.merchant || 'Unknown' }}</td>
                <td class="py-3 text-finance-muted">{{ new Date(tx.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) }}</td>
                <td class="py-3">
                  <span v-if="tx.categoryName" class="px-2 py-1 bg-blue-900/30 text-blue-400 rounded text-xs">
                    {{ tx.categoryName }}
                  </span>
                  <span v-else class="text-finance-muted text-xs">Uncategorized</span>
                </td>
                <td class="py-3 text-right" :class="tx.type === 'Income' ? 'text-finance-green' : 'text-finance-text'">
                  {{ tx.type === 'Income' ? '+' : '-' }}${{ tx.amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
                </td>
                <td class="py-3">
                  <div class="flex items-center justify-center space-x-2">
                    <button 
                      @click="handleEditTransaction(tx)"
                      class="p-1.5 text-finance-cyan hover:text-white hover:bg-finance-cyan/10 rounded transition-colors"
                      title="Edit transaction"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                    </button>
                    <button 
                      @click="handleDeleteTransaction(tx.id)"
                      class="p-1.5 text-finance-red hover:text-white hover:bg-finance-red/10 rounded transition-colors"
                      title="Delete transaction"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </DashboardCard>
    </div>



    <!-- PDF Import Dialog -->
    <PdfImportDialog 
      v-if="showImportDialog"
      :budget-space-id="currentBudgetSpaceId"
      @imported="onImportComplete"
      @close="showImportDialog = false"
    />

    <!-- Transaction Form Dialog -->
    <TransactionFormDialog
      v-if="showTransactionDialog"
      :budget-space-id="currentBudgetSpaceId"
      :mode="transactionDialogMode"
      :transaction="editingTransaction"
      @saved="onTransactionSaved"
      @close="showTransactionDialog = false"
    />
  </div>
</template>
