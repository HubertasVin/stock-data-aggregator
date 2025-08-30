export interface MetricBounds {
  lower: number | null;
  upper: number | null;
}

export interface YearValue {
  year: number;
  value: number | null;
}

export interface BalancedRiskMetrics {
  symbol: string;
  date: string;
  updateDate: string;
  currency: string;

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

  score: number;
}

export interface SymbolMetrics {
  symbol: string;
  date: string;
  updateDate: string;
  currency: string;

  oneYearSalesGrowth: number | null;
  fourYearSalesGrowth: number | null;
  fourYearEarningsGrowth: number | null;

  freeCashFlow: number | null;
  debtToEquity: number | null;
  pegRatio: number | null;
  returnOnEquity: number | null;
  dividendYield?: number | null;

  esgTotal: number;
  esgEnvironment: number;
  esgSocial: number;
  esgGovernance: number;
  esgPublicationDate: string | null;

  revenueYearly: YearValue[];
  earningsYearly: YearValue[];
}
