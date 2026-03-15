# UniStart — Sprint 3: CSCA Question Seeding Plan

## Обзор
Создание вопросов с 4 вариантами ответа для CSCA Математика (20 глав) на основе учебника.
Источники: `QuestionsCscaMath.md` (8774 строк), `AnswersCscaMath.md` (2326 строк).

---

## Фаза 1 — Обновление структуры CSCA (5 секций) ✅

- [x] Добавить 2 новые ExamSection: **Chinese Technical (中文技术)**, **Chinese Humanitarian (中文人文)**
- [x] Добавить навыки `SK_CN_TECH`, `SK_CN_HUM` в таблицу Skills
- [x] Обновить название ExamType CSCA → "CSCA (Mathematics + Physics + Chemistry + Chinese Technical + Chinese Humanitarian)"

---

## Фаза 2 — Сидинг вопросов по математике ✅

### Подход
- Вопросы уже имеющие формат A/B/C/D → прямое использование
- Fill-in-blank / расчётные → конвертация в 4-вариантный формат с правдоподобными дистракторами
- True/False → формат "Какое утверждение верно/неверно?"
- Сложность: Easy (базовые), Medium (стандартные), Hard (olympiad-уровень, proof, multi-step)

### Файлы
- [x] `Infrastructure/Data/CscaMathQuestionSeeder.cs` — создан, ~182 вопроса по 20 главам
- [x] Wired в `DatabaseSeeder.SeedAsync()` после `SeedQuestionsAsync()`

### Главы 1-5 (~60 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 1 | Множества (определения, операции) | 11 | Easy-Medium |
| 2 | Неравенства (линейные, квадратичные, дробные) | 12 | Easy-Hard |
| 3 | Функции (область, чётность, виды) | 15 | Easy-Hard |
| 4 | Тригонометрия (единичная окружность, графики) | 14 | Medium-Hard |
| 5 | Обратные тригонометрические функции | 6 | Easy-Medium |

### Главы 6-10 (~39 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 6 | Формулы сложения/удвоения | 8 | Easy-Medium |
| 7 | Последовательности (арифм., геом.) | 7 | Easy-Hard |
| 8 | Комплексные числа | 7 | Easy-Medium |
| 9 | Прямые на плоскости | 7 | Easy-Medium |
| 10 | Конические сечения (окружность, эллипс, гипербола, парабола) | 9 | Easy-Hard |

### Главы 11-15 (~40 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 11 | Векторы на плоскости | 9 | Easy-Medium |
| 12 | Пространственные векторы | 6 | Easy-Medium |
| 13 | Пространственные плоскости и прямые | 6 | Easy-Medium |
| 14 | Пределы последовательностей и функций | 9 | Easy-Hard |
| 15 | Производные (определение, правила) | 10 | Easy-Medium |

### Главы 16-20 (~36 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 16 | Применение производных | 7 | Medium |
| 17 | Перестановки и сочетания | 9 | Easy-Medium |
| 18 | Вероятность (классическая, условная) | 10 | Easy-Hard |
| 19 | Случайные величины, E(X), D(X) | 10 | Easy-Medium |
| 20 | Статистика (выборка, среднее, дисперсия) | 10 | Easy-Medium |

**Итого: ~182 вопроса** (покрыты все 20 глав; можно расширить до ~550 в будущем)

---

## Фаза 3 — Верификация ✅

- [x] `dotnet build` — 0 ошибок, 0 предупреждений
- [x] `cd client && npx tsc --noEmit` — 0 ошибок
- [ ] Запуск приложения, проверка что вопросы загружаются
- [ ] Проверка корректности ответов по AnswersCscaMath.md

---

## Будущие задачи (Спринт 4+)

- [ ] Сидинг вопросов по Физике (12 глав)
- [ ] Сидинг вопросов по Химии
- [ ] Контент для Chinese Technical (中文技术)
- [ ] Контент для Chinese Humanitarian (中文人文)
- [ ] Улучшение взаимодействия Тьютор ↔ Студент
- [ ] Новые фичи (аналитика, адаптивность)

---

## Sprint 5 — Монетизация & UX

### FIX-29 ✅ Фильтрация секций предикшена
### FIX-30 ✅ 3-дневный trial период
### FIX-31 ✅ Аналитика: free/pro split (ProGate)
### FIX-32 ✅ Работа над ошибками — лимит по тарифу
### FIX-33 ✅ Предикшен — blur для Free
### FIX-34 ✅ Профиль: убраны уроки, кнопка апгрейда → PricingModal
### FIX-35 ✅ PricingModal: цена 10 000 ₸, годовая подписка 99 990 ₸
### FIX-36 ✅ i18n: обновлены ключи (3 локали) + LandingPage

