import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useTranslation } from '../hooks/useTranslation';
import { fetchExams } from '../store/slices/examSlice';
import recommendationService from '../services/recommendationService';
import { subscriptionService } from '../services/subscriptionService';
import { pricingService } from '../services/pricingService';
import { cartService } from '../services/cartService';
import { cscaStrings } from '../i18n/csca';
import { CSCA_SUBJECTS, CSCA_BOOK_PRICE } from '../cscaConfig';
import CscaNewsSection from '../components/csca/CscaNewsSection';
import MockShop from '../components/MockShop';
import type { Streak, Recommendation, DailySummary, DailyUsage } from '../types';

function DashboardPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { selectedExams, selectedSectionIds } = useAppSelector((state) => state.exam);
  const { t, locale } = useTranslation();

  const [streak, setStreak] = useState<Streak | null>(null);
  const [recs, setRecs] = useState<Recommendation[]>([]);
  const [yesterday, setYesterday] = useState<DailySummary | null>(null);
  const [usage, setUsage] = useState<DailyUsage | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [briefing, dailyUsage] = await Promise.all([
        recommendationService.getDailyBriefing(selectedSectionIds.length > 0 ? selectedSectionIds : undefined),
        subscriptionService.getDailyUsage().catch(() => null),
      ]);
      setStreak(briefing.streak);
      setRecs(briefing.recommendations.slice(0, 4));
      setYesterday(briefing.yesterdaySummary);
      setUsage(dailyUsage);
    } catch {
      setError(t.dashboard.loadError);
    } finally {
      setIsLoading(false);
    }
  }, [locale, selectedSectionIds]);

  useEffect(() => { dispatch(fetchExams()); load(); }, [dispatch, load]);

  // If user arrived here after clicking "Buy" on the landing (guest → register → dashboard),
  // pick up the pending checkout intent and redirect straight to /checkout.
  useEffect(() => {
    const pending = sessionStorage.getItem('checkout');
    if (pending) {
      try {
        const order = JSON.parse(pending);
        if (order?.itemCode) {
          // Clear BEFORE navigating to avoid infinite loop if user clicks "Back"
          sessionStorage.removeItem('checkout');
          navigate('/checkout', { state: order, replace: true });
        }
      } catch {
        sessionStorage.removeItem('checkout');
      }
    }
  // Only run once on mount
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const greeting = (() => {
    const h = new Date().getHours();
    if (h < 6) return t.dashboard.greetingNight;
    if (h < 12) return t.dashboard.greetingMorning;
    if (h < 18) return t.dashboard.greetingAfternoon;
    return t.dashboard.greetingEvening;
  })();

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      {/* ─── Header ─── */}
      <div style={{ marginBottom: '1.5rem', position: 'relative', overflow: 'hidden' }}>
        <span className="csca-app-hanzi" style={{ position: 'absolute', right: 0, top: '-1.4rem', fontSize: '5.5rem', zIndex: 0 }} aria-hidden="true">学</span>
        <h1 style={{ margin: 0, fontSize: '1.5rem', position: 'relative' }}>
          {greeting}, {user?.name?.split(' ')[0]}
        </h1>
        {selectedExams.length > 0 && (
          <div style={{ display: 'flex', gap: '0.4rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
            {selectedExams.map(code => (
              <span key={code} style={{
                padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.75rem',
                fontWeight: 600, background: 'rgba(200,16,46,0.1)', color: 'var(--primary-color)',
              }}>{code}</span>
            ))}
          </div>
        )}
      </div>

      {/* ─── Shop: run-based mocks (tiers + packages) ─── */}
      <MockShop />

      {/* ─── Materials (PDF textbooks) ─── */}
      <MaterialsSection />

      {/* ─── Error Banner ─── */}
      {error && (
        <div className="card" style={{
          padding: '0.75rem 1rem', marginBottom: '1rem',
          borderLeft: '3px solid var(--error-color)',
          display: 'flex', justifyContent: 'space-between', alignItems: 'center',
          background: 'var(--bg-secondary)',
        }}>
          <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{error}</span>
          <button className="btn btn-outline" style={{ padding: '0.25rem 0.75rem', fontSize: '0.8rem' }} onClick={load}>
            {t.dashboard.retry}
          </button>
        </div>
      )}

      {/* ─── Stats Row ─── */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '0.75rem', marginBottom: '1.5rem' }}>
        <StatCard
          icon=""
          label={t.dashboard.streak}
          value={streak ? `${streak.currentStreak} ${t.dashboard.daysShort}` : '—'}
          accent={streak && streak.currentStreak >= 7 ? 'var(--error-color)' : streak && streak.currentStreak >= 3 ? 'var(--warning-color)' : undefined}
          loading={isLoading}
        />
        <StatCard
          icon=""
          label={t.dashboard.today}
          value={usage ? `${usage.questionsAnswered}` : '—'}
          sub={usage ? (usage.questionsLimit === -1 ? t.dashboard.unlimited : `${t.dashboard.of} ${usage.questionsLimit}`) : ''}
          loading={isLoading}
        />
        {yesterday && (
          <StatCard
            icon=""
            label={t.dashboard.yesterday}
            value={`${yesterday.accuracy}%`}
            sub={`${yesterday.questionsAnswered} ${t.dashboard.questionsShort}`}
          />
        )}
        <StatCard
          icon=""
          label={t.dashboard.bestStreak}
          value={streak ? `${streak.longestStreak} ${t.dashboard.daysShort}` : '—'}
          loading={isLoading}
        />
      </div>

      {/* ─── Quick Actions ─── */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '0.75rem', marginBottom: '1.5rem' }}>
        <ActionCard icon="▶" title={t.dashboard.practice} desc={t.dashboard.practiceDesc} onClick={() => navigate('/learn')} primary />
        <ActionCard icon="" title={t.dashboard.mockExam} desc={t.dashboard.mockExamDesc} onClick={() => navigate('/learn?tab=mock')} />
        <ActionCard icon="" title={t.dashboard.review} desc={t.dashboard.reviewDesc} onClick={() => navigate('/learn?tab=review')} />
        <ActionCard icon="" title={t.dashboard.progress} desc={t.dashboard.progressDesc} onClick={() => navigate('/progress')} />
      </div>

      {/* ─── Recommendations ─── */}
      {recs.length > 0 && (
        <div style={{ marginBottom: '1.5rem' }}>
          <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{t.dashboard.recommendations}</h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {recs.map((r, i) => (
              <div
                key={i}
                className="card"
                style={{
                  padding: '0.75rem 1rem', display: 'flex', alignItems: 'center', gap: '0.75rem',
                  cursor: r.actionUrl ? 'pointer' : 'default',
                  borderLeft: `3px solid ${r.priority === 'high' ? 'var(--error-color)' : r.priority === 'medium' ? 'var(--warning-color)' : 'var(--primary-color)'}`,
                }}
                onClick={() => r.actionUrl && navigate(r.actionUrl)}
              >
                <span style={{ fontSize: '1.25rem' }}>{r.icon || ''}</span>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{r.title}</div>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{r.description}</div>
                </div>
                {r.actionLabel && (
                  <span style={{ fontSize: '0.75rem', color: 'var(--primary-color)', fontWeight: 600, whiteSpace: 'nowrap' }}>
                    {r.actionLabel} →
                  </span>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ─── No exams selected prompt ─── */}
      {selectedExams.length === 0 && (
        <div className="card" style={{ padding: '2rem', textAlign: 'center' }}>
          <h3 style={{ marginBottom: '0.5rem' }}>{t.dashboard.selectExams}</h3>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
            {t.dashboard.selectExamsHelp}
          </p>
          <button className="btn btn-primary" onClick={() => navigate('/profile')}>
            {t.dashboard.selectExamsBtn}
          </button>
        </div>
      )}

      {/* ─── News ─── */}
      <div style={{ marginTop: '2rem' }}>
        <NewsBlock />
      </div>
    </div>
  );
}

function NewsBlock() {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  return (
    <CscaNewsSection
      title={s.newsTitle}
      lead={s.newsLead}
      readMore={s.newsReadMore}
      readLess={s.newsReadLess}
      emptyText={s.newsEmpty}
      limit={6}
      appTheme
    />
  );
}

/* ─── Materials section (study textbooks) ─── */
function MaterialsSection() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const [bookPrice, setBookPrice] = useState<number>(CSCA_BOOK_PRICE);
  const [currency, setCurrency] = useState<string>('₸');
  useEffect(() => {
    pricingService.get().then((p) => {
      setBookPrice(p.materialPrice);
      setCurrency(p.currency);
    }).catch(() => {});
  }, []);

  const subjectName: Record<string, string> = {
    chineseTech: s.subjChineseTech,
    chineseHum: s.subjChineseHum,
    math: s.subjMath,
    physics: s.subjPhysics,
    chemistry: s.subjChemistry,
  };

  const buy = (subj: typeof CSCA_SUBJECTS[number]) => {
    cartService.add({
      itemType: 'book',
      itemCode: subj.key,
      title: `${subjectName[subj.key]} · ${s.bookLabel}`,
      subjects: subj.key,
      amount: bookPrice,
      currency,
    });
    navigate('/cart');
  };

  return (
    <div style={{ marginBottom: '1.75rem' }}>
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.materialsTitle}</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '0.75rem' }}>
        {CSCA_SUBJECTS.map((subj) => {
          return (
            <div key={subj.key} className="card csca-book" style={{ padding: '1.1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', background: 'var(--card-background)' }}>
              <div className="csca-book-cover" style={{ background: subj.cover, width: '100%', margin: '0 auto 0.5rem', maxWidth: '140px' }}>
                <span className="csca-book-hanzi" style={{ fontSize: '1.8rem' }}>{subj.hanzi}</span>
                <span className="csca-book-label csca-hanzi" style={{ fontSize: '0.55rem' }}>CSCA · 备考教材</span>
              </div>
              <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{subjectName[subj.key]}</div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>{s.bookLabel}</div>
              <div style={{ marginTop: 'auto', display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                <div style={{ fontWeight: 800, fontSize: '1.1rem', color: 'var(--primary-color)', textAlign: 'center' }}>
                  {bookPrice.toLocaleString('ru-RU')} {currency}
                </div>
                <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => buy(subj)}>
                  {s.addToCart}
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

/* ─── Sub-components ─── */

function StatCard({ icon, label, value, sub, accent, loading }: {
  icon: string; label: string; value: string; sub?: string; accent?: string; loading?: boolean;
}) {
  return (
    <div className="card" style={{ padding: '1rem', textAlign: 'center' }}>
      <div style={{ fontSize: '1.5rem', marginBottom: '0.25rem' }}>{icon}</div>
      {loading ? (
        <div style={{ height: '1.5rem', background: 'var(--bg-secondary)', borderRadius: '4px', margin: '0.25rem auto', width: '60%' }} />
      ) : (
        <div style={{ fontSize: '1.25rem', fontWeight: 700, color: accent || 'var(--text-primary)' }}>{value}</div>
      )}
      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{label}</div>
      {sub && <div style={{ fontSize: '0.7rem', color: 'var(--text-secondary)' }}>{sub}</div>}
    </div>
  );
}

function ActionCard({ icon, title, desc, onClick, primary }: {
  icon: string; title: string; desc: string; onClick: () => void; primary?: boolean;
}) {
  return (
    <div
      className="card"
      onClick={onClick}
      style={{
        padding: '1.25rem', cursor: 'pointer', transition: 'all 0.2s',
        border: primary ? '2px solid var(--primary-color)' : undefined,
      }}
      onMouseEnter={e => { e.currentTarget.style.transform = 'translateY(-2px)'; e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.1)'; }}
      onMouseLeave={e => { e.currentTarget.style.transform = ''; e.currentTarget.style.boxShadow = ''; }}
    >
      <div style={{ fontSize: '1.75rem', marginBottom: '0.5rem' }}>{icon}</div>
      <div style={{ fontWeight: 600, marginBottom: '0.15rem' }}>{title}</div>
      <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{desc}</div>
    </div>
  );
}

export default DashboardPage;
