interface SkeletonProps {
  className?: string;
  width?: string | number;
  height?: string | number;
  circle?: boolean;
  count?: number;
}

export function Skeleton({ 
  className = '', 
  width, 
  height, 
  circle = false,
  count = 1 
}: SkeletonProps) {
  const style = {
    width: typeof width === 'number' ? `${width}px` : width,
    height: typeof height === 'number' ? `${height}px` : height,
  };

  const skeletons = Array.from({ length: count }, (_, i) => (
    <div
      key={i}
      className={`skeleton ${circle ? 'skeleton-circle' : ''} ${className}`}
      style={style}
    />
  ));

  return <>{skeletons}</>;
}

// Common skeleton patterns
export function QuestionSkeleton() {
  return (
    <div className="card animate-fade-in">
      <div className="skeleton skeleton-text" style={{ width: '30%', marginBottom: '1rem' }} />
      <div className="skeleton skeleton-text" style={{ width: '100%' }} />
      <div className="skeleton skeleton-text" style={{ width: '80%', marginBottom: '1.5rem' }} />
      
      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
        {[1, 2, 3, 4].map(i => (
          <div key={i} className="skeleton" style={{ height: '3rem', width: '100%' }} />
        ))}
      </div>
    </div>
  );
}

export function TopicsSkeleton() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      {[1, 2, 3, 4].map(i => (
        <div key={i} className="card animate-fade-in" style={{ animationDelay: `${i * 0.1}s` }}>
          <div className="skeleton skeleton-text" style={{ width: '40%' }} />
          <div className="skeleton skeleton-text-sm" style={{ marginTop: '0.5rem' }} />
          <div className="skeleton" style={{ height: '0.5rem', marginTop: '1rem' }} />
        </div>
      ))}
    </div>
  );
}

export function AnalyticsSkeleton() {
  return (
    <div className="animate-fade-in">
      {/* Stats grid */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', marginBottom: '2rem' }}>
        {[1, 2, 3].map(i => (
          <div key={i} className="card">
            <div className="skeleton skeleton-text" style={{ width: '50%' }} />
            <div className="skeleton" style={{ height: '2rem', width: '40%', marginTop: '0.5rem' }} />
          </div>
        ))}
      </div>
      
      {/* Skills list */}
      <div className="card">
        <div className="skeleton skeleton-text" style={{ width: '30%', marginBottom: '1rem' }} />
        {[1, 2, 3].map(i => (
          <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
            <div className="skeleton" style={{ width: '80px', height: '1rem' }} />
            <div className="skeleton" style={{ flex: 1, height: '0.75rem' }} />
            <div className="skeleton" style={{ width: '40px', height: '1rem' }} />
          </div>
        ))}
      </div>
    </div>
  );
}
