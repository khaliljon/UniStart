import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';

function ReferralTermsPage() {
  const { t, locale } = useTranslation();

  return (
    <div style={{
      maxWidth: '800px',
      margin: '0 auto',
      padding: '2rem 1.5rem 4rem',
      color: 'var(--text-primary)',
    }}>
      <Link to="/landing" style={{ color: 'var(--primary-color)', textDecoration: 'none', fontSize: '0.9rem' }}>
        ← {t.legal.backToHome}
      </Link>

      <h1 style={{ margin: '1.5rem 0 0.5rem', fontSize: '2rem', fontWeight: 700 }}>
        {t.legal.referralTermsTitle}
      </h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', fontSize: '0.85rem' }}>
        {t.legal.lastUpdated}: {t.legal.referralTermsDate}
      </p>

      {locale === 'ru' ? <ContentRu /> : locale === 'kz' ? <ContentKz /> : <ContentEn />}
    </div>
  );
}

const sectionStyle: React.CSSProperties = { marginBottom: '2rem' };
const h2Style: React.CSSProperties = { fontSize: '1.25rem', fontWeight: 700, marginBottom: '0.75rem' };
const pStyle: React.CSSProperties = { lineHeight: 1.7, color: 'var(--text-secondary)', marginBottom: '0.5rem', fontSize: '0.95rem' };
const ulStyle: React.CSSProperties = { paddingLeft: '1.5rem', lineHeight: 1.7, color: 'var(--text-secondary)', fontSize: '0.95rem' };

function ContentRu() {
  return (
    <>
      <div style={sectionStyle}>
        <p style={pStyle}>
          Настоящее Партнёрское соглашение определяет условия участия в реферальной программе платформы UniStart
          и порядок выплат за привлечённых пользователей.
        </p>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>1. Условия вознаграждения</h2>
        <p style={pStyle}><strong>Тьюторы и школы:</strong></p>
        <ul style={ulStyle}>
          <li>За каждого приглашённого пользователя, который оформил подписку Pro — 500 ₸.</li>
          <li>Минимальная сумма для вывода: тьюторы — 5 000 ₸, школы — 10 000 ₸.</li>
        </ul>
        <p style={{ ...pStyle, marginTop: '0.75rem' }}><strong>Студенты:</strong></p>
        <ul style={ulStyle}>
          <li>За каждого друга, оформившего Pro — +5 дней Pro подписки.</li>
          <li>Ограничений на количество бонусных дней нет.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>2. Правила программы</h2>
        <ul style={ulStyle}>
          <li>Реферальный код выдаётся после активации в профиле.</li>
          <li>Приглашённый пользователь должен зарегистрироваться по вашей ссылке и оформить подписку Pro.</li>
          <li>Запрещено создание фиктивных аккаунтов для получения вознаграждений.</li>
          <li>Вознаграждения начисляются автоматически при оплате Pro приглашённым.</li>
          <li>Неиспользованные вознаграждения сгорают через 12 месяцев.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>3. Прекращение участия</h2>
        <ul style={ulStyle}>
          <li>Участник может выйти из программы в любое время.</li>
          <li>UniStart вправе приостановить участие при нарушении условий с уведомлением за 14 дней.</li>
          <li>При прекращении участия ранее начисленные невыплаченные вознаграждения аннулируются.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>4. Защита данных</h2>
        <p style={pStyle}>
          Персональные данные обрабатываются в соответствии с{' '}
          <Link to="/privacy" style={{ color: 'var(--primary-color)' }}>Политикой конфиденциальности</Link>.
        </p>
      </div>

      <div style={sectionStyle}>
        <p style={pStyle}>
          Контакт: <a href="mailto:unistart.kz@gmail.com" style={{ color: 'var(--primary-color)' }}>unistart.kz@gmail.com</a>
        </p>
      </div>
    </>
  );
}

