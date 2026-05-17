export interface DailySizzlingResult {
  date: string;
  productId: string;
  productName: string;
  saleCount: number;
}

export interface PeriodSizzlingResult {
  periodStart: string;
  periodEnd: string;
  productId: string;
  productName: string;
  saleCount: number;
}

export interface SizzlingHotProductsResponse {
  dailyResults: DailySizzlingResult[];
  threeDayResult: PeriodSizzlingResult;
}
