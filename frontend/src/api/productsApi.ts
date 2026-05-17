import axios from 'axios';
import { SizzlingHotProductsResponse } from '../types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

const client = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

/**
 * Fetches sizzling hot product results from the backend.
 * @param today Optional date override in dd/MM/yyyy format.
 */
export async function fetchSizzlingHotProducts(
  today?: string
): Promise<SizzlingHotProductsResponse> {
  const params = today ? { today } : {};
  const { data } = await client.get<SizzlingHotProductsResponse>(
    '/api/products/sizzling-hot',
    { params }
  );
  return data;
}