function ContentEn() {
  return (
    <>
      <div style={sectionStyle}>
        <p style={pStyle}>
          This Partner Agreement defines the terms of participation in the UniStart referral program
          and the payout conditions for referred users.
        </p>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>1. Reward Conditions</h2>
        <p style={pStyle}><strong>Tutors and schools:</strong></p>
        <ul style={ulStyle}>
          <li>For each referred user who subscribes to Pro — 500 ₸.</li>
          <li>Minimum withdrawal: tutors — 5,000 ₸, schools — 10,000 ₸.</li>
        </ul>
        <p style={{ ...pStyle, marginTop: '0.75rem' }}><strong>Students:</strong></p>
        <ul style={ulStyle}>
          <li>For each friend who subscribes to Pro — +5 Pro days.</li>
          <li>No limit on bonus days.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>2. Program Rules</h2>
        <ul style={ulStyle}>
          <li>A referral code is issued after activation in your profile.</li>
          <li>The referred user must register via your link and subscribe to Pro.</li>
          <li>Creating fake accounts to earn rewards is prohibited.</li>
          <li>Rewards are credited automatically when the referred user pays for Pro.</li>
          <li>Unused rewards expire after 12 months.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>3. Termination</h2>
        <ul style={ulStyle}>
          <li>You may leave the program at any time.</li>
          <li>UniStart may suspend participation for violations with 14 days notice.</li>
          <li>Upon termination, unpaid rewards are forfeited.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>4. Data Protection</h2>
        <p style={pStyle}>
          Personal data is processed in accordance with our{' '}
          <Link to="/privacy" style={{ color: 'var(--primary-color)' }}>Privacy Policy</Link>.
        </p>
      </div>

      <div style={sectionStyle}>
        <p style={pStyle}>
          Contact: <a href="mailto:unistart.kz@gmail.com" style={{ color: 'var(--primary-color)' }}>unistart.kz@gmail.com</a>
        </p>
      </div>
    </>
  );
}

function ContentKz() {
  return (
    <>
      <div style={sectionStyle}>
        <p style={pStyle}>
          Осы Серіктестік келісім UniStart реферал бағдарламасына қатысу шарттарын
          және тартылған пайдаланушылар үшін төлем тәртібін анықтайды.
        </p>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>1. Сыйақы шарттары</h2>
        <p style={pStyle}><strong>Тьюторлар мен мектептер:</strong></p>
        <ul style={ulStyle}>
          <li>Pro жазылымын рәсімдеген әр шақырылған пайдаланушы үшін — 500 ₸.</li>
          <li>Шығару минимумы: тьюторлар — 5 000 ₸, мектептер — 10 000 ₸.</li>
        </ul>
        <p style={{ ...pStyle, marginTop: '0.75rem' }}><strong>Студенттер:</strong></p>
        <ul style={ulStyle}>
          <li>Pro рәсімдеген әр дос үшін — +5 Pro күн.</li>
          <li>Бонус күндерге шектеу жоқ.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>2. Бағдарлама ережелері</h2>
        <ul style={ulStyle}>
          <li>Реферал коды профильде белсендіргеннен кейін беріледі.</li>
          <li>Шақырылған пайдаланушы сіздің сілтемеңіз арқылы тіркеліп, Pro жазылуы керек.</li>
          <li>Сыйақы алу үшін жалған аккаунттар жасау тыйым салынады.</li>
          <li>Сыйақылар шақырылған адам Pro төлегенде автоматты түрде есептеледі.</li>
          <li>Пайдаланылмаған сыйақылар 12 айдан кейін жойылады.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>3. Қатысуды тоқтату</h2>
        <ul style={ulStyle}>
          <li>Қатысушы кез келген уақытта бағдарламадан шыға алады.</li>
          <li>UniStart шарттарды бұзған жағдайда 14 күн мерзімде хабарлай отырып қатысуды тоқтата алады.</li>
          <li>Тоқтатылған кезде есептелген, бірақ төленбеген сыйақылар жойылады.</li>
        </ul>
      </div>

      <div style={sectionStyle}>
        <h2 style={h2Style}>4. Деректерді қорғау</h2>
        <p style={pStyle}>
          Жеке деректер біздің{' '}
          <Link to="/privacy" style={{ color: 'var(--primary-color)' }}>Құпиялылық саясатымызға</Link> сәйкес өңделеді.
        </p>
      </div>

      <div style={sectionStyle}>
        <p style={pStyle}>
          Байланыс: <a href="mailto:unistart.kz@gmail.com" style={{ color: 'var(--primary-color)' }}>unistart.kz@gmail.com</a>
        </p>
      </div>
    </>
  );
}

export default ReferralTermsPage;
