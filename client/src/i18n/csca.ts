// Standalone i18n strings for the CSCA landing page.
// Kept separate from the main Translations interface so all three locales
// stay in parity in one file and the huge types.ts is untouched.

import type { Locale } from './types';

export interface CscaStrings {
  // nav
  navAbout: string;
  navNews: string;
  navCourses: string;
  navMaterials: string;
  navMocks: string;
  navAboutUs: string;
  navContacts: string;
  login: string;
  cabinet: string;
  // hero
  heroBadge: string;
  heroTitle: string;
  heroTitleAccent: string;
  heroSub: string;
  ctaStart: string;
  ctaLearnMore: string;
  // countdown
  cdNextExam: string;
  cdRegOpens: string;
  cdDays: string;
  cdHours: string;
  cdMinutes: string;
  // stats
  statStudents: string;
  statQuestions: string;
  statAnswered: string;
  statSuccess: string;
  // about exam
  aboutTitle: string;
  aboutLead: string;
  aboutBody: string;
  subjectsTitle: string;
  subjectsLead: string;
  subjChinese: string;
  subjChineseTag: string;
  subjChineseTech: string;
  subjChineseTechTag: string;
  subjChineseHum: string;
  subjChineseHumTag: string;
  subjMath: string;
  subjMathTag: string;
  subjPhysics: string;
  subjPhysicsTag: string;
  subjChemistry: string;
  subjChemistryTag: string;
  required: string;
  viewTopics: string;
  addToCalendar: string;
  calGoogle: string;
  calApple: string;
  // news
  newsTitle: string;
  newsLead: string;
  newsReadMore: string;
  newsReadLess: string;
  newsEmpty: string;
  // features
  featuresTitle: string;
  featuresLead: string;
  fAdaptiveT: string;
  fAdaptiveD: string;
  fAnalyticsT: string;
  fAnalyticsD: string;
  fMockT: string;
  fMockD: string;
  fMaterialsT: string;
  fMaterialsD: string;
  fAiT: string;
  fAiD: string;
  fPlanT: string;
  fPlanD: string;
  // mocks
  mocksTitle: string;
  mocksLead: string;
  freeMockTitle: string;
  freeMockDesc: string;
  getFree: string;
  pkgStart: string;
  pkgStartFor: string;
  pkgStandard: string;
  pkgStandardFor: string;
  pkgAdvanced: string;
  pkgAdvancedFor: string;
  pkgFull: string;
  pkgFullFor: string;
  popular: string;
  oneSubject: string;
  twoSubjects: string;
  threeSubjects: string;
  allSubjects: string;
  buy: string;
  pkgFeatAi: string;
  pkgFeatAnalytics: string;
  pkgFeatFull: string;
  // materials
  materialsTitle: string;
  materialsLead: string;
  bookLabel: string;
  addToCart: string;
  freePdfTitle: string;
  freePdfDesc: string;
  // cta
  ctaBandTitle: string;
  ctaBandDesc: string;
  ctaBandBtn: string;
  // footer
  footerDesc: string;
  footerPlatform: string;
  footerAbout: string;
  footerContacts: string;
  footerRights: string;
  footerTerms: string;
  footerPrivacy: string;
  // exam dates (About page)
  examDatesTitle: string;
  examDatesLead: string;
  regOpensLabel: string;
  nextExamLabel: string;
  statusCompleted: string;
  statusUpcoming: string;
  monthJanuary: string;
  monthMarch: string;
  monthJune: string;
  monthSeptember: string;
  monthNovember: string;
  // Courses page
  coursesLead2: string;
  coursesComingSoon: string;
  coursesFree: string;
  coursesPaid: string;
  coursesPriceNote: string;
  // About us page
  aboutUsTitle: string;
  aboutUsLead: string;
  aboutUsMission: string;
  aboutUsGoal: string;
  // Contacts page
  contactsTitle: string;
  contactsLead: string;
  contactsWriteUs: string;
  // checkout / purchases (internal)
  checkoutTitle: string;
  checkoutEmpty: string;
  checkoutBackHome: string;
  checkoutSuccess: string;
  checkoutError: string;
  checkoutStubNote: string;
  checkoutPay: string;
  purchasesTitle: string;
  purchasesEmpty: string;
  purchasesBrowse: string;
  currency: string;
  // packages / mocks / purchases (localized UI added later)
  perSubject: string;
  discountPackages: string;
  mockQuestions: string;
  mockTime: string;
  mockBest: string;
  mockSessions: string;
  mockRemaining: string;
  mockFreeBadge: string;
  mockSolvePaid: string;
  mockSolveFree: string;
  recentAttempts: string;
  thExam: string;
  thScore: string;
  thStatus: string;
  thDate: string;
  statusInProgress: string;
  review: string;
  viewBtn: string;
  resumeBtn: string;
  paymentDone: string;
  paymentDoneDesc: string;
  myMocks: string;
  goSolve: string;
  sessionHistory: string;
  statusPaid: string;
  myMaterialsTitle: string;
  downloadPdf: string;
  materialsEmptyOwned: string;
  minShort: string;
  firstMockFree: string;
  firstMockFreeDesc: string;
  mocksFullLead: string;
  freeMockBannerDesc: string;
  examInProgress: string;
  sectionLabel: string;
  abandon: string;
  noMocksAvailable: string;
  buyOnHome: string;
  mockRunsOut: string;
  pickSubjects: string;
  bought: string;
  boughtOpen: string;
  openBtn: string;
  singleMocksTitle: string;
  // cart (internal)
  cartTitle: string;
  cartEmpty: string;
  cartRemove: string;
  cartTotal: string;
  cartCheckout: string;
  continueShopping: string;
  cartBrowse: string;
  addedToCart: string;
}

