import { useMemo, useState } from 'react';

type University = {
  id: string;
  name: string;
  city: string;
  country: string;
  exam: 'SAT' | 'NUET';
  minScore: number | null;
  level: string;
  specialties: string[];
  language: string;
  grants: string;
  plus: string;
  minus: string;
  acceptance?: string;
  deadlines?: string;
};

const SAT_UNIVERSITIES: University[] = [
  {
    id: 'toyo',
    name: 'Toyo University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['Economics', 'Education', 'Philosophy', 'History', 'Literature', 'Sociology'],
    language: 'English / Japanese',
    grants: 'Есть частичные гранты. Официальный сайт для деталей.',
    plus: 'Низкий порог, широкий выбор программ.',
    minus: 'Слабая международная узнаваемость.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'apucampus',
    name: 'Ritsumeikan APU',
    city: 'Beppu',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['International Relations', 'Media', 'Culture', 'Management', 'Marketing', 'Accounting', 'Entrepreneurship', 'Tourism'],
    language: 'English',
    grants: 'Есть международные гранты и стипендии.',
    plus: 'Сильный международный кампус, много иностранных студентов.',
    minus: 'Расположен не в Токио.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'okayama',
    name: 'Okayama University',
    city: 'Okayama',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['Math', 'Physics', 'Chemistry', 'Biology', 'Computer Science', 'Mechanical Engineering', 'Electrical Engineering', 'Civil Engineering', 'Economics', 'Law', 'Medicine', 'Pharmacy', 'Dentistry', 'Agriculture', 'Linguistics', 'Philosophy', 'History', 'Literature', 'Sociology', 'Anthropology', 'Psychology'],
    language: 'English / Japanese',
    grants: 'Есть государственные гранты MEXT и факультетские стипендии.',
    plus: 'Национальный вуз с широким академическим охватом.',
    minus: 'Высокая конкуренция среди иностранных абитуриентов.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'kuas',
    name: 'Kyoto University of Advanced Science',
    city: 'Kyoto',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['Mechanical Engineering', 'Electrical Engineering', 'Applied Biological Sciences', 'Environmental Sciences', 'Bioresource Sciences', 'Business'],
    language: 'English',
    grants: 'Есть частные гранты и стипендии университета.',
    plus: 'Современный кампус с практическим акцентом.',
    minus: 'Рейтинг ниже национальных университетов.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'kanazawa',
    name: 'Kanazawa University',
    city: 'Kanazawa',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['Math', 'Physics', 'Chemistry', 'Information and Communication', 'Mechanical Engineering', 'Environmental Design', 'Economics', 'Law', 'Medicine', 'Pharmacy', 'Nursing', 'Education'],
    language: 'English / Japanese',
    grants: 'Есть гранты MEXT и академические стипендии.',
    plus: 'Национальный вуз, медицина и фармация.',
    minus: 'Провинциальный город.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'aiu',
    name: 'Akita International University',
    city: 'Akita',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['English Language Teaching', 'Culture', 'Media', 'Japan Studies', 'Arts', 'Humanities', 'Global Studies'],
    language: 'English',
    grants: 'Есть гранты и стипендии для иностранных студентов.',
    plus: '100% английский, сильная языковая среда.',
    minus: 'Только гуманитарные направления.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'tsukuba',
    name: 'University of Tsukuba',
    city: 'Tsukuba',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Engineering', 'Sciences', 'Humanities', 'Social Sciences'],
    language: 'English / Japanese',
    grants: 'Есть исследования и академические гранты.',
    plus: 'Топ исследовательский университет.',
    minus: '60 км от Токио.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'sophia',
    name: 'Sophia University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Humanities', 'Social Sciences', 'International Studies', 'Languages'],
    language: 'English',
    grants: 'Есть международные гранты и стипендии.',
    plus: 'Центр Токио, сильные международные программы.',
    minus: 'Высокая стоимость обучения.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'ritsumeikan',
    name: 'Ritsumeikan University',
    city: 'Kyoto',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Sciences', 'Engineering', 'Humanities', 'Business', 'International Relations'],
    language: 'English / Japanese',
    grants: 'Есть университетские гранты.',
    plus: 'Крупный частный университет со сильным инженерным факультетом.',
    minus: 'Конкуренция высокая.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'meiji',
    name: 'Meiji University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Law', 'Commerce', 'Political Science', 'Economics', 'Business Administration'],
    language: 'Japanese',
    grants: 'Есть программы частичной поддержки.',
    plus: 'Престижный частный университет Токио.',
    minus: 'Большинство программ на японском.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'kyushu',
    name: 'Kyushu University',
    city: 'Fukuoka',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Engineering', 'Sciences', 'Agriculture', 'Medicine'],
    language: 'English / Japanese',
    grants: 'Есть гранты MEXT и исследовательские стипендии.',
    plus: 'Топ-7 Японии, сильная исследовательская база.',
    minus: 'Фукуока далеко от Токио.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'icu',
    name: 'International Christian University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Liberal Arts'],
    language: 'English',
    grants: 'Есть академические стипендии.',
    plus: 'Уникальная либеральная система, 100% английский.',
    minus: 'Очень высокий конкурс.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'hokkaido',
    name: 'Hokkaido University',
    city: 'Sapporo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Agriculture', 'Engineering', 'Sciences', 'Medicine', 'Pharmacy'],
    language: 'English / Japanese',
    grants: 'Есть гранты MEXT и исследовательские стипендии.',
    plus: 'Топ-10 Японии, сильный научный профиль.',
    minus: 'Холодный климат.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'hiroshima',
    name: 'Hiroshima University',
    city: 'Hiroshima',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Education', 'Sciences', 'Engineering', 'Social Sciences', 'Medicine'],
    language: 'English / Japanese',
    grants: 'Есть государственные гранты.',
    plus: 'Национальный университет с сильным педагогическим профилем.',
    minus: 'Не в Токио/Осака.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'doshisha',
    name: 'Doshisha University',
    city: 'Kyoto',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1400,
    level: 'Умеренно',
    specialties: ['Humanities', 'Law', 'Commerce', 'Social Sciences', 'Sciences'],
    language: 'Japanese',
    grants: 'Есть частичные гранты и стипендии.',
    plus: 'Один из лучших частных университетов Киото.',
    minus: 'Большинство программ на японском.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'waseda',
    name: 'Waseda University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Law', 'Political Science', 'Economics', 'Business', 'Literature', 'Education', 'Sciences'],
    language: 'English / Japanese',
    grants: 'Есть международные стипендии и частичные гранты.',
    plus: 'Топ-3 частных вузов Японии.',
    minus: 'Очень высокий конкурс.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'utokyo',
    name: 'University of Tokyo',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Engineering', 'Sciences', 'Medicine', 'Law', 'Economics', 'Humanities'],
    language: 'Japanese',
    grants: 'Есть исследовательские гранты и частичная поддержка.',
    plus: '№1 Японии и Азии.',
    minus: 'Крайне высокий конкурс.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'tokyo-tech',
    name: 'Tokyo Institute of Technology',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Engineering', 'Sciences', 'Computing', 'Materials Science'],
    language: 'English / Japanese',
    grants: 'Есть исследовательские гранты.',
    plus: 'Лучший технический вуз Японии.',
    minus: 'Только технические науки.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'tohoku',
    name: 'Tohoku University',
    city: 'Sendai',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Engineering', 'Sciences', 'Medicine', 'Agriculture'],
    language: 'English / Japanese',
    grants: 'Есть гранты MEXT и исследовательские стипендии.',
    plus: 'Топ-5 Японии, сильная исследовательская база.',
    minus: 'Не Токио.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'osaka',
    name: 'Osaka University',
    city: 'Osaka',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Engineering', 'Sciences', 'Medicine', 'Humanities', 'Economics'],
    language: 'English / Japanese',
    grants: 'Есть исследовательские гранты.',
    plus: 'Топ-3 Японии, сильная медицина.',
    minus: 'Очень высокая конкуренция.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'nagoya',
    name: 'Nagoya University',
    city: 'Nagoya',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Engineering', 'Sciences', 'Medicine', 'Law', 'Economics'],
    language: 'English / Japanese',
    grants: 'Есть университетские гранты.',
    plus: 'Топ-7 Японии, Нобелевская репутация.',
    minus: 'Многие программы на японском.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'kyoto',
    name: 'Kyoto University',
    city: 'Kyoto',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Sciences', 'Engineering', 'Medicine', 'Law', 'Economics', 'Humanities'],
    language: 'Japanese',
    grants: 'Есть международные гранты, но большинство программ на японском.',
    plus: '№2 Японии, исторический университет.',
    minus: 'Почти всё на японском.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
  {
    id: 'keio',
    name: 'Keio University',
    city: 'Tokyo',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1500,
    level: 'Очень сложно',
    specialties: ['Economics', 'Law', 'Commerce', 'Sciences', 'Engineering', 'Medicine'],
    language: 'Japanese',
    grants: 'Есть частичные гранты.',
    plus: 'Топ-2 частный университет Японии.',
    minus: 'Высокая стоимость и конкурс.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
  },
];

