import styles from './PeriodWinnerBanner.module.css';

interface PeriodSizzlingResult {
  periodStart: string;
  periodEnd: string;
  productId: string;
  productName: string;
  saleCount: number;
}

interface Props {
  result: PeriodSizzlingResult;
}

export function PeriodWinnerBanner({ result }: Props) {
  return (
    <section className={styles.banner} aria-label="3-day period winner">
      <div className={styles.flame}>🔥</div>
      <div className={styles.content}>
        <p className={styles.label}>Sizzling Hot — {result.periodStart} to {result.periodEnd}</p>
        <h2 className={styles.productName}>{result.productName}</h2>
        <p className={styles.subtext}>
          {result.saleCount} unique sale{result.saleCount !== 1 ? 's' : ''} across the period
          <span className={styles.divider}>·</span>
          <span className={styles.productId}>{result.productId}</span>
        </p>
      </div>
    </section>
  );
}