const ru: CscaStrings = {
  navAbout: 'О CSCA', navNews: 'Новости', navCourses: 'Курсы', navMaterials: 'Материалы', navMocks: 'Пробные экзамены', navAboutUs: 'О нас', navContacts: 'Контакты',
  login: 'Войти', cabinet: 'Личный кабинет',
  heroBadge: 'Подготовка к экзамену CSCA',
  heroTitle: 'Твой путь в ведущие', heroTitleAccent: 'университеты Китая',
  heroSub: 'Системная подготовка к CSCA: пробные тесты с ИИ-объяснениями, официальные материалы и экспертное сопровождение на трёх языках.',
  ctaStart: 'Начать подготовку', ctaLearnMore: 'Узнать больше',
  cdNextExam: 'Ближайший экзамен', cdRegOpens: 'Регистрация открывается в мае 2026',
  cdDays: 'дней', cdHours: 'часов', cdMinutes: 'минут',
  statStudents: 'Студентов', statQuestions: 'Тем в базе', statAnswered: 'Решённых заданий', statSuccess: 'Довольных учеников',
  aboutTitle: 'Что такое CSCA?', aboutLead: 'China Scholastic Competency Assessment',
  aboutBody: 'CSCA — стандартизированный вступительный экзамен для иностранных абитуриентов, поступающих на бакалавриат в университеты Китая. С 2026 года он обязателен для большинства иностранных абитуриентов по государственным стипендиальным программам.',
  subjectsTitle: 'Предметы экзамена', subjectsLead: 'Готовься к любому предмету CSCA на выбранном языке.',
  subjChinese: 'Китайский язык', subjChineseTag: 'Professional Chinese',
  subjChineseTech: 'Технический китайский', subjChineseTechTag: 'Естественно-научный поток',
  subjChineseHum: 'Гуманитарный китайский', subjChineseHumTag: 'Гуманитарный поток',
  subjMath: 'Математика', subjMathTag: 'Обязательно для всех',
  subjPhysics: 'Физика', subjPhysicsTag: 'Технические направления',
  subjChemistry: 'Химия', subjChemistryTag: 'По специальности',
  required: 'Обязательно', viewTopics: 'Список тем',
  addToCalendar: 'Добавить в календарь',
  calGoogle: 'Google Календарь', calApple: 'Apple / Outlook (.ics)',
  newsTitle: 'Новости CSCA', newsLead: 'Актуальная и достоверная информация об экзамене, датах и поступлении.',
  newsReadMore: 'Читать', newsReadLess: 'Скрыть', newsEmpty: 'Пока новостей нет — скоро здесь появятся свежие материалы.',
  featuresTitle: 'Что Вы получите на платформе?', featuresLead: 'Всё для эффективной подготовки в одном месте.',
  fAdaptiveT: 'Адаптивное обучение', fAdaptiveD: 'Система подстраивается под ваш уровень и подтягивает слабые темы.',
  fAnalyticsT: 'Аналитика и прогресс', fAnalyticsD: 'Процент готовности и список проблемных тем по каждому предмету.',
  fMockT: 'Пробные тесты', fMockD: 'Реалистичные пробники в формате экзамена CSCA.',
  fMaterialsT: 'Учебные материалы', fMaterialsD: 'Официальные учебники и PDF по всем предметам на RU / EN / KZ.',
  fAiT: 'ИИ-объяснения', fAiD: 'Подробный разбор каждого задания с пошаговым решением.',
  fPlanT: 'План подготовки', fPlanD: 'Индивидуальный план до даты вашего экзамена.',
  mocksTitle: 'Пробные экзамены', mocksLead: 'Решай пробники и следи за прогрессом.',
  freeMockTitle: 'Получи пробный тест с объяснением бесплатно!', freeMockDesc: 'Зарегистрируйся — и получи пробники с ИИ-объяснениями и общим точным баллом с разбором проблемных тем.',
  getFree: 'Получить бесплатно',
  pkgStart: 'Start', pkgStartFor: 'Для знакомства с форматом',
  pkgStandard: 'Standard', pkgStandardFor: 'Самый популярный вариант',
  pkgAdvanced: 'Advanced', pkgAdvancedFor: 'Для большинства абитуриентов',
  pkgFull: 'Full CSCA', pkgFullFor: 'Полная подготовка',
  popular: 'Популярный',
  oneSubject: 'Любой 1 предмет', twoSubjects: 'Любые 2 предмета', threeSubjects: 'Любые 3 предмета', allSubjects: 'Все 5 предметов',
  buy: 'Купить',
  pkgFeatAi: 'ИИ-объяснения к заданиям', pkgFeatAnalytics: 'Дешборд аналитики', pkgFeatFull: 'Доступ ко всем пробникам предмета',
  materialsTitle: 'Официальные учебные материалы', materialsLead: 'Учебники разработаны с учётом актуального формата экзамена CSCA.',
  bookLabel: 'Учебник для подготовки к CSCA', addToCart: 'В корзину',
  freePdfTitle: 'Забери наши полезные материалы по подготовке к CSCA абсолютно бесплатно!', freePdfDesc: 'PDF по всем предметам — доступны сразу после регистрации.',
  ctaBandTitle: 'Готов начать подготовку к CSCA?', ctaBandDesc: 'Создай аккаунт и получай больше бесплатных материалов.',
  ctaBandBtn: 'Создать аккаунт бесплатно',
  footerDesc: 'Платформа подготовки к экзамену CSCA. Источник достоверной информации и онлайн-обучения для абитуриентов всего СНГ.',
  footerPlatform: 'Платформа', footerAbout: 'Об экзамене', footerContacts: 'Контакты',
  footerRights: 'Все права защищены.',
  footerTerms: 'Пользовательское соглашение',
  footerPrivacy: 'Политика конфиденциальности',
  examDatesTitle: 'Даты экзамена 2026',
  examDatesLead: 'CSCA проводится 5 раз в 2026 году. Проверьте даты и расписание ниже.',
  regOpensLabel: 'Регистрация открывается',
  nextExamLabel: 'Ближайший экзамен',
  statusCompleted: 'Завершён', statusUpcoming: 'Предстоит',
  monthJanuary: 'Январь', monthMarch: 'Март', monthJune: 'Июнь', monthSeptember: 'Сентябрь', monthNovember: 'Ноябрь',
  coursesLead2: 'Видеоуроки по всем предметам CSCA — бесплатные и платные.',
  coursesComingSoon: 'Скоро', coursesFree: 'Бесплатные уроки', coursesPaid: 'Полная подготовка',
  coursesPriceNote: 'Стоимость занятий — от 9 990 ₸ / месяц. Уточняйте актуальные пакеты у команды.',
  aboutUsTitle: 'О нас', aboutUsLead: 'Платформа подготовки к CSCA для абитуриентов всего СНГ.',
  aboutUsMission: 'Мы создаём достоверный источник информации и место для онлайн-подготовки к экзамену CSCA — с адаптивными тестами, аналитикой и экспертными материалами.',
  aboutUsGoal: 'Наша цель — стать №1 в подготовке к экзамену CSCA.',
  contactsTitle: 'Контакты', contactsLead: 'Свяжитесь с нами — мы всегда на связи.', contactsWriteUs: 'Напишите нам',
  checkoutTitle: 'Оформление заказа',
  checkoutEmpty: 'Нет выбранного товара. Вернитесь на главную и выберите пакет.',
  checkoutBackHome: 'На главную',
  checkoutSuccess: 'Заказ оформлен!',
  checkoutError: 'Не удалось оформить заказ. Попробуйте ещё раз.',
  checkoutStubNote: 'Оплата пока проводится в тестовом режиме — после подтверждения доступ откроется сразу.',
  checkoutPay: 'Оплатить',
  purchasesTitle: 'Мои покупки',
  purchasesEmpty: 'У вас пока нет покупок. Выберите пробник или учебник.',
  purchasesBrowse: 'Выбрать пакет',
  currency: '₸',
  perSubject: 'на каждый предмет',
  discountPackages: 'Пакеты со скидкой',
  mockQuestions: 'Вопросов',
  mockTime: 'Время',
  mockBest: 'Лучший',
  mockSessions: 'Сессий',
  mockRemaining: 'Осталось',
  mockFreeBadge: 'Бесплатный мок',
  mockSolvePaid: '▶ Решить',
  mockSolveFree: '▶ Решить бесплатно',
  recentAttempts: 'Недавние попытки',
  thExam: 'Экзамен',
  thScore: 'Балл',
  thStatus: 'Статус',
  thDate: 'Дата',
  statusInProgress: 'В процессе',
  review: 'Разбор',
  viewBtn: 'Открыть',
  resumeBtn: 'Продолжить',
  paymentDone: 'Оплата прошла',
  paymentDoneDesc: 'Доступ начислен. Моки — во вкладке «Пробные экзамены», учебники — в «Материалах».',
  myMocks: 'Мои моки',
  goSolve: 'Перейти к решению',
  sessionHistory: 'История сессий',
  statusPaid: 'Оплачено',
  myMaterialsTitle: 'Материалы',
  downloadPdf: '⇓ Скачать PDF',
  materialsEmptyOwned: 'У вас пока нет материалов. Приобретите учебники на Главной.',
  minShort: 'мин',
  firstMockFree: '🎁 Первый мок — бесплатно',
  firstMockFreeDesc: 'Начните любой пробник бесплатно во вкладке «Пробные экзамены». Дальше — покупка моков поштучно или пакетом.',
  mocksFullLead: 'Полноформатный пробник в реальных экзаменационных условиях с таймером по секциям',
  freeMockBannerDesc: 'Выберите любой пробник ниже и пройдите один мок бесплатно. Купить ещё моки можно на Главной.',
  examInProgress: 'Экзамен в процессе',
  sectionLabel: 'секция',
  abandon: 'Прервать',
  noMocksAvailable: 'У вас пока нет доступных пробников. Приобретите моки на Главной.',
  buyOnHome: 'Приобрести на Главной',
  mockRunsOut: 'Закончились — в каталог',
  pickSubjects: 'Выбрать предметы',
  bought: '✓ Куплено',
  boughtOpen: '✓ Куплено — открыть',
  openBtn: 'Открыть',
  singleMocksTitle: 'Отдельные пробники по предметам',
  cartTitle: 'Моя корзина',
  cartEmpty: 'Корзина пуста. Выберите пробник или учебник.',
  cartRemove: 'Удалить',
  cartTotal: 'Итого',
  cartCheckout: 'Оформить заказ',
  continueShopping: 'Продолжить покупки',
  cartBrowse: 'В каталог',
  addedToCart: 'Добавлено в корзину',
};

