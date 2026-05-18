export type AdvisorExam = 'SAT' | 'NUET';

export type AdvisorUniversity = {
  id: string;
  name: string;
  city: string;
  country: string;
  exam: AdvisorExam;
  minScore: number | null;
  level: string;
  specialties: string[];
  language: string;
  grants: string;
  plus: string;
  minus: string;
  acceptance?: string;
  deadlines?: string;
  ranking?: string;
  procedure?: string;
  videoUrl?: string;
  photoUrl?: string;
  description?: string;
  lat?: number;
  lng?: number;
};

export type AdvisorCategory = {
  key: string;
  label: string;
  specialties: string[];
};

export type AdvisorConfig = {
  categories: AdvisorCategory[];
  countries: string[];
  languages: string[];
  universities: AdvisorUniversity[];
};

const DEFAULT_CATEGORIES: AdvisorCategory[] = [
  {
    key: 'education',
    label: 'Образование',
    specialties: ['English Language Teaching', 'Education'],
  },
  {
    key: 'arts',
    label: 'Искусство и гуманитарные науки',
    specialties: ['Culture', 'Media', 'Japan Studies', 'Arts', 'Humanities', 'Linguistics', 'Philosophy', 'History', 'Literature', 'Sociology', 'Anthropology', 'Psychology'],
  },
  {
    key: 'social',
    label: 'Социальные науки и журналистика',
    specialties: ['International Relations', 'Global Studies', 'Journalism', 'Economics', 'Management', 'Marketing'],
  },
  {
    key: 'business',
    label: 'Бизнес, управление, право',
    specialties: ['Management', 'Marketing', 'Accounting', 'Entrepreneurship', 'Business', 'Law', 'Economics'],
  },
  {
    key: 'science',
    label: 'Естественные науки',
    specialties: ['Math', 'Physics', 'Chemistry', 'Biology'],
  },
  {
    key: 'it',
    label: 'IT / ICT',
    specialties: ['Computer Science', 'Information and Communication', 'Computing'],
  },
  {
    key: 'engineering',
    label: 'Инженерия',
    specialties: ['Mechanical Engineering', 'Electrical Engineering', 'Civil Engineering', 'Environmental Design'],
  },
  {
    key: 'agriculture',
    label: 'Сельское хозяйство',
    specialties: ['Applied Biological Sciences', 'Environmental Sciences', 'Bioresource Sciences', 'Agriculture'],
  },
  {
    key: 'health',
    label: 'Здравоохранение',
    specialties: ['Medicine', 'Pharmacy', 'Nursing', 'Dentistry'],
  },
  {
    key: 'services',
    label: 'Сервисные отрасли',
    specialties: ['Tourism', 'Sustainability'],
  },
];

const DEFAULT_COUNTRIES = [
  'Japan',
  'Kazakhstan',
  'USA',
  'Canada',
  'Singapore',
  'UAE',
  'Korea',
  'Hong Kong',
  'Europe',
];

const DEFAULT_LANGUAGES = ['English', 'Japanese', 'English / Japanese', 'Russian', 'Other'];

const DEFAULT_UNIVERSITIES: AdvisorUniversity[] = [
  {
    id: 'apucampus',
    name: 'Ritsumeikan APU',
    city: 'Beppu',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['International Relations', 'Media', 'Culture', 'Management', 'Marketing', 'Accounting', 'Entrepreneurship', 'Tourism', 'Global Studies', 'English Language Teaching'],
    language: 'English',
    grants: 'Есть международные гранты и стипендии.',
    plus: 'Сильный международный кампус, много иностранных студентов.',
    minus: 'Расположен не в Токио.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
    ranking: 'Топ частных вузов Японии',
    procedure: 'Обычная подача через сайт университета.',
    photoUrl: '',
    lat: 33.2846,
    lng: 131.4909,
  },
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
    ranking: 'Хороший региональный рейтинг',
    procedure: 'Подача через онлайн-портал.',
    photoUrl: '',
    lat: 35.6895,
    lng: 139.6917,
  },
  {
    id: 'kanazawa',
    name: 'Kanazawa University',
    city: 'Kanazawa',
    country: 'Japan',
    exam: 'SAT',
    minScore: 1300,
    level: 'Несложно',
    specialties: ['Education', 'Law', 'Medicine', 'Pharmacy', 'Nursing', 'Mechanical Engineering', 'Electrical Engineering', 'Environmental Design', 'Information and Communication'],
    language: 'English / Japanese',
    grants: 'Есть гранты MEXT и академические стипендии.',
    plus: 'Национальный вуз, медицина и фармация.',
    minus: 'Провинциальный город.',
    acceptance: 'нет данных',
    deadlines: 'Смотри официальный сайт университета',
    ranking: 'Национальный университет',
    procedure: 'Сбор документов и подача через международный офис.',
    photoUrl: '',
    lat: 36.5611,
    lng: 136.6601,
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
    ranking: 'Международный частный вуз',
    procedure: 'Подача документов через отдел международных программ.',
    photoUrl: '',
    lat: 39.7186,
    lng: 140.1023,
  },
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
    ranking: 'Топ вузов Казахстана',
    procedure: 'Подача через NUG портал.',
    photoUrl: '',
    lat: 51.1804,
    lng: 71.4460,
  },
  {
    id: 'kbtu',
    name: 'KBTU',
    city: 'Almaty',
    country: 'Kazakhstan',
    exam: 'NUET',
    minScore: null,
    level: 'Средне',
    specialties: ['Engineering', 'Economics', 'Business', 'Computer Science'],
    language: 'English / Russian',
    grants: 'Есть университетские гранты.',
    plus: 'Сильный инженерно-экономический профиль.',
    minus: 'Конкуренция на лучшие программы.',
    acceptance: 'нет данных',
    deadlines: 'Официальный сайт KBTU',
    ranking: 'Известен техническими специальностями',
    procedure: 'Подача через приемную комиссию.',
    photoUrl: '',
    lat: 43.2565,
    lng: 76.9285,
  },
];

export const DEFAULT_ADVISOR_CONFIG: AdvisorConfig = {
  categories: DEFAULT_CATEGORIES,
  countries: DEFAULT_COUNTRIES,
  languages: DEFAULT_LANGUAGES,
  universities: DEFAULT_UNIVERSITIES,
};

const STORAGE_KEY = 'unistart_advisor_config';

export function loadAdvisorConfig(): AdvisorConfig {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw) as AdvisorConfig;
      if (parsed?.categories && parsed?.countries && parsed?.languages && parsed?.universities) {
        return parsed;
      }
    }
  } catch {
    // ignore invalid storage data
  }
  return DEFAULT_ADVISOR_CONFIG;
}

export function saveAdvisorConfig(config: AdvisorConfig) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(config));
}
