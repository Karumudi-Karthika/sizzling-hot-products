import { useQuery } from '@tanstack/react-query';
import { fetchSizzlingHotProducts } from '../api/productsApi';

/**
 * Hook that fetches and caches sizzling hot product data.
 * Automatically refetches every 60 seconds to stay fresh.
 */
export function useSizzlingHotProducts(today?: string) {
  return useQuery({
    queryKey: ['sizzling-hot-products', today],
    queryFn: () => fetchSizzlingHotProducts(today),
    staleTime: 60_000,
    retry: 2,
  });
}
