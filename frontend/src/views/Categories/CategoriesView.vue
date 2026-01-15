<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

const categories = ref<any[]>([])
const loading = ref(true)
const spaceId = ref('')
const newCategory = ref({ name: '', appliesTo: 2 }) // Both

async function fetchCategories() {
    loading.value = true
    try {
        const res = await axios.get(`http://localhost:5194/api/Categories?budgetSpaceId=${spaceId.value}`)
        categories.value = res.data
    } catch (e) {
        console.error("Failed to fetch categories", e)
    } finally {
        loading.value = false
    }
}

onMounted(async () => {
    try {
        const spaces = await axios.get('http://localhost:5194/api/BudgetSpaces')
        if (spaces.data.length > 0) {
            spaceId.value = spaces.data[0].id
            await fetchCategories()
        }
    } catch (e) {
        console.error("Failed to initialize categories page", e)
    }
})

async function addCategory() {
    if (!newCategory.value.name) return
    try {
        await axios.post('http://localhost:5194/api/Categories', {
            budgetSpaceId: spaceId.value,
            name: newCategory.value.name,
            appliesTo: newCategory.value.appliesTo
        })
        newCategory.value.name = ''
        await fetchCategories()
    } catch (e: any) {
        alert(e.response?.data || "Failed to add category")
    }
}

async function deleteCategory(id: string) {
    if (!confirm("Are you sure? Transactions using this category will be preserved but the category label will be removed.")) return
    try {
        await axios.delete(`http://localhost:5194/api/Categories/${id}`)
        await fetchCategories()
    } catch (e: any) {
        alert(e.response?.data || "Failed to delete category")
    }
}
</script>

<template>
  <div class="p-8 max-w-4xl mx-auto">
    <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-white">Manage Categories</h1>
        <router-link to="/" class="text-finance-cyan hover:underline">← Back</router-link>
    </div>

    <!-- Quick Add -->
    <div class="bg-finance-card border border-white/10 rounded-2xl p-6 mb-8">
        <h2 class="text-xs font-bold text-finance-muted uppercase mb-4 tracking-widest">Create New Category</h2>
        <div class="flex gap-4">
            <input 
                v-model="newCategory.name" 
                type="text" 
                placeholder="e.g. Travel, Education..." 
                class="flex-1 bg-finance-bg border border-white/10 rounded-xl px-4 py-2 text-white"
                @keyup.enter="addCategory"
            >
            <select v-model="newCategory.appliesTo" class="bg-finance-bg border border-white/10 rounded-xl px-4 py-2 text-white">
                <option :value="0">Income Only</option>
                <option :value="1">Expense Only</option>
                <option :value="2">Both</option>
            </select>
            <button 
                @click="addCategory"
                class="px-6 py-2 bg-finance-cyan text-black font-bold rounded-xl hover:bg-white transition-all"
            >
                Add
            </button>
        </div>
    </div>

    <!-- List -->
    <div class="bg-finance-card border border-white/10 rounded-2xl overflow-hidden">
        <div v-if="loading" class="p-8 text-center text-finance-muted italic">Loading catalog...</div>
        <table v-else class="w-full">
            <tbody class="divide-y divide-white/5">
                <tr v-for="cat in categories" :key="cat.id" class="hover:bg-white/5 transition-all">
                    <td class="px-6 py-4 font-medium text-white">{{ cat.name }}</td>
                    <td class="px-6 py-4">
                        <span v-if="cat.appliesTo === 0" class="text-xs text-finance-green">Income</span>
                        <span v-else-if="cat.appliesTo === 1" class="text-xs text-finance-red">Expense</span>
                        <span v-else class="text-xs text-finance-muted">Universal</span>
                    </td>
                    <td class="px-6 py-4 text-right">
                        <button @click="deleteCategory(cat.id)" class="text-finance-red hover:bg-finance-red/10 px-3 py-1 rounded text-sm transition-all">
                            Remove
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
  </div>
</template>
