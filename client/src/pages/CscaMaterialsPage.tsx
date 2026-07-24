import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { useAppSelector } from '../hooks/useAppSelector';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';
import { cartService } from '../services/cartService';
import { CSCA_SUBJECTS } from '../cscaConfig';
import { pickLocalized } from '../utils/localize';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';

function CscaMaterialsPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const { isAuthenticated } = useAppSelector((st) => st.auth);
  const [materials, setMaterials] = useState<StudyMaterial[] | null>(null);
  const [ownedBooks, setOwnedBooks] = useState<Set<string>>(new Set());

  useEffect(() => {
    materialsService.list().then(setMaterials).catch(() => setMaterials([]));
    if (isAuthenticated) {
      purchaseService.list()
        .then((ps) => setOwnedBooks(new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode))))
        .catch(() => {});
    }
  }, [isAuthenticated]);

  const coverFor = (key: string) => CSCA_SUBJECTS.find((x) => x.key === key) ?? CSCA_SUBJECTS[0];

  const buy = (mat: StudyMaterial) => {
    const item = {
      itemType: 'book',
      itemCode: String(mat.id),
      title: pickLocalized(mat.title, mat.titleKz, mat.titleEn, locale),
      subjects: mat.subjectKey,
      amount: mat.price,
      currency: '₸',
    };
    if (isAuthenticated) {
      cartService.add(item);
      navigate('/cart');
      return;
    }
    sessionStorage.setItem('checkout', JSON.stringify(item));
    navigate('/register');
  };

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navMaterials} title={s.materialsTitle} lead={s.materialsLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        {materials === null ? (
          <div className="loading"><div className="spinner" /></div>
        ) : materials.length === 0 ? (
          <div className="csca-card" style={{ textAlign: 'center' }}>
            <p className="csca-lead" style={{ margin: 0 }}>Учебные материалы скоро появятся — мы работаем над этим.</p>
          </div>
        ) : (
        <div className="csca-grid csca-grid-5">
          {materials.map((mat) => {
            const subj = coverFor(mat.subjectKey);
            return (
              <div className="csca-card csca-book" key={mat.id}>
                <div className="csca-book-cover" style={{ background: subj.cover }}>
                  <span className="csca-book-hanzi">{subj.hanzi}</span>
                  <span className="csca-book-label csca-hanzi">CSCA · 备考教材</span>
                </div>
                <div className="csca-subject-name">{pickLocalized(mat.title, mat.titleKz, mat.titleEn, locale)}</div>
                <div className="csca-subject-tag" style={{ marginBottom: '0.75rem' }}>{s.bookLabel}</div>
                <div className="csca-price-amount csca-hanzi" style={{ fontSize: '1.4rem', margin: '0 0 0.6rem' }}>
                  {mat.price.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
                </div>
                {ownedBooks.has(String(mat.id)) ? (
                  <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ width: '100%' }} onClick={() => navigate('/materials')}>{s.bought}</button>
                ) : (
                  <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ width: '100%' }} onClick={() => buy(mat)}>{s.buy}</button>
                )}
              </div>
            );
          })}
        </div>
        )}
      </section>
    </CscaPageShell>
  );
}

export default CscaMaterialsPage;
