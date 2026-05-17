import styles from './LoadingSkeleton.module.css';

export function LoadingSkeleton() {
  return (
    <div aria-busy="true" aria-label="Loading sizzling hot products">
      {/* Banner skeleton */}
      <div className={`${styles.skeleton} ${styles.banner}`} />
      {/* Card skeletons */}
      {[1, 2, 3].map((i) => (
        <div key={i} className={`${styles.skeleton} ${styles.card}`} />
      ))}
    </div>
  );
}
