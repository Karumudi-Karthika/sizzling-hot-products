export type DailySizzlingResult = {
  date: string;
  productId: string;
  productName: string;
  saleCount: number;
};

export type PeriodSizzlingResult = {
  periodStart: string;
  periodEnd: string;
  productId: string;
  productName: string;
  saleCount: number;
};

export type SizzlingHotProductsResponse = {
  dailyResults: DailySizzlingResult[];
  threeDayResult: PeriodSizzlingResult;
};
