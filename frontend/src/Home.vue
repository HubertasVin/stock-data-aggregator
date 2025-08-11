<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import StocksTable from './components/StocksTable.vue'
import { fetchAllMetrics, type OkResult, type ErrResult } from './api'
import type { BalancedRiskMetrics, MetricBounds } from './types'
import { metricsMeta, type MetricKey } from './metrics'

const loading = ref(true)
const error = ref('')
const rows = ref<BalancedRiskMetrics[]>([])
const showBalanced = ref(true)
const showAbout = ref(false)

function isOk(r: OkResult | ErrResult): r is OkResult { return r.ok === true }
function isErr(r: OkResult | ErrResult): r is ErrResult { return r.ok === false }

async function refresh() {
    loading.value = true
    error.value = ''
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
}

function onSymbolAdded() { refresh() }

onMounted(() => {
    refresh()
    window.addEventListener('symbol-added', onSymbolAdded as EventListener)
})
onUnmounted(() => {
    window.removeEventListener('symbol-added', onSymbolAdded as EventListener)
})

const firstBounds = computed(() => {
    const r = rows.value[0]
    if (!r) return null
    return {
        oneYearSalesGrowth: r.oneYearSalesGrowthBounds ?? null,
        fiveYearSalesGrowth: r.fiveYearSalesGrowthBounds ?? null,
        fiveYearEarningsGrowth: r.fiveYearEarningsGrowthBounds ?? null,
        freeCashFlow: r.freeCashFlowBounds ?? null,
        debtToEquity: r.debtToEquityBounds ?? null,
        pegRatio: r.pegRatioBounds ?? null,
        returnOnEquity: r.returnOnEquityBounds ?? null,
    } as Record<MetricKey, MetricBounds | null>
})

function fmtNum(n: number, asPercent = false) {
    if (asPercent) return `${(n * 100)}%`
    return Number.isFinite(n) ? n.toLocaleString() : String(n)
}

function fmtBounds(b?: MetricBounds | null, asPercent = false) {
    if (!b) return '—'
    const lo = b.lower
    const up = b.upper
    if (lo != null && up != null) return `${fmtNum(lo, asPercent)} < x > ${fmtNum(up, asPercent)}`
    if (lo != null) return `${fmtNum(lo, asPercent)} < x`
    if (up != null) return `x > ${fmtNum(up, asPercent)}`
    return '—'
}

const metricsMetaList = computed(() => Object.values(metricsMeta))
</script>

<template>
    <section class="section">
        <div class="section-head" @click="showBalanced = !showBalanced">
            <div class="section-title">Balanced Risk</div>
            <div class="caret">
                <img v-if="showBalanced" src="/chevron-up.svg" alt="" class="icon-24 icon-img" />
                <img v-else src="/chevron-down.svg" alt="" class="icon-24 icon-img" />
            </div>
        </div>

        <div v-show="showBalanced" class="section-body">
            <div class="accordion">
                <div class="accordion-head" @click="showAbout = !showAbout">
                    <div>About metrics</div>
                    <div class="caret">
                        <img v-if="showAbout" src="/chevron-up.svg" alt="" class="icon-24 icon-img" />
                        <img v-else src="/chevron-down.svg" alt="" class="icon-24 icon-img" />
                    </div>
                </div>

                <div v-show="showAbout" class="accordion-body">
                    <div v-if="firstBounds">
                        <div class="subtle" style="margin:12px 0 8px">Definitions</div>
                        <div class="defs-list">
                            <div class="def-card" v-for="m in metricsMetaList" :key="m.short">
                                <div class="def-line"><strong>{{ m.short }}</strong> - {{ m.full }}</div>
                                <div class="def-sub"><span class="label">Calculated by:</span> {{ m.calc }}</div>
                                <div class="def-sub"><span class="label">Value meaning:</span> {{ m.meaning }}</div>
                            </div>
                        </div>

                        <div class="subtle" style="margin:12px 0 8px">Bounds</div>
                        <div class="table-wrap mini">
                            <table class="bounds-table compact">
                                <thead>
                                    <tr>
                                        <th>Metric</th>
                                        <th>Bounds</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>1Y Sales</td>
                                        <td>{{ fmtBounds(firstBounds.oneYearSalesGrowth, true) }}</td>
                                    </tr>
                                    <tr>
                                        <td>5Y Sales</td>
                                        <td>{{ fmtBounds(firstBounds.fiveYearSalesGrowth, true) }}</td>
                                    </tr>
                                    <tr>
                                        <td>5Y Earnings</td>
                                        <td>{{ fmtBounds(firstBounds.fiveYearEarningsGrowth, true) }}</td>
                                    </tr>
                                    <tr>
                                        <td>FCF</td>
                                        <td>{{ fmtBounds(firstBounds.freeCashFlow) }}</td>
                                    </tr>
                                    <tr>
                                        <td>D/E</td>
                                        <td>{{ fmtBounds(firstBounds.debtToEquity) }}</td>
                                    </tr>
                                    <tr>
                                        <td>PEG</td>
                                        <td>{{ fmtBounds(firstBounds.pegRatio) }}</td>
                                    </tr>
                                    <tr>
                                        <td>ROE</td>
                                        <td>{{ fmtBounds(firstBounds.returnOnEquity, true) }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div v-else class="subtle">No data</div>
                </div>
            </div>

            <div v-if="loading" class="status">Loading…</div>
            <div v-else-if="error" class="status error">{{ error }}</div>
            <div v-else>
                <div v-if="rows.length < 5" class="status">Scores are relative. Add more symbols for meaningful results.
                </div>
                <StocksTable :rows="rows" />
            </div>
        </div>
    </section>
</template>
