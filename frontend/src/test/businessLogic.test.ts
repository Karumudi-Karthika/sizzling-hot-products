import { describe, it, expect } from 'vitest';

// ─────────────────────────────────────────────────────────────────────────────
// Pure TypeScript port of the core scoring algorithm.
// These tests validate the same business rules as the .NET unit tests,
// giving the front-end team confidence the logic is correct independently.
// ─────────────────────────────────────────────────────────────────────────────

interface OrderEntry { id: string; quantity: number; }
interface Order {
  orderId: string;
  customerId?: string;
  entries?: OrderEntry[];
  date: string;
  status: string;
}
interface Product { id: string; name: string; }
interface SaleScore { productId: string; productName: string; count: number; }

function parseDate(d: string): string { return d; } // keep as string for comparison

/** Core scoring function - mirrors SizzlingProductService.CalculateProductScores */
function calculateTopProduct(
  orders: Order[],
  products: Product[],
  from: string,
  to: string
): SaleScore | null {
  const productMap = new Map(products.map(p => [p.id, p.name]));

  // BR3 - collect cancelled order IDs
  const cancelledIds = new Set(
    orders.filter(o => o.status === 'cancelled').map(o => o.orderId)
  );

  // Valid completed orders in window
  const validOrders = orders.filter(o =>
    o.status === 'completed' &&
    !cancelledIds.has(o.orderId) &&
    o.date >= from &&
    o.date <= to
  );

  // BR1 + BR2 - deduplicate by (customer, date, product)
  const seen = new Set<string>();
  const scores = new Map<string, number>();

  for (const order of validOrders) {
    if (!order.customerId || !order.entries) continue;
    const uniqueProducts = [...new Set(order.entries.map(e => e.id))];
    for (const pid of uniqueProducts) {
      if (!productMap.has(pid)) continue;
      const key = `${order.customerId}|${order.date}|${pid}`;
      if (seen.has(key)) continue;
      seen.add(key);
      scores.set(pid, (scores.get(pid) ?? 0) + 1);
    }
  }

  if (scores.size === 0) return null;

  // BR4 - highest score, then alphabetical on tie
  const winner = [...scores.entries()]
    .map(([id, count]) => ({ productId: id, productName: productMap.get(id)!, count }))
    .sort((a, b) => b.count - a.count || a.productName.localeCompare(b.productName))[0];

  return winner;
}

// ─── Sample data from the repo ─────────────────────────────────────────────

const PRODUCTS: Product[] = [
  { id: 'P1', name: 'Ezy Storage 37L Flexi Laundry Basket - White' },
  { id: 'P2', name: 'Aandleford Black Seaford Post Mounted Letterbox' },
  { id: 'P3', name: 'Coolaroo 5.4m Square Graphite Premium Shade Sail Kit' },
  { id: 'P4', name: 'Ozito 80W Soldering Iron' },
  { id: 'P5', name: 'Richgro 25L All Purpose Garden Soil Mix' },
  { id: 'P6', name: 'Arlec 160W Crystalline Solar Foldable Charging Kit' },
];