const en: CscaStrings = {
  navAbout: 'About CSCA', navNews: 'News', navCourses: 'Courses', navMaterials: 'Materials', navMocks: 'Mock exams', navAboutUs: 'About us', navContacts: 'Contacts',
  login: 'Sign in', cabinet: 'My account',
  heroBadge: 'CSCA exam preparation',
  heroTitle: 'Your path to top', heroTitleAccent: 'universities in China',
  heroSub: 'Structured CSCA preparation: mock tests with AI explanations, official study materials and expert guidance in three languages.',
  ctaStart: 'Start preparing', ctaLearnMore: 'Learn more',
  cdNextExam: 'Next exam', cdRegOpens: 'Registration opens May 2026',
  cdDays: 'days', cdHours: 'hours', cdMinutes: 'minutes',
  statStudents: 'Students', statQuestions: 'Topics in base', statAnswered: 'Questions solved', statSuccess: 'Happy students',
  aboutTitle: 'What is CSCA?', aboutLead: 'China Scholastic Competency Assessment',
  aboutBody: 'CSCA is a standardized entrance examination for international students applying to undergraduate programs at Chinese universities. Since 2026 it is mandatory for most international applicants under Chinese government scholarship schemes.',
  subjectsTitle: 'Exam subjects', subjectsLead: 'Prepare for any CSCA subject in your chosen language.',
  subjChinese: 'Chinese', subjChineseTag: 'Professional Chinese',
  subjChineseTech: 'Technical Chinese', subjChineseTechTag: 'STEM track',
  subjChineseHum: 'Humanities Chinese', subjChineseHumTag: 'Humanities track',
  subjMath: 'Mathematics', subjMathTag: 'Required for everyone',
  subjPhysics: 'Physics', subjPhysicsTag: 'STEM programs',
  subjChemistry: 'Chemistry', subjChemistryTag: 'By specialty',
  required: 'Required', viewTopics: 'Topic list',
  addToCalendar: 'Add to calendar',
  calGoogle: 'Google Calendar', calApple: 'Apple / Outlook (.ics)',
  newsTitle: 'CSCA news', newsLead: 'Up-to-date, reliable information about the exam, dates and admissions.',
  newsReadMore: 'Read', newsReadLess: 'Collapse', newsEmpty: 'No news yet — fresh materials will appear here soon.',
  featuresTitle: 'What You get on the platform?', featuresLead: 'Everything for effective preparation in one place.',
  fAdaptiveT: 'Adaptive learning', fAdaptiveD: 'The system adapts to your level and reinforces weak topics.',
  fAnalyticsT: 'Analytics & progress', fAnalyticsD: 'Readiness percentage and problem-topic list for each subject.',
  fMockT: 'Mock tests', fMockD: 'Realistic practice tests in the CSCA exam format.',
  fMaterialsT: 'Study materials', fMaterialsD: 'Official textbooks and PDFs for all subjects in RU / EN / KZ.',
  fAiT: 'AI explanations', fAiD: 'Detailed step-by-step breakdown of every question.',
  fPlanT: 'Study plan', fPlanD: 'A personal plan up to your exam date.',
  mocksTitle: 'Mock exams', mocksLead: 'Take practice tests and track your progress.',
  freeMockTitle: 'Get a mock test with explanations for free!', freeMockDesc: 'Sign up and get practice tests with AI explanations and an accurate overall score with a breakdown of problem topics.',
  getFree: 'Get for free',
  pkgStart: 'Start', pkgStartFor: 'To get to know the format',
  pkgStandard: 'Standard', pkgStandardFor: 'The most popular option',
  pkgAdvanced: 'Advanced', pkgAdvancedFor: 'For most applicants',
  pkgFull: 'Full CSCA', pkgFullFor: 'Complete preparation',
  popular: 'Popular',
  oneSubject: 'Any 1 subject', twoSubjects: 'Any 2 subjects', threeSubjects: 'Any 3 subjects', allSubjects: 'All 5 subjects',
  buy: 'Buy',
  pkgFeatAi: 'AI explanations for questions', pkgFeatAnalytics: 'Analytics dashboard', pkgFeatFull: 'Access to all subject mocks',
  materialsTitle: 'Official study materials', materialsLead: 'Textbooks designed for the current CSCA exam format.',
  bookLabel: 'CSCA preparation textbook', addToCart: 'Add to cart',
  freePdfTitle: 'Grab our helpful CSCA prep materials — absolutely free!', freePdfDesc: 'PDFs for all subjects — available right after registration.',
  ctaBandTitle: 'Ready to start preparing for CSCA?', ctaBandDesc: 'Create an account and get more free materials.',
  ctaBandBtn: 'Create a free account',
  footerDesc: 'A CSCA exam preparation platform. A trusted source of information and online learning for applicants across the CIS.',
  footerPlatform: 'Platform', footerAbout: 'About the exam', footerContacts: 'Contacts',
  footerRights: 'All rights reserved.',
  footerTerms: 'Terms of Service',
  footerPrivacy: 'Privacy Policy',
  examDatesTitle: 'CSCA exam dates 2026',
  examDatesLead: 'CSCA runs 5 times in 2026. Check the dates and schedule below.',
  regOpensLabel: 'Registration opens',
  nextExamLabel: 'Next exam',
  statusCompleted: 'Completed', statusUpcoming: 'Upcoming',
  monthJanuary: 'January', monthMarch: 'March', monthJune: 'June', monthSeptember: 'September', monthNovember: 'November',
  coursesLead2: 'Video lessons for every CSCA subject — free and paid.',
  coursesComingSoon: 'Coming soon', coursesFree: 'Free lessons', coursesPaid: 'Full preparation',
  coursesPriceNote: 'Lessons from 9,990 ₸ / month. Ask our team about current packages.',
  aboutUsTitle: 'About us', aboutUsLead: 'A CSCA preparation platform for applicants across the CIS.',
  aboutUsMission: 'We build a trusted source of information and a place for online CSCA preparation — with adaptive tests, analytics and expert materials.',
  aboutUsGoal: 'Our goal is to become #1 in CSCA exam preparation.',
  contactsTitle: 'Contacts', contactsLead: 'Get in touch — we are always available.', contactsWriteUs: 'Write to us',
  checkoutTitle: 'Checkout',
  checkoutEmpty: 'No item selected. Go back to the home page and pick a package.',
  checkoutBackHome: 'Back home',
  checkoutSuccess: 'Order placed!',
  checkoutError: 'Could not place the order. Please try again.',
  checkoutStubNote: 'Payment is currently in test mode — access opens right after confirmation.',
  checkoutPay: 'Pay',
  purchasesTitle: 'My purchases',
  purchasesEmpty: 'You have no purchases yet. Pick a mock or a textbook.',
  purchasesBrowse: 'Choose a package',
  currency: '₸',
  perSubject: 'per subject',
  discountPackages: 'Discount packages',
  mockQuestions: 'Questions',
  mockTime: 'Time',
  mockBest: 'Best',
  mockSessions: 'Sessions',
  mockRemaining: 'Left',
  mockFreeBadge: 'Free mock',
  mockSolvePaid: '▶ Start',
  mockSolveFree: '▶ Start for free',
  recentAttempts: 'Recent attempts',
  thExam: 'Exam',
  thScore: 'Score',
  thStatus: 'Status',
  thDate: 'Date',
  statusInProgress: 'In progress',
  review: 'Review',
  viewBtn: 'View',
  resumeBtn: 'Resume',
  paymentDone: 'Payment successful',
  paymentDoneDesc: 'Access granted. Mocks are in “Mock exams”, textbooks in “Materials”.',
  myMocks: 'My mocks',
  goSolve: 'Go to practice',
  sessionHistory: 'Session history',
  statusPaid: 'Paid',
  myMaterialsTitle: 'Materials',
  downloadPdf: '⇓ Download PDF',
  materialsEmptyOwned: 'You have no materials yet. Buy textbooks on the Home page.',
  minShort: 'min',
  firstMockFree: '🎁 First mock — free',
  firstMockFreeDesc: 'Start any mock for free in the “Mock exams” tab. After that — buy mocks individually or as a package.',
  mocksFullLead: 'A full-length mock under real exam conditions with a per-section timer',
  freeMockBannerDesc: 'Pick any mock below and take one for free. You can buy more mocks on the Home page.',
  examInProgress: 'Exam in progress',
  sectionLabel: 'section',
  abandon: 'Abort',
  noMocksAvailable: 'You have no available mocks yet. Buy mocks on the Home page.',
  buyOnHome: 'Buy on the Home page',
  mockRunsOut: 'Sold out — to catalog',
  pickSubjects: 'Choose subjects',
  bought: '✓ Purchased',
  boughtOpen: '✓ Purchased — open',
  openBtn: 'Open',
  singleMocksTitle: 'Individual mocks by subject',
  cartTitle: 'My cart',
  cartEmpty: 'Your cart is empty. Pick a mock or a textbook.',
  cartRemove: 'Remove',
  cartTotal: 'Total',
  cartCheckout: 'Checkout',
  continueShopping: 'Continue shopping',
  cartBrowse: 'Browse catalog',
  addedToCart: 'Added to cart',
};

