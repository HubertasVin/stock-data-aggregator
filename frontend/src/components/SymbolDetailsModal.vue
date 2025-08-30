<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, computed } from 'vue'
import { fetchSymbolMetrics } from '../api'
import type { SymbolMetrics } from '../types'
import { formatAmount } from '../currency'

const props = defineProps<{ open: boolean; symbol: string | null }>()
const emit = defineEmits<{ (e: 'update:open', v: boolean): void }>()

const loading = ref(false)
const error = ref('')
const data = ref<SymbolMetrics | null>(null)

function close() { emit('update:open', false) }

function fmtPercent(n: number | null | undefined, digits = 2) {
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
function fmtMoney(n: number | null | undefined) {
  if (!data.value) return '—'
  if (n === null || n === undefined || Number.isNaN(n)) return '—'
  return formatAmount(n, data.value.currency)
}

async function load() {
  if (!props.open || !props.symbol) return
  loading.value = true
  error.value = ''
  data.value = null
  try {
    data.value = await fetchSymbolMetrics(props.symbol)
  } catch (e) {
    error.value = String(e)
  } finally {
    loading.value = false
  }
}

function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') close()
}

watch(() => props.open, (v) => { if (v) load() })
watch(() => props.symbol, () => { if (props.open) load() })

onMounted(() => { window.addEventListener('keydown', onKey) })
onUnmounted(() => { window.removeEventListener('keydown', onKey) })

const allYears = computed<number[]>(() => {
  const years = new Set<number>()
  for (const d of (data.value?.revenueYearly ?? [])) years.add(d.year)
  for (const d of (data.value?.earningsYearly ?? [])) years.add(d.year)
  return Array.from(years).sort((a, b) => a - b)
})
</script>

<template>
  <div v-if="open" class="modal-backdrop" @click.self="close">
    <div class="modal modal--details" role="dialog" aria-modal="true">
      <div class="modal-header">
        <div class="modal-title">Details — {{ symbol }}</div>
        <button class="toggle" aria-label="Close" @click="close">✕</button>
      </div>

      <div v-if="loading" class="status">Loading…</div>
      <div v-else-if="error" class="status error">{{ error }}</div>
      <div v-else-if="data">
        <div class="table-wrap mini modal-table-wrap">
          <table class="bounds-table compact details-table">
            <tbody>
              <tr>
                <th>Date</th>
                <td>{{ fmtLocalDateYYYYMMDD(data.date) }}</td>
              </tr>
              <tr>
                <th>Updated</th>
                <td>{{ fmtLocalDateYYYYMMDD(data.updateDate) }}</td>
              </tr>

              <tr>
                <th>1Y Sales Growth</th>
                <td>{{ fmtPercent(data.oneYearSalesGrowth, 2) }}</td>
              </tr>
              <tr>
                <th>4Y Sales Growth</th>
                <td>{{ fmtPercent(data.fourYearSalesGrowth, 2) }}</td>
              </tr>
              <tr>
                <th>4Y Earnings Growth</th>
                <td>{{ fmtPercent(data.fourYearEarningsGrowth, 2) }}</td>
              </tr>

              <tr>
                <th>Free Cash Flow</th>
                <td>{{ fmtMoney(data.freeCashFlow) }}</td>
              </tr>
              <tr>
                <th>Debt / Equity</th>
                <td>{{ data.debtToEquity ?? '—' }}</td>
              </tr>
              <tr>
                <th>PEG</th>
                <td>{{ data.pegRatio?.toPrecision ? data.pegRatio.toPrecision(4) : (data.pegRatio ?? '—') }}</td>
              </tr>
              <tr>
                <th>Return on Equity</th>
                <td>{{ fmtPercent(data.returnOnEquity, 2) }}</td>
              </tr>
              <tr v-if="data.dividendYield != null">
                <th>Dividend Yield</th>
                <td>{{ fmtPercent(data.dividendYield, 2) }}</td>
              </tr>

              <tr>
                <th>ESG Total</th>
                <td>{{ data.esgTotal }} / 30</td>
              </tr>
              <tr>
                <th>ESG Environment</th>
                <td>{{ data.esgEnvironment }} / 10</td>
              </tr>
              <tr>
                <th>ESG Social</th>
                <td>{{ data.esgSocial }} / 10</td>
              </tr>
              <tr>
                <th>ESG Governance</th>
                <td>{{ data.esgGovernance }} / 10</td>
              </tr>
              <tr>
                <th>ESG Publication Date</th>
                <td>{{ data.esgPublicationDate ? fmtLocalDateYYYYMMDD(data.esgPublicationDate) : '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="allYears.length" class="modal-table-wrap">
          <div class="subtle">Yearly Financials</div>
          <div class="table-wrap mini">
            <table class="bounds-table compact details-table">
              <thead>
                <tr>
                  <th>Year</th>
                  <th>Revenue</th>
                  <th>Earnings</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="y in allYears" :key="y">
                  <td>{{ y }}</td>
                  <td>{{fmtMoney(data.revenueYearly?.find(d => d.year === y)?.value ?? null)}}</td>
                  <td>{{fmtMoney(data.earningsYearly?.find(d => d.year === y)?.value ?? null)}}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>
