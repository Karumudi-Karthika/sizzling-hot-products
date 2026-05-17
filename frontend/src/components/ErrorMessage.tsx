import styles from './ErrorMessage.module.css';

interface Props {
  message?: string;
  onRetry?: () => void;
}

export function ErrorMessage({ message = 'Something went wrong.', onRetry }: Props) {
  return (
    <div className={styles.error} role="alert">
      <span className={styles.icon}>⚠️</span>
      <div>
        <p className={styles.text}>{message}</p>
        {onRetry && (
          <button className={styles.retry} onClick={onRetry}>
            Try again
          </button>
        )}
      </div>
    </div>
  );
}
