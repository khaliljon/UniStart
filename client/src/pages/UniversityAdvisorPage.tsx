import { useMemo, useState } from 'react';
import { AdvisorCategory, AdvisorUniversity, loadAdvisorConfig } from '../advisorConfig';

function normalizeText(text: string | null | undefined): string {
  return (text ?? '').trim().toLowerCase();
}

function matchesSpecialty(
  university: AdvisorUniversity,
  selectedCategories: string[],
  selectedSpecialties: string[],
  categories: AdvisorCategory[]
): boolean {
  if (selectedCategories.length === 0 && selectedSpecialties.length === 0) {
    return true;
  }

  const activeSpecialties = selectedSpecialties.length > 0
    ? selectedSpecialties
    : selectedCategories.flatMap((categoryKey) =>
        categories.find((category) => category.key === categoryKey)?.specialties ?? []
      );

  const normalizedUniversity = university.specialties.map(normalizeText);
  const normalizedSelected = activeSpecialties.map(normalizeText);

  return normalizedSelected.some((selected) =>
    normalizedUniversity.some((specialty) => specialty.includes(selected) || selected.includes(specialty))
  );
}

function matchesCountry(
  university: AdvisorUniversity,
  selectedCountries: string[],
  exam: 'SAT' | 'NUET' | ''
): boolean {
  if (selectedCountries.length === 0) return true;
  const normalizedCountry = normalizeText(university.country);
  return selectedCountries.some((selected) => {
    const value = normalizeText(selected);
    if (exam === 'NUET' && !value.includes('kazakh') && !value.includes('kz')) {
      return false;
    }
    return normalizedCountry.includes(value) || value.includes(normalizedCountry);
  });
}

