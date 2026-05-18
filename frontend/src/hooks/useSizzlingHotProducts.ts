import { useQuery } from '@tanstack/react-query';
import { fetchSizzlingHotProducts } from '../api/productsApi';

export function useSizzlingHotProducts(today?: string) {
  return useQuery({
    queryKey: ['sizzling-hot-products', today],
    queryFn: () => fetchSizzlingHotProducts(today),
    staleTime: 60_000,
    retry: 2,
  });
}
