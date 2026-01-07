<script setup lang="ts">
import { useAuthStore } from '../../stores/auth'
import { useRouter } from 'vue-router'
import DashboardCard from '../../components/DashboardCard.vue'
import LineChart from '../../components/LineChart.vue'
import { ref, onMounted } from 'vue'
import axios from 'axios'

const auth = useAuthStore()
const router = useRouter()

const monthlyData = ref({ income: 0, expense: 0, net: 0 })
// Mock data for charts until real API aggregation is more robust
const chartLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun']
const burnRateData = [12000, 15000, 11000, 18000, 16000, 21500]
const cashData = [50000, 48000, 52000, 55000, 49000, 60000]

onMounted(async () => {
  // Fetch real summary for current month
  try {
    const spaceIdResp = await axios.get('http://localhost:5194/api/BudgetSpaces')
    if(spaceIdResp.data.length > 0) {
      const spaceId = spaceIdResp.data[0].id
      const now = new Date()
      const monthStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
      
      const summaryResp = await axios.get(`http://localhost:5194/api/Analytics/monthlySummary?budgetSpaceId=${spaceId}&month=${monthStr}`)
      monthlyData.value = summaryResp.data
    }
  } catch (e) {
    console.error("Failed to load dashboard data", e)
  }
})

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
        <button @click="logout" class="px-4 py-2 bg-finance-card border border-gray-700 text-finance-text rounded hover:bg-gray-800 transition-colors text-sm">
          Logout
        </button>
      </div>
    </div>
    
    <!-- Grid Layout -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
      <!-- KPI Cards -->
      <DashboardCard title="Current Cash Balance">
        <div class="flex items-baseline space-x-2">
          <span class="text-4xl font-bold text-finance-text">$64,200</span>
          <span class="text-finance-green text-sm font-medium">↑ 12%</span>
        </div>
        <div class="mt-4 h-1 bg-gray-800 rounded overflow-hidden">
          <div class="h-full bg-finance-cyan" style="width: 75%"></div>
        </div>
        <p class="text-finance-muted text-xs mt-2">Target: $85,000</p>
      </DashboardCard>

      <DashboardCard title="Monthly Burn Rate">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold text-finance-text">${{ monthlyData.expense.toLocaleString() }}</span>
           <span class="text-finance-red text-sm font-medium">↑ 5%</span>
        </div>
        <p class="text-finance-muted text-xs mt-4">vs. previous 30 days</p>
      </DashboardCard>

      <DashboardCard title="Net Burn">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold" :class="monthlyData.net >= 0 ? 'text-finance-green' : 'text-finance-red'">
             {{ monthlyData.net >= 0 ? '+' : '' }}${{ Math.abs(monthlyData.net).toLocaleString() }}
           </span>
        </div>
         <p class="text-finance-muted text-xs mt-4">Income - Expenses</p>
      </DashboardCard>

      <DashboardCard title="Runway">
        <div class="flex items-baseline space-x-2">
           <span class="text-4xl font-bold text-finance-text">14.2</span>
           <span class="text-finance-muted text-lg">months</span>
        </div>
        <p class="text-finance-muted text-xs mt-4">Based on current burn rate</p>
      </DashboardCard>
    </div>

    <!-- Charts Row -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
      <DashboardCard title="Cash Flow Forecast">
        <LineChart :labels="chartLabels" :data="cashData" color="#00D1FF" />
      </DashboardCard>
      
      <DashboardCard title="Expense Trend (Burn)">
        <LineChart :labels="chartLabels" :data="burnRateData" color="#F9575E" />
      </DashboardCard>
    </div>

    <!-- Tables Row -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <DashboardCard title="Recent Transactions" class="lg:col-span-2">
        <div class="overflow-x-auto">
          <table class="w-full text-sm text-left">
            <thead>
              <tr class="text-finance-muted border-b border-gray-800">
                <th class="pb-3 font-normal">Merchant</th>
                <th class="pb-3 font-normal">Date</th>
                <th class="pb-3 font-normal">Category</th>
                <th class="pb-3 font-normal text-right">Amount</th>
              </tr>
            </thead>
            <tbody class="text-finance-text">
              <tr class="border-b border-gray-800/50 last:border-0">
                <td class="py-3">AWS Web Services</td>
                <td class="py-3 text-finance-muted">Jan 05</td>
                <td class="py-3"><span class="px-2 py-1 bg-blue-900/30 text-blue-400 rounded text-xs">Infrastructure</span></td>
                <td class="py-3 text-right">-$1,204.50</td>
              </tr>
              <tr class="border-b border-gray-800/50 last:border-0">
                <td class="py-3">Stripe Payout</td>
                <td class="py-3 text-finance-muted">Jan 04</td>
                <td class="py-3"><span class="px-2 py-1 bg-green-900/30 text-green-400 rounded text-xs">Revenue</span></td>
                <td class="py-3 text-right text-finance-green">+$4,500.00</td>
              </tr>
              <tr class="border-b border-gray-800/50 last:border-0">
                <td class="py-3">WeWork Rent</td>
                <td class="py-3 text-finance-muted">Jan 01</td>
                <td class="py-3"><span class="px-2 py-1 bg-orange-900/30 text-orange-400 rounded text-xs">Office</span></td>
                <td class="py-3 text-right">-$3,500.00</td>
              </tr>
            </tbody>
          </table>
        </div>
      </DashboardCard>
      
      <DashboardCard title="Department Spend">
        <div class="space-y-4">
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Engineering</span>
              <span class="text-finance-text">$12,400</span>
            </div>
            <div class="h-2 bg-gray-800 rounded overflow-hidden">
               <div class="h-full bg-finance-cyan" style="width: 65%"></div>
            </div>
          </div>
          <div>
            <div class="flex justify-between text-sm mb-1">
              <span>Marketing</span>
              <span class="text-finance-text">$8,200</span>
            </div>
            <div class="h-2 bg-gray-800 rounded overflow-hidden">
               <div class="h-full bg-finance-yellow" style="width: 45%"></div>
            </div>
          </div>
          <div>
             <div class="flex justify-between text-sm mb-1">
              <span>Operations</span>
              <span class="text-finance-text">$4,100</span>
            </div>
            <div class="h-2 bg-gray-800 rounded overflow-hidden">
               <div class="h-full bg-finance-green" style="width: 25%"></div>
            </div>
          </div>
        </div>
      </DashboardCard>
    </div>
  </div>
</template>