function matchesLanguage(university: AdvisorUniversity, selectedLanguages: string[]): boolean {
  if (selectedLanguages.length === 0) return true;
  const normalizedLanguage = normalizeText(university.language);
  return selectedLanguages.some((language) => {
    const selected = normalizeText(language);
    if (selected === 'other') {
      return !normalizedLanguage.includes('english') && !normalizedLanguage.includes('japanese');
    }
    return normalizedLanguage.includes(selected);
  });
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
  const [difficulty, setDifficulty] = useState('Любая');
  const [submitted, setSubmitted] = useState(false);

  const specialtyOptions = useMemo(() => {
    if (selectedCategories.length === 0) {
      return Array.from(new Set(config.categories.flatMap((category) => category.specialties)));
    }
    return Array.from(new Set(selectedCategories.flatMap((categoryKey) =>
      config.categories.find((category) => category.key === categoryKey)?.specialties ?? []
    )));
  }, [config.categories, selectedCategories]);

  const countryIsValidForNuet = useMemo(() => {
    if (exam !== 'NUET') return true;
    if (selectedCountries.length === 0) return true;
    return selectedCountries.some((country) => {
      const normalized = normalizeText(country);
      return normalized.includes('kazakh') || normalized.includes('kz');
    });
  }, [exam, selectedCountries]);

  const getSelectedValues = (event: React.ChangeEvent<HTMLSelectElement>) =>
    Array.from(event.target.selectedOptions).map((option) => option.value);

  const filteredResult = useMemo(() => {
    if (!submitted) return null;
    if (!exam) return { error: 'Укажите SAT или NUET.' };
    if (!currentScore) return { error: 'Укажите текущий балл по экзамену.' };
    const score = Number(currentScore);
    if (Number.isNaN(score) || score <= 0) return { error: 'Введите корректный числовой балл.' };
    if (exam === 'SAT' && score > 1600) return { error: 'Для SAT допустим балл до 1600.' };
    if (exam === 'NUET' && score > 200) return { error: 'Для NUET допустим балл до 200.' };
    if (!countryIsValidForNuet) return { error: 'NUET работает только для Казахстана. Укажите Казахстан или оставьте поле страны пустым.' };

    const available = config.universities.filter((uni) => uni.exam === exam);
    const filtered = available.filter((uni) => {
      if (!matchesSpecialty(uni, selectedCategories, selectedSpecialties, config.categories)) return false;
      if (!matchesCountry(uni, selectedCountries, exam)) return false;
      if (!matchesLanguage(uni, selectedLanguages)) return false;
      if (grant === 'yes' && !uni.grants) return false;
      if (grant === 'no' && uni.grants) return false;
      if (difficulty !== 'Любая' && normalizeText(uni.level) !== normalizeText(difficulty)) return false;
      return true;
    });

    const already = filtered.filter((uni) => uni.minScore !== null && score >= uni.minScore).sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));
    const goals = filtered.filter((uni) => uni.minScore !== null && score < uni.minScore && (uni.minScore ?? 0) <= score + 100).sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));
    const fallback = filtered.filter((uni) => uni.minScore === null);

    return {
      score,
      goal: goalScore ? Number(goalScore) : null,
      filtered,
      already,
      goals,
      fallback,
    };
  }, [submitted, exam, currentScore, goalScore, selectedCategories, selectedSpecialties, selectedCountries, selectedLanguages, grant, difficulty, config.universities, config.categories, countryIsValidForNuet]);

  const handleCategoryChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const values = getSelectedValues(event);
    setSelectedCategories(values);
    setSelectedSpecialties((current) => current.filter((specialty) => specialtyOptions.includes(specialty)));
  };

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitted(true);
  };

  return (
    <div style={{ maxWidth: 960, margin: '0 auto' }}>
      <h1>Академический советник UniStart</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Помогаем подобрать подходящие SAT/NUET направления и университеты по категориям, странам, языку и уровню сложности.
      </p>

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
            <input
              className="form-input"
              type="number"
              min={0}
              value={currentScore}
              onChange={(e) => setCurrentScore(e.target.value)}
              placeholder={exam === 'NUET' ? '0–200' : '0–1600'}
            />
          </div>
          <div className="form-group">
            <label>Целевой балл (необязательно)</label>
            <input
              className="form-input"
              type="number"
              min={0}
              value={goalScore}
              onChange={(e) => setGoalScore(e.target.value)}
              placeholder={exam === 'NUET' ? '0–200' : '0–1600'}
            />
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Категории</label>
            <select
              className="form-input"
              multiple
              size={6}
              value={selectedCategories}
              onChange={handleCategoryChange}
            >
              {config.categories.map((category) => (
                <option key={category.key} value={category.key}>
                  {category.label}
                </option>
              ))}
            </select>
            <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Выберите одну или несколько категорий. Оставьте пустым, чтобы выбрать все направления.
            </p>
          </div>

          <div className="form-group">
            <label>Специальности</label>
            <select
              className="form-input"
              multiple
              size={8}
              value={selectedSpecialties}
              onChange={(event) => setSelectedSpecialties(getSelectedValues(event))}
            >
              {specialtyOptions.map((specialty) => (
                <option key={specialty} value={specialty}>{specialty}</option>
              ))}
            </select>
            <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Оставьте пустым, чтобы выбирать все специальности из выбранных категорий.
            </p>
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Страны</label>
            <select
              className="form-input"
              multiple
              size={5}
              value={selectedCountries}
              onChange={(event) => setSelectedCountries(getSelectedValues(event))}
            >
              {config.countries.map((countryOption) => (
                <option key={countryOption} value={countryOption}>{countryOption}</option>
              ))}
            </select>
            <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Оставьте пустым для всех стран. Для NUET выбирайте Казахстан.
            </p>
          </div>

          <div className="form-group">
            <label>Язык обучения</label>
            <select
              className="form-input"
              multiple
              size={4}
              value={selectedLanguages}
              onChange={(event) => setSelectedLanguages(getSelectedValues(event))}
            >
              {config.languages.map((languageOption) => (
                <option key={languageOption} value={languageOption}>{languageOption}</option>
              ))}
            </select>
            <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Оставьте пустым, чтобы выбрать все языки.
            </p>
          </div>
        </div>

        <div className="form-grid" style={{ gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
          <div className="form-group">
            <label>Грант</label>
            <select className="form-input" value={grant} onChange={(e) => setGrant(e.target.value as 'any' | 'yes' | 'no')}>
              <option value="any">Любой</option>
              <option value="yes">Только с грантом</option>
              <option value="no">Без учета гранта</option>
            </select>
          </div>
          <div className="form-group">
            <label>Сложность</label>
            <select className="form-input" value={difficulty} onChange={(e) => setDifficulty(e.target.value)}>
              <option>Любая</option>
              <option>Несложно</option>
              <option>Умеренно</option>
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
                  <strong>Экзамен:</strong> {exam} · Текущий балл: {filteredResult.score}
                  {filteredResult.goal ? ` · Цель: ${filteredResult.goal}` : ''}
                </p>
                <p style={{ margin: '0.75rem 0 0', color: 'var(--text-secondary)' }}>
                  Это ориентировочные рекомендации. Окончательное решение зависит от требований каждого университета.
                </p>
              </div>

              {exam === 'NUET' && (
                <div className="card" style={{ background: '#eff6ff', color: '#1e3a8a', padding: '1rem', marginBottom: '1rem' }}>
                  Раздел NUET поддерживает подбор по вузам Казахстана. В ближайшее время расширим список вузов и направлений.
                </div>
              )}

              {exam === 'SAT' && selectedCountries.length > 0 && !selectedCountries.some((country) => normalizeText(country).includes('japan')) && (
                <div className="card" style={{ background: '#fff4e5', color: '#92400e', padding: '1rem', marginBottom: '1rem' }}>
                  SAT подбор по другим странам пока в разработке. Сейчас доступны рекомендации по японским университетам.
                </div>
              )}

              {filteredResult.already.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>Уже подходит</h2>
                  {filteredResult.already.map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <div style={{ display: 'grid', gap: '0.5rem' }}>
                        <p style={{ margin: 0 }}>Страна: {university.country}</p>
                        <p style={{ margin: 0 }}>Специальности: {university.specialties.join(', ')}</p>
                        <p style={{ margin: 0 }}>Язык: {university.language}</p>
                        <p style={{ margin: 0 }}>Гранты: {university.grants}</p>
                        <p style={{ margin: 0 }}>Плюсы: {university.plus}</p>
                        <p style={{ margin: 0 }}>Минусы: {university.minus}</p>
                        {university.procedure && <p style={{ margin: 0 }}>Подача: {university.procedure}</p>}
                        {university.deadlines && <p style={{ margin: 0, color: 'var(--text-secondary)' }}>Сроки: {university.deadlines}</p>}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {filteredResult.goals.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>Цели на вырост</h2>
                  {filteredResult.goals.map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <div style={{ display: 'grid', gap: '0.5rem' }}>
                        <p style={{ margin: 0 }}>Мин. балл: {university.minScore}</p>
                        <p style={{ margin: 0 }}>Специальности: {university.specialties.join(', ')}</p>
                        <p style={{ margin: 0 }}>Язык: {university.language}</p>
                        <p style={{ margin: 0 }}>Плюсы: {university.plus}</p>
                        <p style={{ margin: 0 }}>Минусы: {university.minus}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {filteredResult.already.length === 0 && filteredResult.goals.length === 0 && filteredResult.filtered.length > 0 && (
                <div>
                  <h2 style={{ marginBottom: '0.75rem' }}>Рекомендации</h2>
                  {filteredResult.filtered.map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <div style={{ display: 'grid', gap: '0.5rem' }}>
                        <p style={{ margin: 0 }}>Специальности: {university.specialties.join(', ')}</p>
                        <p style={{ margin: 0 }}>Язык: {university.language}</p>
                        <p style={{ margin: 0 }}>Гранты: {university.grants}</p>
                        <p style={{ margin: 0 }}>Плюсы: {university.plus}</p>
                        <p style={{ margin: 0 }}>Минусы: {university.minus}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {filteredResult.filtered.length === 0 && (
                <div className="card" style={{ padding: '1rem' }}>
                  <p style={{ margin: 0 }}>
                    Для выбранных фильтров пока нет рекомендаций. Попробуйте расширить выбор категорий, стран или языков.
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