const NUET_PLACEHOLDERS: University[] = [
  {
    id: 'nug',
    name: 'Nazarbayev University',
    city: 'Nur-Sultan',
    country: 'Kazakhstan',
    exam: 'NUET',
    minScore: null,
    level: 'Сложно',
    specialties: ['Engineering', 'Sciences', 'Business', 'Computer Science'],
    language: 'English',
    grants: 'Есть конкурсные гранты и стажировки.',
    plus: 'Ведущий национальный университет Казахстана.',
    minus: 'Очень высокий конкурс.',
    acceptance: 'нет данных',
    deadlines: 'Официальный сайт НУ',
  },
  {
    id: 'kbtu',
    name: 'KBTU',
    city: 'Almaty',
    country: 'Kazakhstan',
    exam: 'NUET',
    minScore: null,
    level: 'Средне',
    specialties: ['Engineering', 'Economics', 'Business', 'IT'],
    language: 'English / Russian',
    grants: 'Есть университетские гранты.',
    plus: 'Сильный инженерно-экономический профиль.',
    minus: 'Конкуренция на лучшие программы.',
    acceptance: 'нет данных',
    deadlines: 'Официальный сайт KBTU',
  },
];

const SPECIALTY_KEYWORDS: Record<string, string[]> = {
  education: ['education'],
  arts: ['arts', 'humanities', 'culture', 'philosophy', 'literature', 'languages'],
  social: ['social', 'journalism', 'media', 'information', 'political', 'international relations', 'sociology', 'anthropology'],
  business: ['business', 'commerce', 'management', 'marketing', 'economics', 'accounting', 'law', 'administration'],
  science: ['science', 'math', 'physics', 'chemistry', 'biology', 'statistics', 'agriculture'],
  it: ['it', 'computer', 'cs', 'information', 'communication', 'computing'],
  engineering: ['engineering', 'mechanical', 'electrical', 'civil', 'environmental', 'materials'],
  agriculture: ['agriculture', 'veterinary', 'environmental', 'bioresource'],
  health: ['medicine', 'pharmacy', 'nursing', 'health', 'medical'],
  services: ['tourism', 'hospitality', 'services'],
};

