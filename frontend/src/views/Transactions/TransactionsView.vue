<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import axios from 'axios'
import TransactionFormDialog from '../../components/TransactionFormDialog.vue'

const transactions = ref<any[]>([])
const categories = ref<any[]>([])
const loading = ref(true)
const spaceId = ref('')

const filters = ref({
    page: 1,
    pageSize: 20,
    type: '',
    categoryId: '',
    q: ''
})
const totalCount = ref(0)
const showEditDialog = ref(false)
const selectedTransaction = ref<any>(null)

async function fetchData() {
    loading.value = true
    try {
        const [transResp, catResp] = await Promise.all([
            axios.get(`http://localhost:5194/api/transactions`, {
                params: {
                    budgetSpaceId: spaceId.value,
                    page: filters.value.page,
                    pageSize: filters.value.pageSize,
                    type: filters.value.type || undefined,
                    categoryId: filters.value.categoryId || undefined,
                    q: filters.value.q || undefined
                }
            }),
            axios.get(`http://localhost:5194/api/Categories?budgetSpaceId=${spaceId.value}`)
        ])
        transactions.value = transResp.data.items
        totalCount.value = transResp.data.totalCount
        categories.value = catResp.data
    } catch (e) {
        console.error("Failed to load transactions", e)
    } finally {
        loading.value = false
    }
}

onMounted(async () => {
    try {
        const spaces = await axios.get('http://localhost:5194/api/BudgetSpaces')
        if (spaces.data.length > 0) {
            spaceId.value = spaces.data[0].id
            await fetchData()
        }
    } catch (e) {
        console.error("Failed to initialize transactions page", e)
    }
})

watch(filters, () => {
    fetchData()
}, { deep: true })

function editTransaction(tx: any) {
    selectedTransaction.value = tx
    showEditDialog.value = true
}

async function deleteTransaction(id: string) {
    if (confirm("Delete this transaction?")) {
        await axios.delete(`http://localhost:5194/api/transactions/${id}?budgetSpaceId=${spaceId.value}`)
        fetchData()
    }
}
</script>

<template>
  <div class="p-8">
    <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-white">All Transactions</h1>
        <router-link to="/" class="text-finance-cyan hover:underline">← Back to Dashboard</router-link>
    </div>

    <!-- Filters -->
    <div class="bg-finance-card border border-white/10 rounded-2xl p-6 mb-8 flex flex-wrap gap-4">
        <div class="flex-1 min-w-[200px]">
            <label class="block text-xs font-bold text-finance-muted uppercase mb-2">Search</label>
            <input v-model="filters.q" type="text" placeholder="Merchant or note..." class="w-full bg-finance-bg border border-white/10 rounded-lg px-4 py-2 text-white">
        </div>
        <div class="w-40">
            <label class="block text-xs font-bold text-finance-muted uppercase mb-2">Type</label>
            <select v-model="filters.type" class="w-full bg-finance-bg border border-white/10 rounded-lg px-4 py-2 text-white">
                <option value="">All</option>
                <option value="Income">Income</option>
                <option value="Expense">Expense</option>
            </select>
        </div>
        <div class="w-48">
            <label class="block text-xs font-bold text-finance-muted uppercase mb-2">Category</label>
            <select v-model="filters.categoryId" class="w-full bg-finance-bg border border-white/10 rounded-lg px-4 py-2 text-white">
                <option value="">All Categories</option>
                <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
        </div>
    </div>

    <!-- Table -->
    <div class="bg-finance-card border border-white/10 rounded-2xl overflow-hidden">
        <table class="w-full text-left">
            <thead class="bg-white/5 text-finance-muted text-xs uppercase font-bold">
                <tr>
                    <th class="px-6 py-4">Date</th>
                    <th class="px-6 py-4">Merchant</th>
                    <th class="px-6 py-4">Category</th>
                    <th class="px-6 py-4 text-right">Amount</th>
                    <th class="px-6 py-4 text-center">Actions</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-white/5">
                <tr v-for="tx in transactions" :key="tx.id" class="hover:bg-white/5 transition-colors">
                    <td class="px-6 py-4 text-sm">{{ new Date(tx.date).toLocaleDateString() }}</td>
                    <td class="px-6 py-4 font-medium">{{ tx.merchant }}</td>
                    <td class="px-6 py-4">
                        <span v-if="tx.categoryName" class="px-2 py-1 bg-finance-cyan/10 text-finance-cyan rounded text-xs">{{ tx.categoryName }}</span>
                    </td>
                    <td class="px-6 py-4 text-right font-bold" :class="tx.type === 'Income' ? 'text-finance-green' : 'text-white'">
                        {{ tx.type === 'Income' ? '+' : '-' }}${{ tx.amount.toLocaleString() }}
                    </td>
                    <td class="px-6 py-4">
                        <div class="flex justify-center space-x-2">
                             <button @click="editTransaction(tx)" class="text-finance-cyan p-1 hover:bg-finance-cyan/20 rounded">Edit</button>
                             <button @click="deleteTransaction(tx.id)" class="text-finance-red p-1 hover:bg-finance-red/20 rounded">Delete</button>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        
        <!-- Pagination -->
        <div class="p-6 border-t border-white/5 flex justify-between items-center text-sm text-finance-muted">
            <span>Showing {{ transactions.length }} of {{ totalCount }}</span>
            <div class="flex space-x-2">
                <button :disabled="filters.page === 1" @click="filters.page--" class="px-3 py-1 bg-white/5 rounded disabled:opacity-30">Prev</button>
                <button :disabled="filters.page * filters.pageSize >= totalCount" @click="filters.page++" class="px-3 py-1 bg-white/5 rounded disabled:opacity-30">Next</button>
            </div>
        </div>
    </div>

    <TransactionFormDialog 
        v-if="showEditDialog"
        mode="edit"
        :budget-space-id="spaceId"
        :transaction="selectedTransaction"
        @saved="fetchData(); showEditDialog = false"
        @close="showEditDialog = false"
    />
  </div>
</template>
