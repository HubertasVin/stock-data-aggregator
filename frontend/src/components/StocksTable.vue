<script setup lang="ts">
import { ref, computed } from 'vue'
import type { BalancedRiskMetrics, MetricBounds } from '../types'
import { metricsMeta } from '../metrics'

type Row = BalancedRiskMetrics
const props = defineProps<{ rows: Row[] }>()
const emit = defineEmits<{ (e: 'open-details', symbol: string): void }>()

const query = ref('')
const sortKey = ref<keyof Row>('symbol')
const sortDir = ref<'asc' | 'desc'>('asc')

function fmtNumber(n: number | null | undefined) {
  if (n === null || n === undefined || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat().format(n)
}
function fmtPercent(n: number | null | undefined, digits = 1) {
  if (n === null || n === undefined || Number.isNaN(n)) return '—'
  return `${(n * 100).toFixed(digits)}%`
}
function fmtLocalDateYYYYMMDD(s: string | null | undefined) {
  if (!s) return '—'
  const d = new Date(s)
  if (Number.isNaN(d.getTime())) return '—'
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}
function metricClass(value: number | null | undefined, bounds?: MetricBounds | null, _higherIsBetter = true) {
  if (value === null || value === undefined || !bounds) return ''
  const lowerOk = bounds.lower == null || value > Number(bounds.lower)
  const upperOk = bounds.upper == null || value < Number(bounds.upper)
  const ok = lowerOk && upperOk
  return ok ? 'metric-ok' : 'metric-bad'
}

const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  let data = props.rows
  if (q) {
    data = data.filter(r =>
      r.symbol.toLowerCase().includes(q) || (r.date ?? '').toString().includes(q)
    )
  }
  const dir = sortDir.value === 'asc' ? 1 : -1
  return [...data].sort((a, b) => {
    const ka = (a as any)[sortKey.value]
    const kb = (b as any)[sortKey.value]
    if (ka == null && kb == null) return 0
    if (ka == null) return 1 * dir
    if (kb == null) return -1 * dir
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
              {{ metricsMeta.oneYearSalesGrowth.short }}
              <span class="q" :title="metricsMeta.oneYearSalesGrowth.full">?</span>
              <span class="dir">{{ sortKey === 'oneYearSalesGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('fourYearSalesGrowth')">
              {{ metricsMeta.fourYearSalesGrowth.short }}
              <span class="q" :title="metricsMeta.fourYearSalesGrowth.full">?</span>
              <span class="dir">{{ sortKey === 'fourYearSalesGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('fourYearEarningsGrowth')">
              {{ metricsMeta.fourYearEarningsGrowth.short }}
              <span class="q" :title="metricsMeta.fourYearEarningsGrowth.full">?</span>
              <span class="dir">{{ sortKey === 'fourYearEarningsGrowth' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>

            <th class="sortable" @click="sortBy('freeCashFlow')">
              {{ metricsMeta.freeCashFlow.short }}
              <span class="q" :title="metricsMeta.freeCashFlow.full">?</span>
              <span class="dir">{{ sortKey === 'freeCashFlow' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('debtToEquity')">
              {{ metricsMeta.debtToEquity.short }}
              <span class="q" :title="metricsMeta.debtToEquity.full">?</span>
              <span class="dir">{{ sortKey === 'debtToEquity' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('pegRatio')">
              {{ metricsMeta.pegRatio.short }}
              <span class="q" :title="metricsMeta.pegRatio.full">?</span>
              <span class="dir">{{ sortKey === 'pegRatio' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
            <th class="sortable" @click="sortBy('returnOnEquity')">
              {{ metricsMeta.returnOnEquity.short }}
              <span class="q" :title="metricsMeta.returnOnEquity.full">?</span>
              <span class="dir">{{ sortKey === 'returnOnEquity' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>

            <th class="sortable" @click="sortBy('score')">
              {{ metricsMeta.score.short }}
              <span class="q" :title="metricsMeta.score.full">?</span>
              <span class="dir">{{ sortKey === 'score' ? (sortDir === 'asc' ? '▲' : '▼') : '' }}</span>
            </th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="r in filtered" :key="r.symbol">
            <td>
              <button class="badge link-badge" @click="emit('open-details', r.symbol)">{{ r.symbol }}</button>
            </td>
            <td>{{ fmtLocalDateYYYYMMDD(r.date) }}</td>

            <td :class="metricClass(r.oneYearSalesGrowth, r.oneYearSalesGrowthBounds, true)">
              {{ fmtPercent(r.oneYearSalesGrowth) }}
            </td>
            <td :class="metricClass(r.fourYearSalesGrowth, r.fourYearSalesGrowthBounds, true)">
              {{ fmtPercent(r.fourYearSalesGrowth) }}
            </td>
            <td :class="metricClass(r.fourYearEarningsGrowth, r.fourYearEarningsGrowthBounds, true)">
              {{ fmtPercent(r.fourYearEarningsGrowth) }}
            </td>

            <td :class="metricClass(r.freeCashFlow, r.freeCashFlowBounds, true)">
              {{ fmtNumber(r.freeCashFlow) }}
            </td>
            <td :class="metricClass(r.debtToEquity, r.debtToEquityBounds, false)">
              {{ r.debtToEquity ?? '—' }}
            </td>
            <td :class="metricClass(r.pegRatio, r.pegRatioBounds, false)">
              {{ r.pegRatio?.toPrecision(3) ?? '—' }}
            </td>
            <td :class="metricClass(r.returnOnEquity, r.returnOnEquityBounds, true)">
              {{ fmtPercent(r.returnOnEquity) }}
            </td>

            <td>
              <span class="score-pill" :data-score="r.score">{{ r.score ?? '—' }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