function normalizeText(text: string | null | undefined): string {
  return (text ?? '').trim().toLowerCase();
}

function matchesSpecialty(university: University, specialty: string): boolean {
  const normalized = normalizeText(specialty);
  if (!normalized || normalized === 'any' || normalized === 'любая') return true;

  const uniText = university.specialties.join(' ').toLowerCase();
  if (uniText.includes(normalized)) return true;

  return Object.values(SPECIALTY_KEYWORDS).some((keywords) =>
    keywords.some((keyword) => normalized.includes(keyword) || uniText.includes(keyword))
  );
}

function matchesCountry(exam: 'SAT' | 'NUET', country: string): boolean {
  const normalized = normalizeText(country);
  if (!normalized || normalized === 'any' || normalized === 'любая') return true;
  if (exam === 'NUET') return normalized.includes('kazakh') || normalized.includes('kz') || normalized.includes('казах');
  return normalized.includes('japan') || normalized.includes('jpn') || normalized.includes('asia') || normalized.includes('america') || normalized.includes('canada') || normalized.includes('singapore') || normalized.includes('uae') || normalized.includes('korea') || normalized.includes('hong kong') || normalized.includes('europe') || normalized.includes('usa');
}

function UniversityAdvisorPage() {
  const [exam, setExam] = useState<'SAT' | 'NUET' | ''>('');
  const [currentScore, setCurrentScore] = useState('');
  const [goalScore, setGoalScore] = useState('');
  const [specialty, setSpecialty] = useState('');
  const [country, setCountry] = useState('Любая');
  const [grant, setGrant] = useState<'any' | 'yes' | 'no'>('any');
  const [language, setLanguage] = useState<'any' | 'english' | 'japanese' | 'other'>('any');
  const [submitted, setSubmitted] = useState(false);

  const selectedGoal = Number(goalScore);
  const isSat = exam === 'SAT';
  const isNuet = exam === 'NUET';

  const countrySupported = useMemo(() => {
    const normalized = normalizeText(country);
    if (!normalized || normalized === 'any' || normalized === 'любая') return true;
    if (isNuet) return normalized.includes('kazakh') || normalized.includes('kz') || normalized.includes('каз');
    return normalized.includes('japan') || normalized.includes('jpn') || normalized.includes('usa') || normalized.includes('america') || normalized.includes('canada') || normalized.includes('singapore') || normalized.includes('uae') || normalized.includes('korea') || normalized.includes('hong') || normalized.includes('europe');
  }, [country, isNuet]);

  const result = useMemo(() => {
    if (!submitted) return null;

    if (!exam) {
      return { error: 'Укажите SAT или NUET.' };
    }

    if (!isSat && !isNuet) {
      return { error: 'Платформа работает только с SAT и NUET.' };
    }

    if (exam === 'NUET' && !countrySupported) {
      return { error: 'NUET работает только для Казахстана. Укажите Казахстан или «любая».' };
    }

    if (!currentScore) {
      return { error: 'Укажите текущий балл по экзамену.' };
    }

    const score = Number(currentScore);
    if (Number.isNaN(score) || score <= 0) {
      return { error: 'Введите корректный числовой балл.' };
    }

    if (exam === 'SAT' && score > 1600) {
      return { error: 'Для SAT допустим балл до 1600.' };
    }
    if (exam === 'NUET' && score > 200) {
      return { error: 'Для NUET допустим балл до 200.' };
    }

    const useDegree = selectedGoal > 0 && selectedGoal > score;

    const available = exam === 'SAT' ? SAT_UNIVERSITIES : NUET_PLACEHOLDERS;
    const filtered = available.filter((u) => {
      if (!matchesSpecialty(u, specialty)) return false;
      if (!matchesCountry(exam, country)) return false;
      if (grant === 'yes' && !u.grants) return false;
      if (language === 'english' && !u.language.toLowerCase().includes('english')) return false;
      if (language === 'japanese' && !u.language.toLowerCase().includes('japanese')) return false;
      if (language === 'other' && u.language.toLowerCase().includes('english')) return false;
      return true;
    });

    const already = filtered.filter((u) => u.minScore !== null && score >= (u.minScore ?? 0));
    const goals = filtered.filter((u) => u.minScore !== null && score < (u.minScore ?? 0) && (u.minScore ?? 0) <= score + 100);
    const fallback = filtered.filter((u) => u.minScore === null);

    const sortedAlready = already.sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));
    const sortedGoals = goals.sort((a, b) => (a.minScore ?? 0) - (b.minScore ?? 0));

    return {
      score,
      goal: useDegree ? selectedGoal : null,
      countrySupported,
      filtered,
      already: sortedAlready,
      goals: sortedGoals,
      fallback,
    };
  }, [submitted, exam, currentScore, selectedGoal, specialty, country, grant, language, isSat, isNuet, countrySupported]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitted(true);
  };

  return (
    <div style={{ maxWidth: 900, margin: '0 auto' }}>
      <h1>Академический советник UniStart</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Подбор университета на основе SAT или NUET. Платформа работает только с этими экзаменами.
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

        <div className="form-group">
          <label>Специальность / направление</label>
          <input
            className="form-input"
            value={specialty}
            onChange={(e) => setSpecialty(e.target.value)}
            placeholder="Например, IT, экономика, инженерия, гуманитарные науки"
          />
        </div>

        <div className="form-group">
          <label>Страна</label>
          <select className="form-input" value={country} onChange={(e) => setCountry(e.target.value)}>
            <option>Любая</option>
            <option>Япония</option>
            <option>Казахстан</option>
            <option>США</option>
            <option>Канада</option>
            <option>Сингапур</option>
            <option>ОАЭ</option>
            <option>Корея</option>
            <option>Гонконг</option>
            <option>Европа</option>
          </select>
        </div>

        <div className="form-group">
          <label>Грант</label>
          <select className="form-input" value={grant} onChange={(e) => setGrant(e.target.value as 'any' | 'yes' | 'no')}>
            <option value="any">Любой</option>
            <option value="yes">Важен</option>
            <option value="no">Не важен</option>
          </select>
        </div>

        <div className="form-group">
          <label>Язык обучения</label>
          <select className="form-input" value={language} onChange={(e) => setLanguage(e.target.value as 'any' | 'english' | 'japanese' | 'other')}>
            <option value="any">Любой</option>
            <option value="english">Английский</option>
            <option value="japanese">Японский</option>
            <option value="other">Другой</option>
          </select>
        </div>

        <button type="submit" className="btn btn-primary" style={{ width: 'fit-content' }}>
          Подобрать университеты
        </button>
      </form>

      {submitted && result && (
        <div>
          {'error' in result ? (
            <div className="card" style={{ background: 'var(--error-bg)', color: 'var(--error-text)', padding: '1rem' }}>
              {result.error}
            </div>
          ) : (
            <>
              <div className="card" style={{ marginBottom: '1rem' }}>
                <p style={{ margin: 0 }}>
                  <strong>Экзамен:</strong> {exam}
                  {result.score ? ` · Текущий балл: ${result.score}` : ''}
                  {result.goal ? ` · Цель: ${result.goal}` : ''}
                </p>
                <p style={{ margin: '0.75rem 0 0', color: 'var(--text-secondary)' }}>
                  Важно: это ориентировочные рекомендации. Поступление не гарантируется.
                </p>
              </div>

              {exam === 'SAT' && !countrySupported && (
                <div className="card" style={{ background: '#fff4e5', color: '#92400e', padding: '1rem', marginBottom: '1rem' }}>
                  SAT доступен для Японии и других стран, но в текущем разделе рекомендованы японские университеты.
                </div>
              )}

              {exam === 'NUET' && (
                <div className="card" style={{ background: '#eff6ff', color: '#1e3a8a', padding: '1rem', marginBottom: '1rem' }}>
                  Раздел NUET в разработке. Пока доступны общие рекомендации по ведущим вузам Казахстана.
                </div>
              )}

              {result.already.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>Уже проходит</h2>
                  {result.already.slice(0, 5).map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <p style={{ margin: '0.25rem 0' }}>Страна: {university.country}</p>
                      <p style={{ margin: '0.25rem 0' }}>Экзамен: {university.exam} · {university.minScore ?? 'нет данных'}</p>
                      <p style={{ margin: '0.25rem 0' }}>Сложность: {university.level}</p>
                      <p style={{ margin: '0.25rem 0' }}>Специальности: {university.specialties.join(', ')}</p>
                      <p style={{ margin: '0.25rem 0' }}>Язык: {university.language}</p>
                      <p style={{ margin: '0.25rem 0' }}>Гранты: {university.grants}</p>
                      <p style={{ margin: '0.25rem 0' }}>Плюсы: {university.plus}</p>
                      <p style={{ margin: '0.25rem 0' }}>Минусы: {university.minus}</p>
                      <p style={{ margin: '0.25rem 0', color: 'var(--text-secondary)' }}>Сроки подачи: {university.deadlines}</p>
                    </div>
                  ))}
                </div>
              )}

              {result.goals.length > 0 && (
                <div style={{ marginBottom: '1.5rem' }}>
                  <h2 style={{ marginBottom: '0.75rem' }}>Цели на вырост</h2>
                  {result.goals.slice(0, 5).map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <p style={{ margin: '0.25rem 0' }}>Мин. балл: {university.minScore}</p>
                      <p style={{ margin: '0.25rem 0' }}>Специальности: {university.specialties.join(', ')}</p>
                      <p style={{ margin: '0.25rem 0' }}>Язык: {university.language}</p>
                      <p style={{ margin: '0.25rem 0' }}>Плюсы: {university.plus}</p>
                      <p style={{ margin: '0.25rem 0' }}>Минусы: {university.minus}</p>
                    </div>
                  ))}
                </div>
              )}

              {result.already.length === 0 && result.goals.length === 0 && result.filtered.length > 0 && (
                <div>
                  <h2 style={{ marginBottom: '0.75rem' }}>Рекомендации</h2>
                  {result.filtered.slice(0, 5).map((university) => (
                    <div key={university.id} className="card" style={{ marginBottom: '1rem' }}>
                      <h3 style={{ margin: '0 0 0.5rem' }}>{university.name} — {university.city}</h3>
                      <p style={{ margin: '0.25rem 0' }}>Специальности: {university.specialties.join(', ')}</p>
                      <p style={{ margin: '0.25rem 0' }}>Язык: {university.language}</p>
                      <p style={{ margin: '0.25rem 0' }}>Плюсы: {university.plus}</p>
                      <p style={{ margin: '0.25rem 0' }}>Минусы: {university.minus}</p>
                    </div>
                  ))}
                </div>
              )}

              {exam === 'NUET' && result.filtered.length === 0 && (
                <div className="card" style={{ padding: '1rem' }}>
                  <p style={{ margin: 0 }}>
                    Рекомендации NUET пока общие. Смотрите ведущие вузы Казахстана: Назарбаев Университет, KBTU и другие.
                  </p>
                </div>
              )}

              {exam === 'SAT' && result.filtered.length === 0 && (
                <div className="card" style={{ padding: '1rem' }}>
                  <p style={{ margin: 0 }}>
                    Для выбранных фильтров сейчас доступны рекомендации в Японии. Если нужен SAT для других стран, данные ещё в разработке.
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
