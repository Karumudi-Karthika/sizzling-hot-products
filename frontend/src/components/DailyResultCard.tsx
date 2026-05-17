import { DailySizzlingResult } from '../types';
import styles from './DailyResultCard.module.css';

interface Props {
  result: DailySizzlingResult;
  rank: number;
}

/**
 * Displays the top sizzling product for a single day.
 */
export function DailyResultCard({ result, rank }: Props) {
  return (
    <article className={styles.card} aria-label={`Result for ${result.date}`}>
      <div className={styles.rank}>#{rank}</div>
      <div className={styles.body}>
        <p className={styles.date}>{result.date}</p>
        <h3 className={styles.productName}>{result.productName}</h3>
        <p className={styles.meta}>
          <span className={styles.badge}>{result.saleCount} unique sale{result.saleCount !== 1 ? 's' : ''}</span>
          <span className={styles.productId}>ID: {result.productId}</span>
        </p>
      </div>
    </article>
  );
}
