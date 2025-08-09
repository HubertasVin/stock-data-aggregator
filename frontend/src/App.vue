<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { fetchAllMetrics, type OkResult, type ErrResult } from './api'
import StocksTable from './components/StocksTable.vue'
import type { BalancedRiskMetrics } from './types'

const loading = ref(true)
const error = ref('')
const rows = ref<BalancedRiskMetrics[]>([])

function isOk(r: OkResult | ErrResult): r is OkResult {
  return r.ok === true
}
function isErr(r: OkResult | ErrResult): r is ErrResult {
  return r.ok === false
}

onMounted(async () => {
  try {
    const results = await fetchAllMetrics()

    const oks = results.filter(isOk)
    rows.value = oks.map(r => r.data)

    const failed = results.filter(isErr)
    if (failed.length) error.value = `Failed: ${failed.map(f => f.symbol).join(', ')}`
  } catch (e) {
    error.value = String(e)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="container">
    <div class="header">
      <div class="title">Stocks</div>
      <div class="subtle">balanced risk metrics</div>
    </div>

    <div v-if="loading" class="status">Loading…</div>
    <div v-else-if="error" class="status error">{{ error }}</div>
    <StocksTable v-else :rows="rows" />

    <footer>Powered by StockDataAggregator</footer>
  </div>
</template>
