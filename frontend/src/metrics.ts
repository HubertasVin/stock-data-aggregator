export type MetricKey =
  | "oneYearSalesGrowth"
  | "fourYearSalesGrowth"
  | "fourYearEarningsGrowth"
  | "freeCashFlow"
  | "debtToEquity"
  | "pegRatio"
  | "returnOnEquity"
  | "score";

export interface MetricMeta {
  key: MetricKey;
  short: string;
  full: string;
  isPercent?: boolean;
  higherIsBetter?: boolean;
}

export const metricsMeta: Record<MetricKey, MetricMeta> = {
  oneYearSalesGrowth: {
    key: "oneYearSalesGrowth",
    short: "1Y Sales",
    full: "One-year Revenue Growth",
    isPercent: true,
    higherIsBetter: true,
  },
  fourYearSalesGrowth: {
    key: "fourYearSalesGrowth",
    short: "4Y Sales",
    full: "Four-year Revenue Growth (compounded)",
    isPercent: true,
    higherIsBetter: true,
  },
  fourYearEarningsGrowth: {
    key: "fourYearEarningsGrowth",
    short: "4Y Earnings",
    full: "Four-year Net Income Growth (compounded)",
    isPercent: true,
    higherIsBetter: true,
  },
  freeCashFlow: {
    key: "freeCashFlow",
    short: "FCF",
    full: "Free Cash Flow",
    higherIsBetter: true,
  },
  debtToEquity: {
    key: "debtToEquity",
    short: "D/E",
    full: "Debt to Equity",
    higherIsBetter: false,
  },
  pegRatio: {
    key: "pegRatio",
    short: "PEG",
    full: "Price/Earnings-to-Growth Ratio",
    higherIsBetter: false,
  },
  returnOnEquity: {
    key: "returnOnEquity",
    short: "ROE",
    full: "Return on Equity",
    isPercent: true,
    higherIsBetter: true,
  },
  score: {
    key: "score",
    short: "Score",
    full: "Composite Balanced Risk Score",
    higherIsBetter: true,
  },
};