const ORDERS: Order[] = [
  { orderId: 'O10', customerId: 'C1',  date: '21/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
  { orderId: 'O20', customerId: 'C2',  date: '21/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
  { orderId: 'O30', customerId: 'C2',  date: '21/04/2026', status: 'completed', entries: [{ id: 'P2', quantity: 1 }] },
  { orderId: 'O31', customerId: 'C3',  date: '21/04/2026', status: 'completed', entries: [{ id: 'P2', quantity: 1 }, { id: 'P1', quantity: 2 }] },
  { orderId: 'O32', customerId: 'C32', date: '21/04/2026', status: 'completed', entries: [{ id: 'P2', quantity: 1 }] },
  // O30 cancelled on 22nd
  { orderId: 'O30', customerId: 'C2',  date: '22/04/2026', status: 'cancelled' },
  { orderId: 'O40', customerId: 'C3',  date: '22/04/2026', status: 'completed', entries: [{ id: 'P4', quantity: 2 }] },
  { orderId: 'O60', customerId: 'C3',  date: '22/04/2026', status: 'completed', entries: [{ id: 'P4', quantity: 2 }, { id: 'P1', quantity: 2 }] },
  { orderId: 'O70', customerId: 'C4',  date: '22/04/2026', status: 'completed', entries: [{ id: 'P5', quantity: 2 }] },
  { orderId: 'O80', customerId: 'C5',  date: '22/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 2 }] },
  { orderId: 'O81', customerId: 'C5',  date: '22/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 10 }] },
  { orderId: 'O90', customerId: 'C5',  date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
  { orderId: 'O100',customerId: 'C3',  date: '23/04/2026', status: 'completed', entries: [{ id: 'P4', quantity: 1 }, { id: 'P6', quantity: 3 }] },
];

// ─── Expected outcomes from the brief ──────────────────────────────────────

describe('Expected outcomes (sample data)', () => {
  it('21/04/2026 → Ezy Storage Basket', () => {
    const result = calculateTopProduct(ORDERS, PRODUCTS, '21/04/2026', '21/04/2026');
    expect(result?.productName).toBe('Ezy Storage 37L Flexi Laundry Basket - White');
  });

  it('22/04/2026 → Ezy Storage Basket', () => {
    const result = calculateTopProduct(ORDERS, PRODUCTS, '22/04/2026', '22/04/2026');
    expect(result?.productName).toBe('Ezy Storage 37L Flexi Laundry Basket - White');
  });

  it('23/04/2026 → Arlec Solar Charging Kit', () => {
    const result = calculateTopProduct(ORDERS, PRODUCTS, '23/04/2026', '23/04/2026');
    expect(result?.productName).toBe('Arlec 160W Crystalline Solar Foldable Charging Kit');
  });

  it('21/04 – 23/04 period → Ezy Storage Basket', () => {
    const result = calculateTopProduct(ORDERS, PRODUCTS, '21/04/2026', '23/04/2026');
    expect(result?.productName).toBe('Ezy Storage 37L Flexi Laundry Basket - White');
  });
});

// ─── Business rule unit tests ───────────────────────────────────────────────

describe('BR1 – quantity is irrelevant', () => {
  it('order with quantity 99 counts as 1 sale', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 99 }] },
    ];
    const result = calculateTopProduct(orders, PRODUCTS, '23/04/2026', '23/04/2026');
    expect(result?.count).toBe(1);
  });
});

describe('BR2 – same customer same product same day = 1 sale', () => {
  it('two orders same customer same product same day = 1 sale', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
      { orderId: 'O2', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 5 }] },
    ];
    const result = calculateTopProduct(orders, PRODUCTS, '23/04/2026', '23/04/2026');
    expect(result?.count).toBe(1);
  });

  it('two different customers same product same day = 2 sales', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
      { orderId: 'O2', customerId: 'C2', date: '23/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
    ];
    const result = calculateTopProduct(orders, PRODUCTS, '23/04/2026', '23/04/2026');
    expect(result?.count).toBe(2);
  });
});

describe('BR3 – cancelled orders remove the original sale', () => {
  it('cancelled order removes the original completed sale', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '21/04/2026', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
      { orderId: 'O1', customerId: 'C1', date: '22/04/2026', status: 'cancelled' },
    ];
    const result = calculateTopProduct(orders, PRODUCTS, '21/04/2026', '23/04/2026');
    expect(result).toBeNull();
  });
});

describe('BR4 – alphabetical tie break', () => {
  it('on a tie, the alphabetically earlier product name wins', () => {
    const products: Product[] = [
      { id: 'PH', name: 'Hammer' },
      { id: 'PB', name: 'BBQ Tongs' },
    ];
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'PH', quantity: 1 }] },
      { orderId: 'O2', customerId: 'C2', date: '23/04/2026', status: 'completed', entries: [{ id: 'PB', quantity: 1 }] },
    ];
    const result = calculateTopProduct(orders, products, '23/04/2026', '23/04/2026');
    expect(result?.productName).toBe('BBQ Tongs');
  });
});

describe('Edge cases', () => {
  it('empty orders returns null', () => {
    expect(calculateTopProduct([], PRODUCTS, '23/04/2026', '23/04/2026')).toBeNull();
  });

  it('orders outside the window are ignored', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '01/01/2025', status: 'completed', entries: [{ id: 'P1', quantity: 1 }] },
    ];
    expect(calculateTopProduct(orders, PRODUCTS, '23/04/2026', '23/04/2026')).toBeNull();
  });

  it('unknown product IDs in orders are skipped gracefully', () => {
    const orders: Order[] = [
      { orderId: 'O1', customerId: 'C1', date: '23/04/2026', status: 'completed', entries: [{ id: 'UNKNOWN', quantity: 1 }] },
    ];
    expect(calculateTopProduct(orders, PRODUCTS, '23/04/2026', '23/04/2026')).toBeNull();
  });
});