const kz: CscaStrings = {
  navAbout: 'CSCA туралы', navNews: 'Жаңалықтар', navCourses: 'Курстар', navMaterials: 'Материалдар', navMocks: 'Сынақ емтихандары', navAboutUs: 'Біз туралы', navContacts: 'Байланыс',
  login: 'Кіру', cabinet: 'Жеке кабинет',
  heroBadge: 'CSCA емтиханына дайындық',
  heroTitle: 'Қытайдың жетекші', heroTitleAccent: 'университеттеріне жол',
  heroSub: 'CSCA-ға жүйелі дайындық: ЖИ түсіндірмелері бар сынақ тесттері, ресми материалдар және үш тілде сарапшы қолдауы.',
  ctaStart: 'Дайындықты бастау', ctaLearnMore: 'Толығырақ',
  cdNextExam: 'Жақын емтихан', cdRegOpens: 'Тіркеу 2026 жылдың мамырында ашылады',
  cdDays: 'күн', cdHours: 'сағат', cdMinutes: 'минут',
  statStudents: 'Студент', statQuestions: 'Базадағы тақырып', statAnswered: 'Шешілген тапсырма', statSuccess: 'Риза оқушы',
  aboutTitle: 'CSCA дегеніміз не?', aboutLead: 'China Scholastic Competency Assessment',
  aboutBody: 'CSCA — Қытай университеттерінің бакалавриатына түсетін шетелдік талапкерлерге арналған стандартталған қабылдау емтиханы. 2026 жылдан бастап ол мемлекеттік гранттық бағдарламалар бойынша талапкерлердің басым бөлігі үшін міндетті.',
  subjectsTitle: 'Емтихан пәндері', subjectsLead: 'Кез келген CSCA пәніне таңдаған тіліңізде дайындалыңыз.',
  subjChinese: 'Қытай тілі', subjChineseTag: 'Professional Chinese',
  subjChineseTech: 'Техникалық қытай тілі', subjChineseTechTag: 'Жаратылыстану ағыны',
  subjChineseHum: 'Гуманитарлық қытай тілі', subjChineseHumTag: 'Гуманитарлық ағын',
  subjMath: 'Математика', subjMathTag: 'Барлығына міндетті',
  subjPhysics: 'Физика', subjPhysicsTag: 'Техникалық бағыттар',
  subjChemistry: 'Химия', subjChemistryTag: 'Мамандық бойынша',
  required: 'Міндетті', viewTopics: 'Тақырыптар тізімі',
  addToCalendar: 'Күнтізбеге қосу',
  calGoogle: 'Google Күнтізбе', calApple: 'Apple / Outlook (.ics)',
  newsTitle: 'CSCA жаңалықтары', newsLead: 'Емтихан, күндер және қабылдау туралы өзекті ақпарат.',
  newsReadMore: 'Оқу', newsReadLess: 'Жабу', newsEmpty: 'Әзірше жаңалық жоқ — жақында жаңа материалдар пайда болады.',
  featuresTitle: 'Платформадан не аласыз?', featuresLead: 'Тиімді дайындыққа қажеттінің бәрі бір жерде.',
  fAdaptiveT: 'Бейімделетін оқу', fAdaptiveD: 'Жүйе деңгейіңізге бейімделіп, әлсіз тақырыптарды нығайтады.',
  fAnalyticsT: 'Аналитика және прогресс', fAnalyticsD: 'Әр пән бойынша дайындық пайызы мен проблемалық тақырыптар тізімі.',
  fMockT: 'Сынақ тесттері', fMockD: 'CSCA емтихан форматындағы шынайы сынақтар.',
  fMaterialsT: 'Оқу материалдары', fMaterialsD: 'Барлық пәндер бойынша ресми оқулықтар мен PDF: RU / EN / KZ.',
  fAiT: 'ЖИ түсіндірмелері', fAiD: 'Әр тапсырманың қадамдық шешімімен толық талдауы.',
  fPlanT: 'Дайындық жоспары', fPlanD: 'Емтихан күніне дейінгі жеке жоспар.',
  mocksTitle: 'Сынақ емтихандары', mocksLead: 'Сынақтарды шешіп, прогресіңді қадағала.',
  freeMockTitle: 'Түсіндірмесі бар сынақ тестін тегін алыңыз!', freeMockDesc: 'Тіркеліңіз — ЖИ түсіндірмелері мен проблемалық тақырыптар талдауы қосылған жалпы дәл баллы бар сынақтарды алыңыз.',
  getFree: 'Тегін алу',
  pkgStart: 'Start', pkgStartFor: 'Форматпен танысу үшін',
  pkgStandard: 'Standard', pkgStandardFor: 'Ең танымал нұсқа',
  pkgAdvanced: 'Advanced', pkgAdvancedFor: 'Талапкерлердің көбіне',
  pkgFull: 'Full CSCA', pkgFullFor: 'Толық дайындық',
  popular: 'Танымал',
  oneSubject: 'Кез келген 1 пән', twoSubjects: 'Кез келген 2 пән', threeSubjects: 'Кез келген 3 пән', allSubjects: 'Барлық 5 пән',
  buy: 'Сатып алу',
  pkgFeatAi: 'Тапсырмаларға ЖИ түсіндірмелері', pkgFeatAnalytics: 'Аналитика тақтасы', pkgFeatFull: 'Пәннің барлық сынақтарына қолжетімділік',
  materialsTitle: 'Ресми оқу материалдары', materialsLead: 'Оқулықтар CSCA емтиханының өзекті форматына сай әзірленген.',
  bookLabel: 'CSCA-ға дайындық оқулығы', addToCart: 'Себетке',
  freePdfTitle: 'CSCA-ға дайындық материалдарымызды тап-такыр тегін алыңыз!', freePdfDesc: 'Барлық пәндер бойынша PDF — тіркелгеннен кейін бірден қолжетімді.',
  ctaBandTitle: 'CSCA-ға дайындықты бастауға дайынсыз ба?', ctaBandDesc: 'Аккаунт ашып, көбірек тегін материалдар алыңыз.',
  ctaBandBtn: 'Тегін аккаунт ашу',
  footerDesc: 'CSCA емтиханына дайындық платформасы. Бүкіл ТМД талапкерлеріне арналған сенімді ақпарат пен онлайн оқыту көзі.',
  footerPlatform: 'Платформа', footerAbout: 'Емтихан туралы', footerContacts: 'Байланыс',
  footerRights: 'Барлық құқықтар қорғалған.',
  footerTerms: 'Пайдаланушы келісімі',
  footerPrivacy: 'Құпиялық саясаты',
  examDatesTitle: '2026 CSCA емтихан күндері',
  examDatesLead: 'CSCA 2026 жылы 5 рет өтеді. Төмендегі күндер мен кестені қараңыз.',
  regOpensLabel: 'Тіркеу ашылады',
  nextExamLabel: 'Жақын емтихан',
  statusCompleted: 'Аяқталды', statusUpcoming: 'Алдағы',
  monthJanuary: 'Қаңтар', monthMarch: 'Наурыз', monthJune: 'Маусым', monthSeptember: 'Қыркүйек', monthNovember: 'Қараша',
  coursesLead2: 'CSCA барлық пәндері бойынша бейнесабақтар — тегін және ақылы.',
  coursesComingSoon: 'Жақында', coursesFree: 'Тегін сабақтар', coursesPaid: 'Толық дайындық',
  coursesPriceNote: 'Сабақ құны — айына 9 990 ₸ бастап. Өзекті пакеттерді командадан сұраңыз.',
  aboutUsTitle: 'Біз туралы', aboutUsLead: 'Бүкіл ТМД талапкерлеріне арналған CSCA дайындық платформасы.',
  aboutUsMission: 'Біз CSCA емтиханына онлайн дайындыққа арналған сенімді ақпарат көзін құрамыз — бейімделетін тесттер, аналитика және сараптамалық материалдармен.',
  aboutUsGoal: 'Біздің мақсатымыз — CSCA емтиханына дайындықта №1 болу.',
  contactsTitle: 'Байланыс', contactsLead: 'Бізбен байланысыңыз — біз әрқашан байланыстамыз.', contactsWriteUs: 'Бізге жазыңыз',
  checkoutTitle: 'Тапсырысты растау',
  checkoutEmpty: 'Таңдалған тауар жоқ. Басты бетке оралып, пакет таңдаңыз.',
  checkoutBackHome: 'Басты бетке',
  checkoutSuccess: 'Тапсырыс расталды!',
  checkoutError: 'Тапсырысты растау сәтсіз аяқталды. Қайта көріңіз.',
  checkoutStubNote: 'Төлем әзірше тестілік режимде — растағаннан кейін қолжетімділік бірден ашылады.',
  checkoutPay: 'Төлеу',
  purchasesTitle: 'Сатып алуларым',
  purchasesEmpty: 'Сізде әзірше сатып алулар жоқ. Сынақ немесе оқулық таңдаңыз.',
  purchasesBrowse: 'Пакет таңдау',
  currency: '₸',  perSubject: 'әр пәнге',
  discountPackages: 'Жеңілдікпен пакеттер',
  mockQuestions: 'Сұрақтар',
  mockTime: 'Уақыт',
  mockBest: 'Ең жақсы',
  mockSessions: 'Сессиялар',
  mockRemaining: 'Қалды',
  mockFreeBadge: 'Тегін мок',
  mockSolvePaid: '▶ Шешу',
  mockSolveFree: '▶ Тегін шешу',
  recentAttempts: 'Соңғы әрекеттер',
  thExam: 'Емтихан',
  thScore: 'Ұпай',
  thStatus: 'Күйі',
  thDate: 'Күні',
  statusInProgress: 'Орындалуда',
  review: 'Талдау',
  viewBtn: 'Ашу',
  resumeBtn: 'Жалғастыру',
  paymentDone: 'Төлем сәтті өтті',
  paymentDoneDesc: 'Қолжетімділік берілді. Моктар — «Сынақ емтихандар», оқулықтар — «Материалдар» бөлімінде.',
  myMocks: 'Менің моктарым',
  goSolve: 'Шешуге өту',
  sessionHistory: 'Сессиялар тарихы',
  statusPaid: 'Төленді',
  myMaterialsTitle: 'Материалдар',
  downloadPdf: '⇓ PDF жүктеу',
  materialsEmptyOwned: 'Сізде әзірше материалдар жоқ. Оқулықтарды Басты беттен сатып алыңыз.',
  minShort: 'мин',
  firstMockFree: '🎁 Алғашқы мок — тегін',
  firstMockFreeDesc: '«Сынақ емтихандар» бөлімінде кез келген пробникті тегін бастаңыз. Ары қарай — моктарды даналап немесе пакетпен сатып алу.',
  mocksFullLead: 'Нақты емтихан жағдайында бөлімдер бойынша таймермен толық сынақ',
  freeMockBannerDesc: 'Төмендегі кез келген пробникті таңдап, бір мокты тегін тапсырыңыз. Көбірек мок сатып алуды Басты беттен жасай аласыз.',
  examInProgress: 'Емтихан орындалуда',
  sectionLabel: 'бөлім',
  abandon: 'Тоқтату',
  noMocksAvailable: 'Сізде әзірше қолжетімді пробниктер жоқ. Моктарды Басты беттен сатып алыңыз.',
  buyOnHome: 'Басты беттен сатып алу',
  mockRunsOut: 'Бітті — каталогқа',
  pickSubjects: 'Пәндерді таңдау',
  bought: '✓ Сатып алынды',
  boughtOpen: '✓ Сатып алынды — ашу',
  openBtn: 'Ашу',
  singleMocksTitle: 'Пәндер бойынша жеке пробниктер',  cartTitle: 'Менің себетім',
  cartEmpty: 'Себет бос. Сынақ немесе оқулық таңдаңыз.',
  cartRemove: 'Жою',
  cartTotal: 'Барлығы',
  cartCheckout: 'Тапсырыс беру',
  continueShopping: 'Сатып алуды жалғастыру',
  cartBrowse: 'Каталогқа',
  addedToCart: 'Себетке қосылды',
};

export const cscaStrings: Record<Locale, CscaStrings> = { ru, en, kz };
