export interface MetricBounds {
  lower?: number | null;
  upper?: number | null;
}

export interface BalancedRiskMetrics {
  symbol: string;
  date: string;
  updateDate: string;

  oneYearSalesGrowth: number;
  fiveYearSalesGrowth: number;
  fiveYearEarningsGrowth: number;
  freeCashFlow: number;
  debtToEquity: number;
  pegRatio: number;
  returnOnEquity: number;

  oneYearSalesGrowthBounds?: MetricBounds | null;
  fiveYearSalesGrowthBounds?: MetricBounds | null;
  fiveYearEarningsGrowthBounds?: MetricBounds | null;
  freeCashFlowBounds?: MetricBounds | null;
  debtToEquityBounds?: MetricBounds | null;
  pegRatioBounds?: MetricBounds | null;
  returnOnEquityBounds?: MetricBounds | null;

  score?: number | null;
}
