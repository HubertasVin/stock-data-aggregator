export interface MetricBounds {
  lower: number | null;
  upper: number | null;
}

export interface YearDatum {
  year: number;
  revenue?: number | null;
  earnings?: number | null;
}

export interface BalancedRiskMetrics {
  symbol: string;
  date: string;
  updateDate: string;

  oneYearSalesGrowth: number | null;
  fourYearSalesGrowth: number | null;
  fourYearEarningsGrowth: number | null;

  freeCashFlow: number | null;
  debtToEquity: number | null;
  pegRatio: number | null;
  returnOnEquity: number | null;

  oneYearSalesGrowthBounds?: MetricBounds | null;
  fourYearSalesGrowthBounds?: MetricBounds | null;
  fourYearEarningsGrowthBounds?: MetricBounds | null;
  freeCashFlowBounds?: MetricBounds | null;
  debtToEquityBounds?: MetricBounds | null;
  pegRatioBounds?: MetricBounds | null;
  returnOnEquityBounds?: MetricBounds | null;

  financialsYearly?: YearDatum[];

  score: number;
}
