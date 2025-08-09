import type { BalancedRiskMetrics } from './types'

const BASE = (import.meta.env.VITE_API_BASE as string | undefined)?.replace(/\/$/, '') || ''

export interface OkResult {
  ok: true
  symbol: string
  data: BalancedRiskMetrics
}
export interface ErrResult {
  ok: false
  symbol: string
  error: string
}
export type MetricsResult = OkResult | ErrResult

export async function fetchSymbols(): Promise<string[]> {
  const r = await fetch(`${BASE}/symbols`)
  if (!r.ok) throw new Error(`symbols: ${r.status}`)
  return r.json()
}

export async function fetchBalancedRisk(symbol: string): Promise<BalancedRiskMetrics> {
  const r = await fetch(`${BASE}/balancedrisk/${encodeURIComponent(symbol)}`)
  if (!r.ok) throw new Error(`balancedrisk(${symbol}): ${r.status}`)
  return r.json()
}

export async function fetchAllMetrics(): Promise<MetricsResult[]> {
  const symbols = await fetchSymbols()
  const tasks = symbols.map(async (s): Promise<MetricsResult> => {
    try {
      const data = await fetchBalancedRisk(s)
      return { ok: true as const, symbol: s, data }
    } catch (e) {
      return { ok: false as const, symbol: s, error: String(e) }
    }
  })
  return Promise.all(tasks)
}
