import { useState } from 'react';
import { useSizzlingHotProducts } from '../hooks/useSizzlingHotProducts';
import { DailyResultCard } from './DailyResultCard';
import { PeriodWinnerBanner } from './PeriodWinnerBanner';
import { LoadingSkeleton } from './LoadingSkeleton';
import { ErrorMessage } from './ErrorMessage';
import styles from './SizzlingDashboard.module.css';

/**
 * Top-level dashboard page.
 * Renders the 3-day sizzling hot products view:
 *  - Overall period winner (banner)
 *  - Daily breakdown (cards)
 * Supports an optional date override for testing/demo purposes.
 */
export function SizzlingDashboard() {
  // Allow the user to override "today" for demo/testing
  const [dateOverride, setDateOverride] = useState('');
  const [appliedDate, setAppliedDate] = useState<string | undefined>(undefined);

  const { data, isLoading, isError, error, refetch } = useSizzlingHotProducts(appliedDate);

  const handleApplyDate = () => {
    const trimmed = dateOverride.trim();
    setAppliedDate(trimmed || undefined);
  };

  const handleReset = () => {
    setDateOverride('');
    setAppliedDate(undefined);
  };

  const errorMessage =
    error instanceof Error ? error.message : 'Failed to load sizzling hot products.';

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <div className={styles.logo}>
            <span className={styles.logoIcon}>🔥</span>
            <span className={styles.logoText}>Sizzling Hot Products</span>
          </div>
          <p className={styles.tagline}>Top-selling products over the past 3 days</p>
        </div>
      </header>

      <main className={styles.main}>
        {/* Date override control */}
        <section className={styles.controls} aria-label="Date controls">
          <label htmlFor="date-input" className={styles.controlLabel}>
            Override "today" (dd/MM/yyyy):
          </label>
          <div className={styles.controlRow}>
            <input
              id="date-input"
              type="text"
              className={styles.dateInput}
              placeholder="e.g. 23/04/2026"
              value={dateOverride}
              onChange={(e) => setDateOverride(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleApplyDate()}
            />
            <button className={styles.btnPrimary} onClick={handleApplyDate}>
              Apply
            </button>
            {appliedDate && (
              <button className={styles.btnSecondary} onClick={handleReset}>
                Reset
              </button>
            )}
          </div>
          {appliedDate && (
            <p className={styles.controlNote}>
              Showing results as of: <strong>{appliedDate}</strong>
            </p>
          )}
        </section>

        {/* Content */}
        {isLoading && <LoadingSkeleton />}

        {isError && (
          <ErrorMessage message={errorMessage} onRetry={() => refetch()} />
        )}

        {data && (
          <>
            {data.threeDayResult?.productName ? (
              <PeriodWinnerBanner result={data.threeDayResult} />
            ) : null}

            <section aria-label="Daily results">
              <h2 className={styles.sectionTitle}>Daily Breakdown</h2>
              {data.dailyResults.length === 0 ? (
                <p className={styles.empty}>No results found for this period.</p>
              ) : (
                <ul className={styles.cardList}>
                  {data.dailyResults.map((result, index) => (
                    <li key={result.date}>
                      <DailyResultCard result={result} rank={index + 1} />
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
        )}
      </main>
    </div>
  );
}
