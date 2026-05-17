import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { DailyResultCard } from '../components/DailyResultCard';
import { PeriodWinnerBanner } from '../components/PeriodWinnerBanner';
import { ErrorMessage } from '../components/ErrorMessage';
import { LoadingSkeleton } from '../components/LoadingSkeleton';
import type { DailySizzlingResult, PeriodSizzlingResult } from '../types';

// ─── Fixtures ──────────────────────────────────────────────────────────────

const mockDailyResult: DailySizzlingResult = {
  date: '21/04/2026',
  productId: 'P1',
  productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
  saleCount: 3,
};

const mockPeriodResult: PeriodSizzlingResult = {
  periodStart: '21/04/2026',
  periodEnd: '23/04/2026',
  productId: 'P1',
  productName: 'Ezy Storage 37L Flexi Laundry Basket - White',
  saleCount: 5,
};

// ─── DailyResultCard ────────────────────────────────────────────────────────

describe('DailyResultCard', () => {
  it('renders the product name', () => {
    render(<DailyResultCard result={mockDailyResult} rank={1} />);
    expect(screen.getByText('Ezy Storage 37L Flexi Laundry Basket - White')).toBeInTheDocument();
  });

  it('renders the date', () => {
    render(<DailyResultCard result={mockDailyResult} rank={1} />);
    expect(screen.getByText('21/04/2026')).toBeInTheDocument();
  });

  it('renders the rank', () => {
    render(<DailyResultCard result={mockDailyResult} rank={2} />);
    expect(screen.getByText('#2')).toBeInTheDocument();
  });

  it('renders sale count with correct plural', () => {
    render(<DailyResultCard result={mockDailyResult} rank={1} />);
    expect(screen.getByText(/3 unique sales/i)).toBeInTheDocument();
  });

  it('renders singular "sale" for count of 1', () => {
    const result = { ...mockDailyResult, saleCount: 1 };
    render(<DailyResultCard result={result} rank={1} />);
    expect(screen.getByText(/1 unique sale$/i)).toBeInTheDocument();
  });

  it('has accessible article label', () => {
    render(<DailyResultCard result={mockDailyResult} rank={1} />);
    expect(screen.getByRole('article', { name: /Result for 21\/04\/2026/i })).toBeInTheDocument();
  });
});

// ─── PeriodWinnerBanner ─────────────────────────────────────────────────────

describe('PeriodWinnerBanner', () => {
  it('renders the product name', () => {
    render(<PeriodWinnerBanner result={mockPeriodResult} />);
    expect(screen.getByText('Ezy Storage 37L Flexi Laundry Basket - White')).toBeInTheDocument();
  });

  it('renders the period start and end dates', () => {
    render(<PeriodWinnerBanner result={mockPeriodResult} />);
    expect(screen.getByText(/21\/04\/2026 to 23\/04\/2026/i)).toBeInTheDocument();
  });

  it('renders sale count', () => {
    render(<PeriodWinnerBanner result={mockPeriodResult} />);
    expect(screen.getByText(/5 unique sales/i)).toBeInTheDocument();
  });

  it('has accessible section label', () => {
    render(<PeriodWinnerBanner result={mockPeriodResult} />);
    expect(screen.getByRole('region', { name: /3-day period winner/i })).toBeInTheDocument();
  });
});

// ─── ErrorMessage ──────────────────────────────────────────────────────────

describe('ErrorMessage', () => {
  it('renders default message when none provided', () => {
    render(<ErrorMessage />);
    expect(screen.getByText('Something went wrong.')).toBeInTheDocument();
  });

  it('renders a custom error message', () => {
    render(<ErrorMessage message="Network error" />);
    expect(screen.getByText('Network error')).toBeInTheDocument();
  });

  it('shows retry button when onRetry provided', () => {
    render(<ErrorMessage onRetry={() => {}} />);
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
  });

  it('calls onRetry when retry button clicked', async () => {
    const onRetry = vi.fn();
    render(<ErrorMessage onRetry={onRetry} />);
    await userEvent.click(screen.getByRole('button', { name: /try again/i }));
    expect(onRetry).toHaveBeenCalledOnce();
  });

  it('does not show retry button when onRetry not provided', () => {
    render(<ErrorMessage />);
    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  it('has alert role for accessibility', () => {
    render(<ErrorMessage />);
    expect(screen.getByRole('alert')).toBeInTheDocument();
  });
});

// ─── LoadingSkeleton ────────────────────────────────────────────────────────

describe('LoadingSkeleton', () => {
  it('renders with aria-busy true', () => {
    render(<LoadingSkeleton />);
    expect(screen.getByLabelText(/loading sizzling hot products/i)).toHaveAttribute('aria-busy', 'true');
  });
});
