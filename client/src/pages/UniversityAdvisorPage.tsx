import { useMemo, useRef, useState } from 'react';
import AdvisorWorldMap from '../components/AdvisorWorldMap';
import { AdvisorCategory, AdvisorUniversity, loadAdvisorConfig } from '../advisorConfig';

function normalizeText(text: string | null | undefined): string {
  return (text ?? '').trim().toLowerCase();
}

function matchesSpecialty(
  university: AdvisorUniversity,
  selectedCategories: string[],
  selectedSpecialties: string[],
  categories: AdvisorCategory[],
): boolean {
  if (selectedCategories.length === 0 && selectedSpecialties.length === 0) {
    return true;
  }
  const activeSpecialties =
    selectedSpecialties.length > 0
      ? selectedSpecialties
      : selectedCategories.flatMap(
          (categoryKey) =>
            categories.find((c) => c.key === categoryKey)?.specialties ?? [],
        );
  const normUni = university.specialties.map(normalizeText);
  const normSel = activeSpecialties.map(normalizeText);
  return normSel.some((sel) =>
    normUni.some((sp) => sp.includes(sel) || sel.includes(sp)),
  );
}

function matchesCountry(
  university: AdvisorUniversity,
  selectedCountries: string[],
  exam: 'SAT' | 'NUET' | '',
): boolean {
  if (selectedCountries.length === 0) return true;
  const normCountry = normalizeText(university.country);
  return selectedCountries.some((selected) => {
    const val = normalizeText(selected);
    if (exam === 'NUET' && !val.includes('kazakh') && !val.includes('kz')) {
      return false;
    }
    return normCountry.includes(val) || val.includes(normCountry);
  });
}

function matchesLanguage(
  university: AdvisorUniversity,
  selectedLanguages: string[],
): boolean {
  if (selectedLanguages.length === 0) return true;
  const normLang = normalizeText(university.language);
  return selectedLanguages.some((lang) => {
    const sel = normalizeText(lang);
    if (sel === 'other') {
      return !normLang.includes('english') && !normLang.includes('japanese');
    }
    return normLang.includes(sel);
  });
}

function getSelectedValues(event: React.ChangeEvent<HTMLSelectElement>) {
  return Array.from(event.target.selectedOptions).map((o) => o.value);
}

function UniCard({
  uni,
  highlighted,
  refProp,
}: {
  uni: AdvisorUniversity;
  highlighted: boolean;
  refProp?: React.RefObject<HTMLDivElement | null>;
}) {
  return (
    <div
      ref={refProp}
      className="card"
      style={{
        marginBottom: '1rem',
        outline: highlighted ? '2px solid var(--accent-color)' : undefined,
        transition: 'outline 0.2s',
      }}
    >
      <h3 style={{ margin: '0 0 0.5rem' }}>
        {uni.name} — {uni.city}
      </h3>
      <div style={{ display: 'grid', gap: '0.35rem', fontSize: '0.9rem' }}>
        <p style={{ margin: 0 }}><strong>Страна:</strong> {uni.country}</p>
        <p style={{ margin: 0 }}><strong>Специальности:</strong> {uni.specialties.join(', ')}</p>
        <p style={{ margin: 0 }}><strong>Язык:</strong> {uni.language}</p>
        {uni.minScore != null && (
          <p style={{ margin: 0 }}><strong>Мин. балл ({uni.exam}):</strong> {uni.minScore}</p>
        )}
        {uni.grants && (
          <p style={{ margin: 0 }}><strong>Гранты:</strong> {uni.grants}</p>
        )}
        {uni.plus && (
          <p style={{ margin: 0, color: 'var(--success-color)' }}>+ {uni.plus}</p>
        )}
        {uni.minus && (
          <p style={{ margin: 0, color: 'var(--error-color)' }}>− {uni.minus}</p>
        )}
        {uni.procedure && (
          <p style={{ margin: 0 }}><strong>Подача:</strong> {uni.procedure}</p>
        )}
        {uni.deadlines && (
          <p style={{ margin: 0, color: 'var(--text-secondary)' }}><strong>Сроки:</strong> {uni.deadlines}</p>
        )}
      </div>
    </div>
  );
}

