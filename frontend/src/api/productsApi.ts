import axios from 'axios';

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

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

const client = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

export async function fetchSizzlingHotProducts(today?: string): Promise<SizzlingHotProductsResponse> {
  const params = today ? { today } : {};
  const { data } = await client.get<SizzlingHotProductsResponse>('/api/products/sizzling-hot', { params });
  return data;
}
