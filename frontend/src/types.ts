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
  esgTotal?: number;
  esgEnvironment?: number;
  esgSocial?: number;
  esgGovernance?: number;
}