---

## FIX-37 — Бесплатный пробный мок-экзамен + интерактивный upsell

### Концепция
После регистрации пользователь получает **1 бесплатный полноценный мок-экзамен** (реальный сценарий: все секции, ограничение по времени, настоящие вопросы). После завершения показывается **интерактивный upsell-модал** с превью Pro-функций.

### Backend

#### 1. User entity — новое поле
```csharp
// Domain/Entities/User.cs
public bool FreeMockUsed { get; set; } = false;
```

#### 2. EF Migration
```
dotnet ef migrations add AddFreeMockUsed
```

#### 3. SubscriptionService — логика доступа
В `HasAccessAsync` изменить кейс `mock_exams`:
```csharp
"mock_exams" => limits.MockExamsEnabled || !user.FreeMockUsed,
```
Результат: Free-пользователь с `FreeMockUsed == false` → доступ разрешён.

#### 4. SubscriptionStatusDto — новое поле
```csharp
public record SubscriptionStatusDto(
    ...
    bool FreeMockAvailable  // !user.FreeMockUsed && !isPro
);
```

#### 5. MockExamController / MockExamService — пометка использования
При **завершении** мок-экзамена (endpoint `POST /api/mock-exams/attempts/{id}/complete`):
```csharp
if (!user.IsPro && !user.FreeMockUsed)
{
    user.FreeMockUsed = true;
    await _unitOfWork.SaveChangesAsync();
}
```

### Frontend

#### 6. MockExamPage.tsx — разрешить 1 бесплатный мок
Заменить `ProGate hasAccess={isPro}` на:
```tsx
const canAccessMock = isPro || subscriptionStatus?.freeMockAvailable;
<ProGate hasAccess={canAccessMock} featureName="Mock Exams">
```

#### 7. MockResultUpsellModal — интерактивный upsell после бесплатного мока
Новый компонент `client/src/components/MockResultUpsellModal.tsx`.
Показывается **только после завершения бесплатного мока** (не для Pro).

**Экраны (шаги):**
1. **Результаты** — «Твой балл: X/Y. Хочешь узнать, где ты потерял баллы?»
   - Кнопка: «Показать аналитику» → переход к шагу 2
2. **Превью аналитики** — Размытый radar-чарт + heatmap с текстом:
   «С Pro ты увидишь полную аналитику: сильные/слабые стороны, прогресс по темам»
   - Кнопка: «А что ещё есть?» → шаг 3
3. **Работа над ошибками** — Превью списка ошибок (1-2 видны, остальные blur):
   «Разбирай каждую ошибку с объяснением. Доступно в Pro.»
   - Кнопка: «Хочу попробовать Pro» → шаг 4
4. **CTA** — PricingModal (месяц/год) со скидкой:
   «Специальное предложение: первый месяц -50%» (5 000 ₸ вместо 10 000 ₸)

#### 8. i18n ключи
Добавить в `mockUpsell` секцию (ru/kz/en):
```
mockUpsell: {
  title: 'Мок завершён!',
  scoreText: 'Твой результат',
  wantAnalytics: 'Хочешь узнать, где потерял баллы?',
  showAnalytics: 'Показать аналитику',  
  analyticsPreview: 'С Pro — полная аналитика',
  whatElse: 'А что ещё есть?',
  mistakesPreview: 'Разбирай каждую ошибку',
  tryPro: 'Хочу попробовать Pro',
  specialOffer: 'Первый месяц -50%',
}
```

### Порядок реализации
1. [ ] Backend: добавить `FreeMockUsed` в User entity
2. [ ] Backend: миграция `AddFreeMockUsed`
3. [ ] Backend: `HasAccessAsync` — разрешить mock_exams при !FreeMockUsed
4. [ ] Backend: `SubscriptionStatusDto` + `FreeMockAvailable`
5. [ ] Backend: пометить FreeMockUsed при завершении мока
6. [ ] Frontend: MockExamPage — `canAccessMock` вместо `isPro`
7. [ ] Frontend: `MockResultUpsellModal` — 4-шаговый интерактив
8. [ ] Frontend: i18n ключи (ru/kz/en)
9. [ ] Тестирование: регистрация → мок → upsell → повторный вход → gate
