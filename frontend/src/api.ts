import type { BalancedRiskMetrics } from "./types";

const BASE =
  (import.meta.env.VITE_API_BASE as string | undefined)?.replace(/\/$/, "") ||
  "";

let authToken: string | null = localStorage.getItem("jwt") || null;

export function setAuthToken(token: string | null) {
  authToken = token;
  if (token) localStorage.setItem("jwt", token);
  else localStorage.removeItem("jwt");
}
export function getAuthToken() {
  return authToken;
}
export function isAuthenticated() {
  return !!authToken;
}

function withAuth(init?: RequestInit): RequestInit {
  const headers = new Headers(init?.headers || {});
  if (authToken) headers.set("Authorization", `Bearer ${authToken}`);
  return { ...init, headers };
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
}

export async function login(
  username: string,
  password: string
): Promise<LoginResponse> {
  const r = await fetch(`${BASE}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  if (!r.ok) throw new Error(`login: ${r.status}`);
  const data = (await r.json()) as LoginResponse;
  setAuthToken(data.token);
  return data;
}

export async function refreshSymbol(symbol: string): Promise<void> {
  const s = symbol.trim().toUpperCase();
  if (!s) throw new Error("Empty symbol");
  const r = await fetch(
    `${BASE}/refresh/${encodeURIComponent(s)}`,
    withAuth({
      method: "POST",
    })
  );
  if (!r.ok) throw new Error(`refresh(${s}): ${r.status}`);
}

export async function addSymbol(symbol: string): Promise<void> {
  const s = symbol.trim().toUpperCase();
  if (!s) throw new Error("Empty symbol");
  const r = await fetch(
    `${BASE}/symbols`,
    withAuth({
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(s),
    })
  );
  if (!r.ok) throw new Error(`addSymbol: ${r.status}`);
  await refreshSymbol(s); // required post-add refresh
}

export interface OkResult {
  ok: true;
  symbol: string;
  data: BalancedRiskMetrics;
}
export interface ErrResult {
  ok: false;
  symbol: string;
  error: string;
}
export type MetricsResult = OkResult | ErrResult;

export async function fetchSymbols(): Promise<string[]> {
  const r = await fetch(`${BASE}/symbols`);
  if (!r.ok) throw new Error(`symbols: ${r.status}`);
  return r.json();
}

export async function fetchBalancedRisk(
  symbol: string
): Promise<BalancedRiskMetrics> {
  const r = await fetch(`${BASE}/balancedrisk/${encodeURIComponent(symbol)}`);
  if (!r.ok) throw new Error(`balancedrisk(${symbol}): ${r.status}`);
  return r.json();
}

export async function fetchAllMetrics(): Promise<MetricsResult[]> {
  const symbols = await fetchSymbols();
  const tasks = symbols.map(async (s): Promise<MetricsResult> => {
    try {
      const data = await fetchBalancedRisk(s);
      return { ok: true as const, symbol: s, data };
    } catch (e) {
      return { ok: false as const, symbol: s, error: String(e) };
    }
  });
  return Promise.all(tasks);
}
