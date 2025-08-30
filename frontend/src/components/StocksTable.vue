<!-- File: components/StocksTable.vue -->
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

function safeVal(v: unknown, fallback: string = '—') {
  if (v === null || v === undefined) return fallback
  return String(v)
}

function fmtNumber(n: number | null | undefined) {
  if (n === null || n === undefined || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat().format(n)
}

function fmtPercent(n: number | null | undefined, digits = 1) {
  if (n === null || n === undefined || Number.isNaN(n)) return '—'
  return `${(n * 100).toFixed(digits)}%`
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
              <button class="badge" style="cursor:pointer;border-color:#2563eb;background:rgba(59,130,246,.08)"
                @click="emit('open-details', r.symbol)">
                {{ r.symbol }}
              </button>
            </td>
            <td>{{ r.date ? new Date(r.date).toISOString().slice(0, 10) : '—' }}</td>

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

<style scoped>
.q {
  margin-left: 6px;
  border: 1px solid var(--border, #e5e7eb);
  border-radius: 999px;
  width: 16px;
  height: 16px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  line-height: 1;
  cursor: help;
  opacity: .7;
}

.metric-ok {
  color: #059669;
  font-weight: 600;
}

.metric-bad {
  color: #dc2626;
  font-weight: 600;
}

.score-pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 999px;
  border: 2px solid #e5e7eb;
  background: #f9fafb;
  color: #111827;
  font-weight: 700;
}

.score-pill[data-score="9"],
.score-pill[data-score="10"] {
  border-color: #16a34a;
  background: #ecfdf5;
  color: #065f46;
}

.score-pill[data-score="7"],
.score-pill[data-score="8"] {
  border-color: #84cc16;
  background: #f7fee7;
  color: #3f6212;
}

.score-pill[data-score="5"],
.score-pill[data-score="6"] {
  border-color: #f59e0b;
  background: #fffbeb;
  color: #7c2d12;
}

.score-pill[data-score="3"],
.score-pill[data-score="4"] {
  border-color: #f97316;
  background: #fff7ed;
  color: #7c2d12;
}

.score-pill[data-score="1"],
.score-pill[data-score="2"] {
  border-color: #dc2626;
  background: #fef2f2;
  color: #7f1d1d;
}
</style>
