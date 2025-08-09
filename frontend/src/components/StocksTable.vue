<script setup lang="ts">
import { ref, computed } from 'vue'
import type { BalancedRiskMetrics } from '../types'

type Row = BalancedRiskMetrics
const props = defineProps<{ rows: Row[] }>()

const query = ref('')
const sortKey = ref<keyof Row>('symbol')
const sortDir = ref<'asc' | 'desc'>('asc')

const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  let data = props.rows
  if (q) {
    data = data.filter(r =>
      r.symbol.toLowerCase().includes(q) ||
      (r.date ?? '').toString().includes(q)
    )
  }
  const dir = sortDir.value === 'asc' ? 1 : -1
  return [...data].sort((a, b) => {
    const ka = a[sortKey.value] as unknown
    const kb = b[sortKey.value] as unknown
    if (ka == null && kb == null) return 0
    if (ka == null) return 1
    if (kb == null) return -1
    if (typeof ka === 'number' && typeof kb === 'number') return (ka - kb) * dir
    return String(ka).localeCompare(String(kb)) * dir
  })
})

function sortBy(k: keyof Row) {
  if (sortKey.value === k) sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  else { sortKey.value = k; sortDir.value = 'asc' }
}
</script>

<template>
  <div class="panel">
    <div class="toolbar">
      <input class="input" v-model="query" placeholder="Search symbols..." />
      <span class="subtle">Rows: {{ filtered.length }}</span>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th class="sortable" @click="sortBy('symbol')">
              Symbol <span class="dir">{{ sortKey === 'symbol' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('date')">
              Date <span class="dir">{{ sortKey === 'date' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('oneYearSalesGrowth')">
              1Y Sales <span class="dir">{{ sortKey === 'oneYearSalesGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('fiveYearSalesGrowth')">
              5Y Sales <span class="dir">{{ sortKey === 'fiveYearSalesGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('fiveYearEarningsGrowth')">
              5Y Earnings <span class="dir">{{ sortKey === 'fiveYearEarningsGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : ''
                }}</span>
            </th>
            <th class="sortable" @click="sortBy('freeCashFlow')">
              FCF <span class="dir">{{ sortKey === 'freeCashFlow' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('debtToEquity')">
              D/E <span class="dir">{{ sortKey === 'debtToEquity' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('pegRatio')">
              PEG <span class="dir">{{ sortKey === 'pegRatio' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('returnOnEquity')">
              ROE <span class="dir">{{ sortKey === 'returnOnEquity' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in filtered" :key="r.symbol">
            <td><span class="badge">{{ r.symbol }}</span></td>
            <td>{{ new Date(r.date).toISOString().slice(0, 10) }}</td>
            <td>{{ (r.oneYearSalesGrowth * 100)?.toFixed?.(1) ?? '' }}%</td>
            <td>{{ (r.fiveYearSalesGrowth * 100)?.toFixed?.(1) ?? '' }}%</td>
            <td>{{ (r.fiveYearEarningsGrowth * 100)?.toFixed?.(1) ?? '' }}%</td>
            <td>{{ r.freeCashFlow?.toLocaleString?.() ?? '' }}</td>
            <td>{{ r.debtToEquity?.toFixed?.(3) ?? '' }}</td>
            <td>{{ r.pegRatio?.toFixed?.(3) ?? '' }}</td>
            <td>{{ r.returnOnEquity?.toFixed?.(3) ?? '' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
