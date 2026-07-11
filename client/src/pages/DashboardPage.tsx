import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useTranslation } from '../hooks/useTranslation';
import { fetchExams } from '../store/slices/examSlice';
import recommendationService from '../services/recommendationService';
import { subscriptionService } from '../services/subscriptionService';
import { materialsService } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';
import { cscaStrings } from '../i18n/csca';
import { CSCA_PACKAGES, CSCA_SUBJECTS, CSCA_BOOK_PRICE } from '../cscaConfig';
import CscaNewsSection from '../components/csca/CscaNewsSection';
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
          navigate('/checkout', { state: order, replace: true });
        }
      } catch { /* malformed – ignore */ }
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

      {/* ─── Order / products ─── */}
      <OrderSection />

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

/* ─── Order section (mock-exam packages) ─── */
function OrderSection() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [openKey, setOpenKey] = useState<string | null>(null);
  const [subs, setSubs] = useState<string[]>([]);

  const pkgName: Record<string, string> = {
    start: s.pkgStart, standard: s.pkgStandard, advanced: s.pkgAdvanced, full: s.pkgFull,
  };
  const pkgFor: Record<string, string> = {
    start: s.pkgStartFor, standard: s.pkgStandardFor, advanced: s.pkgAdvancedFor, full: s.pkgFullFor,
  };
  const subjName: Record<string, string> = {
    chineseTech: s.subjChineseTech, chineseHum: s.subjChineseHum,
    math: s.subjMath, physics: s.subjPhysics, chemistry: s.subjChemistry,
  };
  const allSubjectKeys = CSCA_SUBJECTS.map((x) => x.key);

  const select = (key: string) => {
    setOpenKey((prev) => (prev === key ? null : key));
    setSubs([]);
  };

  const toggleSub = (key: string, allowance: number) => {
    setSubs((prev) => {
      if (prev.includes(key)) return prev.filter((x) => x !== key);
      if (prev.length >= allowance) return [...prev.slice(1), key];
      return [...prev, key];
    });
  };

  const pay = (pkgKey: string) => {
    const pkg = CSCA_PACKAGES.find((p) => p.key === pkgKey)!;
    const chosen = pkg.subjects === 'all'
      ? allSubjectKeys.map((k) => subjName[k])
      : subs.map((k) => subjName[k]);
    navigate('/checkout', {
      state: {
        itemType: 'package',
        itemCode: pkg.key,
        title: `${pkgName[pkg.key]} · CSCA`,
        subjects: chosen.join(','),
        amount: pkg.price,
        currency: '₸',
      },
    });
  };

  return (
    <div style={{ marginBottom: '1.75rem' }}>
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.mocksTitle}</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))', gap: '0.75rem' }}>
        {CSCA_PACKAGES.map((pkg) => {
          const isOpen = openKey === pkg.key;
          const allowance = pkg.subjects === 'all' ? allSubjectKeys.length : pkg.subjects;
          const needsChoice = pkg.subjects !== 'all';
          const canPay = !needsChoice || subs.length === allowance;
          const subjCountLabel = pkg.subjects === 'all' ? s.allSubjects
            : pkg.subjects === 1 ? s.oneSubject
            : pkg.subjects === 2 ? s.twoSubjects
            : s.threeSubjects;
          return (
            <div key={pkg.key} className="card" style={{ padding: '1.1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', border: pkg.featured ? '2px solid var(--primary-color)' : undefined }}>
              <div style={{ fontWeight: 700, fontSize: '1.05rem' }}>{pkgName[pkg.key]}</div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{pkgFor[pkg.key]}</div>
              <div style={{ fontWeight: 800, fontSize: '1.3rem', color: 'var(--primary-color)' }}>
                {pkg.price.toLocaleString('ru-RU')} ₸
              </div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                {subjCountLabel}
              </div>

              {!isOpen ? (
                <button className={pkg.featured ? 'btn btn-primary' : 'btn btn-outline'} style={{ marginTop: 'auto' }} onClick={() => select(pkg.key)}>
                  {s.buy}
                </button>
              ) : (
                <div style={{ marginTop: '0.25rem' }}>
                  {needsChoice && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', marginBottom: '0.6rem' }}>
                      <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                        {subjCountLabel}:
                      </div>
                      {allSubjectKeys.map((k) => (
                        <label key={k} style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.85rem' }}>
                          <input
                            type="checkbox"
                            checked={subs.includes(k)}
                            onChange={() => toggleSub(k, allowance)}
                          />
                          {subjName[k]}
                        </label>
                      ))}
                    </div>
                  )}
                  <div style={{ display: 'flex', gap: '0.4rem' }}>
                    <button className="btn btn-primary" disabled={!canPay} onClick={() => pay(pkg.key)}>{s.checkoutPay}</button>
                    <button className="btn btn-outline" onClick={() => setOpenKey(null)}>×</button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

/* ─── Materials section (study textbooks) ─── */
function MaterialsSection() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [purchasedBooks, setPurchasedBooks] = useState<string[]>([]);
  const [downloading, setDownloading] = useState<string | null>(null);

  const subjectName: Record<string, string> = {
    chineseTech: s.subjChineseTech,
    chineseHum: s.subjChineseHum,
    math: s.subjMath,
    physics: s.subjPhysics,
    chemistry: s.subjChemistry,
  };

  useEffect(() => {
    purchaseService.list()
      .then((items) => {
        const books = items
          .filter((p) => p.itemType === 'book' && p.status === 'Paid')
          .map((p) => p.itemCode);
        setPurchasedBooks(books);
      })
      .catch(() => {});
  }, []);

  const buy = (subj: typeof CSCA_SUBJECTS[number]) => {
    navigate('/checkout', {
      state: {
        itemType: 'book',
        itemCode: subj.key,
        title: `${subjectName[subj.key]} · ${s.bookLabel}`,
        amount: CSCA_BOOK_PRICE,
        currency: '₸',
      },
    });
  };

  const download = async (subjectKey: string) => {
    setDownloading(subjectKey);
    try {
      const res = await materialsService.download(subjectKey);
      window.open(res.pdfUrl, '_blank');
    } catch {
      alert(locale === 'en'
        ? 'Could not load PDF link. Please check your purchase.'
        : locale === 'kz'
        ? 'PDF сілтемесін жүктеу мүмкін болмады.'
        : 'Не удалось получить ссылку на PDF. Проверьте покупку или обратитесь в поддержку.');
    } finally {
      setDownloading(null);
    }
  };

  const isEn = locale === 'en';
  const isKz = locale === 'kz';
  const downloadText = isEn ? 'Download PDF' : isKz ? 'PDF жүктеу' : 'Скачать PDF';

  return (
    <div style={{ marginBottom: '1.75rem' }}>
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.materialsTitle}</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '0.75rem' }}>
        {CSCA_SUBJECTS.map((subj) => {
          const isPurchased = purchasedBooks.includes(subj.key);
          const isLd = downloading === subj.key;
          return (
            <div key={subj.key} className="card csca-book" style={{ padding: '1.1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', background: 'var(--card-background)' }}>
              <div className="csca-book-cover" style={{ background: subj.cover, width: '100%', margin: '0 auto 0.5rem', maxWidth: '140px' }}>
                <span className="csca-book-hanzi" style={{ fontSize: '1.8rem' }}>{subj.hanzi}</span>
                <span className="csca-book-label csca-hanzi" style={{ fontSize: '0.55rem' }}>CSCA · 备考教材</span>
              </div>
              <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{subjectName[subj.key]}</div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>{s.bookLabel}</div>

              {isPurchased ? (
                <button
                  className="btn btn-primary"
                  style={{ marginTop: 'auto', fontSize: '0.85rem' }}
                  disabled={isLd}
                  onClick={() => download(subj.key)}
                >
                  {isLd ? '…' : downloadText}
                </button>
              ) : (
                <div style={{ marginTop: 'auto', display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                  <div style={{ fontWeight: 800, fontSize: '1.1rem', color: 'var(--primary-color)', textAlign: 'center' }}>
                    {CSCA_BOOK_PRICE.toLocaleString('ru-RU')} ₸
                  </div>
                  <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => buy(subj)}>
                    {s.buy}
                  </button>
                </div>
              )}
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
