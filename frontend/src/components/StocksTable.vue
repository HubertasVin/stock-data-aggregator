<script setup lang="ts">
import { ref, computed } from 'vue'
import type { BalancedRiskMetrics, MetricBounds } from '../types'
import { metricsMeta } from '../metrics'

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

// returns: true=in bounds, false=out of bounds, null=no bounds provided
function inBounds(
  value: number | null | undefined,
  b: MetricBounds | null | undefined
): boolean | null {
  if (value == null || !b) return null
  const hasLo = b.lower != null
  const hasUp = b.upper != null
  if (!hasLo && !hasUp) return null // no constraints -> neutral
  if (hasLo && !(value > (b.lower as number))) return false
  if (hasUp && !(value < (b.upper as number))) return false
  return true
}

function metricClass(
  value: number | null | undefined,
  b: MetricBounds | null | undefined
) {
  const res = inBounds(value, b)
  if (res === true) return 'metric-ok'
  if (res === false) return 'metric-bad'
  return '' // neutral when no bounds
}

function scoreClass(score: number | null | undefined) {
  if (score == null) return 'score-mid'
  if (score <= 4) return 'score-bad'
  if (score <= 7) return 'score-mid'
  if (score <= 9) return 'score-good'
  return 'score-great'
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
              Symbol
              <span class="dir-icon" v-if="sortKey === 'symbol'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('date')">
              Date
              <span class="dir-icon" v-if="sortKey === 'date'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('oneYearSalesGrowth')">
              {{ metricsMeta.oneYearSalesGrowth.short }}
              <span class="qmark" :title="metricsMeta.oneYearSalesGrowth.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'oneYearSalesGrowth'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('fiveYearSalesGrowth')">
              {{ metricsMeta.fiveYearSalesGrowth.short }}
              <span class="qmark" :title="metricsMeta.fiveYearSalesGrowth.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'fiveYearSalesGrowth'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('fiveYearEarningsGrowth')">
              {{ metricsMeta.fiveYearEarningsGrowth.short }}
              <span class="qmark" :title="metricsMeta.fiveYearEarningsGrowth.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'fiveYearEarningsGrowth'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('freeCashFlow')">
              {{ metricsMeta.freeCashFlow.short }}
              <span class="qmark" :title="metricsMeta.freeCashFlow.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'freeCashFlow'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('debtToEquity')">
              {{ metricsMeta.debtToEquity.short }}
              <span class="qmark" :title="metricsMeta.debtToEquity.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'debtToEquity'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('pegRatio')">
              {{ metricsMeta.pegRatio.short }}
              <span class="qmark" :title="metricsMeta.pegRatio.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'pegRatio'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('returnOnEquity')">
              {{ metricsMeta.returnOnEquity.short }}
              <span class="qmark" :title="metricsMeta.returnOnEquity.full">?</span>
              <span class="dir-icon" v-if="sortKey === 'returnOnEquity'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
            <th class="sortable" @click="sortBy('score')">
              Score <span class="qmark" title="Composite score (1–10) vs universe">?</span>
              <span class="dir-icon" v-if="sortKey === 'score'">
                <img v-if="sortDir === 'asc'" src="/chevron-up.svg" alt="" class="icon-16 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-16 icon-img" />
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in filtered" :key="r.symbol">
            <td><span class="badge">{{ r.symbol }}</span></td>
            <td>{{ new Date(r.date).toISOString().slice(0, 10) }}</td>

            <td :class="metricClass(r.oneYearSalesGrowth, r.oneYearSalesGrowthBounds)">
              {{ (r.oneYearSalesGrowth * 100)?.toFixed?.(1) ?? '' }}%
            </td>
            <td :class="metricClass(r.fiveYearSalesGrowth, r.fiveYearSalesGrowthBounds)">
              {{ (r.fiveYearSalesGrowth * 100)?.toFixed?.(1) ?? '' }}%
            </td>
            <td :class="metricClass(r.fiveYearEarningsGrowth, r.fiveYearEarningsGrowthBounds)">
              {{ (r.fiveYearEarningsGrowth * 100)?.toFixed?.(1) ?? '' }}%
            </td>
            <td :class="metricClass(r.freeCashFlow, r.freeCashFlowBounds)">
              {{ r.freeCashFlow?.toLocaleString?.() ?? '' }}
            </td>
            <td :class="metricClass(r.debtToEquity, r.debtToEquityBounds)">
              {{ r.debtToEquity?.toFixed?.(3) ?? '' }}
            </td>
            <td :class="metricClass(r.pegRatio, r.pegRatioBounds)">
              {{ r.pegRatio?.toFixed?.(3) ?? '' }}
            </td>
            <td :class="metricClass(r.returnOnEquity, r.returnOnEquityBounds)">
              {{ (r.returnOnEquity * 100)?.toFixed?.(1) ?? '' }}%
            </td>
            <td><span :class="['score-pill', scoreClass(r.score)]">{{ r.score }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
