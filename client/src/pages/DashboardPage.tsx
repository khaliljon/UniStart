import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { cartService } from '../services/cartService';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';
import { cscaStrings } from '../i18n/csca';
import { CSCA_SUBJECTS } from '../cscaConfig';
import { pickLocalized } from '../utils/localize';
import CscaNewsSection from '../components/csca/CscaNewsSection';
import MockShop from '../components/MockShop';

function DashboardPage() {
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { selectedExams } = useAppSelector((state) => state.exam);
  const { t } = useTranslation();

  // Guest → register → dashboard: pick up pending book intent and add it to the cart.
  useEffect(() => {
    const pending = sessionStorage.getItem('checkout');
    if (pending) {
      try {
        const item = JSON.parse(pending);
        sessionStorage.removeItem('checkout');
        if (item?.itemCode && item?.itemType === 'book') {
          cartService.add(item);
          navigate('/cart', { replace: true });
        }
      } catch {
        sessionStorage.removeItem('checkout');
      }
    }
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

  const [materials, setMaterials] = useState<StudyMaterial[] | null>(null);
  const [ownedBooks, setOwnedBooks] = useState<Set<string>>(new Set());
  const cardRefs = useRef<Record<string, HTMLDivElement | null>>({});
  const [highlight, setHighlight] = useState<string | null>(null);
  useEffect(() => {
    materialsService.list().then(setMaterials).catch(() => setMaterials([]));
    purchaseService.list()
      .then((ps) => setOwnedBooks(new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode))))
      .catch(() => {});
  }, []);

  // If the user clicked "Buy" on a book on the landing, scroll to it here and
  // highlight it briefly (instead of adding straight to the cart).
  useEffect(() => {
    if (!materials || materials.length === 0) return;
    const id = sessionStorage.getItem('focusBook');
    if (!id) return;
    sessionStorage.removeItem('focusBook');
    setTimeout(() => cardRefs.current[id]?.scrollIntoView({ behavior: 'smooth', block: 'center' }), 150);
    setHighlight(id);
    const t = setTimeout(() => setHighlight(null), 2400);
    return () => clearTimeout(t);
  }, [materials]);

  const coverFor = (key: string) => CSCA_SUBJECTS.find((x) => x.key === key);

  const buy = (m: StudyMaterial) => {
    cartService.add({
      itemType: 'book',
      itemCode: String(m.id),
      title: pickLocalized(m.title, m.titleKz, m.titleEn, locale),
      subjects: m.subjectKey,
      amount: m.price,
      currency: '₸',
    });
    navigate('/cart');
  };

  if (materials === null) return null; // loading

  if (materials.length === 0) {
    return (
      <div style={{ marginBottom: '1.75rem' }}>
        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.materialsTitle}</h2>
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
          Учебные материалы скоро появятся — мы работаем над этим.
        </div>
      </div>
    );
  }

  return (
    <div style={{ marginBottom: '1.75rem' }}>
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.materialsTitle}</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '0.75rem' }}>
        {materials.map((m) => {
          const cover = coverFor(m.subjectKey);
          return (
            <div key={m.id} ref={(el) => { cardRefs.current[String(m.id)] = el; }} className="card csca-book" style={{ padding: '1.1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', background: 'var(--card-background)', outline: highlight === String(m.id) ? '2px solid var(--primary-color)' : 'none', outlineOffset: 2, transition: 'outline-color 0.3s' }}>
              {cover && (
                <div className="csca-book-cover" style={{ background: cover.cover, width: '100%', margin: '0 auto 0.5rem', maxWidth: '140px' }}>
                  <span className="csca-book-hanzi" style={{ fontSize: '1.8rem' }}>{cover.hanzi}</span>
                  <span className="csca-book-label csca-hanzi" style={{ fontSize: '0.55rem' }}>CSCA · 备考教材</span>
                </div>
              )}
              <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{pickLocalized(m.title, m.titleKz, m.titleEn, locale)}</div>
              {pickLocalized(m.description ?? '', m.descriptionKz, m.descriptionEn, locale) && (
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>{pickLocalized(m.description ?? '', m.descriptionKz, m.descriptionEn, locale)}</div>
              )}
              <div style={{ marginTop: 'auto', display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                <div style={{ fontWeight: 800, fontSize: '1.1rem', color: 'var(--primary-color)', textAlign: 'center' }}>
                  {m.price.toLocaleString('ru-RU')} ₸
                </div>
                {ownedBooks.has(String(m.id)) ? (
                  <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => navigate('/materials')}>
                    {s.boughtOpen}
                  </button>
                ) : (
                  <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => buy(m)}>
                    {s.addToCart}
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default DashboardPage;
