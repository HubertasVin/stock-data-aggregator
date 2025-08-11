export type MetricKey =
  | "oneYearSalesGrowth"
  | "fiveYearSalesGrowth"
  | "fiveYearEarningsGrowth"
  | "freeCashFlow"
  | "debtToEquity"
  | "pegRatio"
  | "returnOnEquity";

export interface MetricInfo {
  short: string;
  full: string;
  calc: string;
  meaning: string;
}

export const metricsMeta: Record<MetricKey, MetricInfo> = {
  oneYearSalesGrowth: {
    short: "1Y Sales",
    full: "One-Year Sales Growth",
    calc: "TTM (trailing twelve months) revenue now vs TTM one year ago.",
    meaning:
      "Higher = faster recent revenue growth. Negative = revenue shrank vs last year.",
  },
  fiveYearSalesGrowth: {
    short: "5Y Sales",
    full: "Five-Year Sales Growth",
    calc: "CAGR (compound annual growth rate) of revenue per share over 5 years.",
    meaning:
      "Higher = long-term sales expansion. Negative = multi-year contraction.",
  },
  fiveYearEarningsGrowth: {
    short: "5Y Earnings",
    full: "Five-Year Earnings Growth",
    calc: "CAGR of net income per share over 5 years.",
    meaning:
      "Higher = profits compounding. Negative = profits fell or turned to losses.",
  },
  freeCashFlow: {
    short: "FCF",
    full: "Free Cash Flow",
    calc: "Operating cash flow minus capital expenditures (capex).",
    meaning:
      "Higher/positive = cash left after investments. Negative = cash burn (heavy capex or weak operations).",
  },
  debtToEquity: {
    short: "D/E",
    full: "Debt to Equity",
    calc: "Total debt divided by shareholders’ equity.",
    meaning:
      "Lower = less leverage risk. Negative often means negative equity, which is a red flag.",
  },
  pegRatio: {
    short: "PEG",
    full: "Price/Earnings-to-Growth",
    calc: "P/E (price-to-earnings) divided by expected earnings growth.",
    meaning:
      "Lower = cheaper vs growth. Negative often comes from negative earnings or growth → interpretation is unreliable.",
  },
  returnOnEquity: {
    short: "ROE",
    full: "Return on Equity",
    calc: "Net income divided by average shareholders’ equity.",
    meaning:
      "Higher = efficient use of equity. Negative = losses or negative equity.",
  },
};