function UniversityAdvisorPage() {
  const config = useMemo(() => loadAdvisorConfig(), []);

  const [exam, setExam] = useState<'SAT' | 'NUET' | ''>('');
  const [currentScore, setCurrentScore] = useState('');
  const [goalScore, setGoalScore] = useState('');
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const [selectedSpecialties, setSelectedSpecialties] = useState<string[]>([]);
  const [selectedCountries, setSelectedCountries] = useState<string[]>([]);
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>([]);
  const [grant, setGrant] = useState<'any' | 'yes' | 'no'>('any');
  const [difficulty, setDifficulty] = useState('Любой');
  const [submitted, setSubmitted] = useState(false);
  const [markerFocusId, setMarkerFocusId] = useState<string | null>(null);
  const cardRefs = useRef<Record<string, React.RefObject<HTMLDivElement | null>>>({});

  const specialtyOptions = useMemo(() => {
    if (selectedCategories.length === 0) {
      return Array.from(new Set(config.categories.flatMap((c) => c.specialties)));
    }
    return Array.from(new Set(
      selectedCategories.flatMap(
        (key) => config.categories.find((c) => c.key === key)?.specialties ?? [],
      ),
    ));
  }, [config.categories, selectedCategories]);

  const countryIsValidForNuet = useMemo(() => {
    if (exam !== 'NUET') return true;
    if (selectedCountries.length === 0) return true;
    return selectedCountries.some((c) => {
      const n = normalizeText(c);
      return n.includes('kazakh') || n.includes('kz');
    });
  }, [exam, selectedCountries]);

  const filteredResult = useMemo(() => {
    if (!submitted) return null;
    if (!exam) return { error: 'Выберите SAT или NUET.' };
    if (!currentScore) return { error: 'Введите текущий балл.' };
    const score = Number(currentScore);
    if (Number.isNaN(score) || score <= 0) return { error: 'Введите корректное значение.' };
    if (exam === 'SAT' && score > 1600) return { error: 'Максимальный балл SAT — 1600.' };
    if (exam === 'NUET' && score > 200) return { error: 'Максимальный балл NUET — 200.' };
    if (!countryIsValidForNuet) return { error: 'NUET доступен только в Казахстане.' };

    const available = config.universities.filter((u) => u.exam === exam);
    const filtered = available.filter((u) => {
      if (!matchesSpecialty(u, selectedCategories, selectedSpecialties, config.categories)) return false;
      if (!matchesCountry(u, selectedCountries, exam)) return false;
      if (!matchesLanguage(u, selectedLanguages)) return false;
      if (grant === 'yes' && !u.grants) return false;
      if (grant === 'no' && u.grants) return false;
      if (difficulty !== 'Любой' && normalizeText(u.level) !== normalizeText(difficulty)) return false;
      return true;
    });

    const already = filtered
      .filter((u) => u.minScore !== null && score >= u.minScore)
      .sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));

    const goals = filtered
      .filter((u) => u.minScore !== null && score < u.minScore && (u.minScore ?? 0) <= score + 100)
      .sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));

    const fallback = filtered.filter((u) => u.minScore === null);

    return { score, goal: goalScore ? Number(goalScore) : null, filtered, already, goals, fallback };
  }, [submitted, exam, currentScore, goalScore, selectedCategories, selectedSpecialties,
      selectedCountries, selectedLanguages, grant, difficulty, config.universities,
      config.categories, countryIsValidForNuet]);

  const highlightedIds = useMemo<Set<string>>(() => {
    if (!filteredResult || 'error' in filteredResult) return new Set();
    return new Set(filteredResult.filtered.map((u) => u.id));
  }, [filteredResult]);

  const handleCategoryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const vals = getSelectedValues(e);
    setSelectedCategories(vals);
    setSelectedSpecialties((prev) => prev.filter((s) => specialtyOptions.includes(s)));
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setSubmitted(true);
    setMarkerFocusId(null);
  };

  const handleMarkerClick = (id: string) => {
    setMarkerFocusId(id);
    const ref = cardRefs.current[id];
    if (ref?.current) {
      ref.current.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  };

  const allDisplayedUnis = useMemo(() => {
    if (!filteredResult || 'error' in filteredResult) return [];
    return [...filteredResult.already, ...filteredResult.goals, ...filteredResult.fallback];
  }, [filteredResult]);

  allDisplayedUnis.forEach((u) => {
    if (!cardRefs.current[u.id]) {
      cardRefs.current[u.id] = { current: null };
    }
  });

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto' }}>
      <h1>Академический советник UniStart</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Подбор университета на основе SAT или NUET.
        Наведите на маркер для предпросмотра, нажмите — перейдёт к карточке.
      </p>

      <AdvisorWorldMap
        universities={config.universities}
        highlightedIds={highlightedIds}
        onMarkerClick={handleMarkerClick}
      />

      <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '1rem', marginBottom: '2rem' }}>
        <div className="form-group">
          <label>Экзамен</label>
          <select className="form-input" value={exam} onChange={(e) => setExam(e.target.value as 'SAT' | 'NUET' | '')}>
            <option value="">Выберите экзамен</option>
            <option value="SAT">SAT</option>
            <option value="NUET">NUET</option>
          </select>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Текущий балл</label>
            <input className="form-input" type="number" min={0} value={currentScore}
              onChange={(e) => setCurrentScore(e.target.value)}
              placeholder={exam === 'NUET' ? '0–200' : '0–1600'} />
          </div>
          <div className="form-group">
            <label>Целевой балл (необязательно)</label>
            <input className="form-input" type="number" min={0} value={goalScore}
              onChange={(e) => setGoalScore(e.target.value)}
              placeholder={exam === 'NUET' ? '0–200' : '0–1600'} />
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Категория</label>
            <select className="form-input" multiple size={6} value={selectedCategories} onChange={handleCategoryChange}>
              {config.categories.map((c) => (
                <option key={c.key} value={c.key}>{c.label}</option>
              ))}
            </select>
            <p style={{ margin: '0.4rem 0 0', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
              Зажмите Ctrl, чтобы выбрать несколько категорий.
            </p>
          </div>
          <div className="form-group">
            <label>Специальность</label>
            <select className="form-input" multiple size={8} value={selectedSpecialties}
              onChange={(e) => setSelectedSpecialties(getSelectedValues(e))}>
              {specialtyOptions.map((sp) => (
                <option key={sp} value={sp}>{sp}</option>
              ))}
            </select>
            <p style={{ margin: '0.4rem 0 0', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
              Зажмите Ctrl, чтобы выбрать несколько специальностей.
            </p>
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Страна</label>
            <select className="form-input" multiple size={5} value={selectedCountries}
              onChange={(e) => setSelectedCountries(getSelectedValues(e))}>
              {config.countries.map((c) => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>
            <p style={{ margin: '0.4rem 0 0', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
              Можно выбрать несколько стран. Для NUET — только Казахстан.
            </p>
          </div>
          <div className="form-group">
            <label>Язык обучения</label>
            <select className="form-input" multiple size={4} value={selectedLanguages}
              onChange={(e) => setSelectedLanguages(getSelectedValues(e))}>
              {config.languages.map((l) => (
                <option key={l} value={l}>{l}</option>
              ))}
            </select>
            <p style={{ margin: '0.4rem 0 0', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
              Зажмите Ctrl, чтобы выбрать несколько языков.
            </p>
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Грант</label>
            <select className="form-input" value={grant} onChange={(e) => setGrant(e.target.value as 'any' | 'yes' | 'no')}>
              <option value="any">Любой</option>
              <option value="yes">Только с грантом</option>
              <option value="no">Без гранта</option>
            </select>
          </div>
          <div className="form-group">
            <label>Сложность поступления</label>
            <select className="form-input" value={difficulty} onChange={(e) => setDifficulty(e.target.value)}>
              <option>Любой</option>
              <option>Несложно</option>
              <option>Средне</option>
              <option>Сложно</option>
              <option>Очень сложно</option>
            </select>
          </div>
          <div className="form-group" style={{ alignSelf: 'end' }}>
            <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
              Подобрать университеты
            </button>
          </div>
        </div>
      </form>

      {submitted && filteredResult && (
        <div>
          {'error' in filteredResult ? (
            <div className="card" style={{ background: 'var(--error-bg)', color: 'var(--error-text)', padding: '1rem' }}>
              {filteredResult.error}
            </div>
          ) : (
            <>
              <div className="card" style={{ marginBottom: '1rem' }}>
                <p style={{ margin: 0 }}>
                  <strong>Экзамен:</strong> {exam}
                  {' · '}<strong>Балл:</strong> {filteredResult.score}
                  {filteredResult.goal ? ` · Цель: ${filteredResult.goal}` : ''}
                </p>
                <p style={{ margin: '0.6rem 0 0', color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
                  Рекомендации носят ориентировочный характер.
                  Проверяйте актуальные требования на официальных сайтах.
                </p>
              </div>

              {exam === 'NUET' && (
                <div className="card" style={{ background: '#eff6ff', color: '#1e3a8a', padding: '1rem', marginBottom: '1rem' }}>
                  NUET принимается только в вузах Казахстана.
                </div>
              )}

              {filteredResult.already.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>
                    Уже проходите ({filteredResult.already.length})
                  </h2>
                  {filteredResult.already.map((u) => (
                    <UniCard key={u.id} uni={u} highlighted={markerFocusId === u.id} refProp={cardRefs.current[u.id]} />
                  ))}
                </div>
              )}

              {filteredResult.goals.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>
                    Цель ({filteredResult.goals.length})
                  </h2>
                  {filteredResult.goals.map((u) => (
                    <UniCard key={u.id} uni={u} highlighted={markerFocusId === u.id} refProp={cardRefs.current[u.id]} />
                  ))}
                </div>
              )}

              {filteredResult.fallback.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>
                    Рекомендуется ({filteredResult.fallback.length})
                  </h2>
                  {filteredResult.fallback.map((u) => (
                    <UniCard key={u.id} uni={u} highlighted={markerFocusId === u.id} refProp={cardRefs.current[u.id]} />
                  ))}
                </div>
              )}

              {filteredResult.filtered.length === 0 && (
                <div className="card" style={{ padding: '1rem' }}>
                  <p style={{ margin: 0 }}>
                    Нет университетов по выбранным критериям.
                    Попробуйте расширить фильтры.
                  </p>
                </div>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default UniversityAdvisorPage;
