# 🎨 План улучшения стилизации UniStart

## 📋 Текущие проблемы

1. **Дублирование стилей**: кнопки/инпуты описаны и в CSS, и в компонентах
2. **90+ строк хардкода** в `@layer base` для dark mode
3. **Отсутствие Design Tokens**: магические числа (`px-6`, `py-2.5`)
4. **Нет единого API** для размеров, состояний, вариантов

## 🚀 Рекомендуемая архитектура

### 1️⃣ Design Tokens (tailwind.config.js)

```javascript
// ✅ Централизованные токены вместо хардкода
theme: {
  extend: {
    spacing: {
      xs: '8px',
      sm: '12px',
      md: '16px',
      lg: '24px',
    },
    borderRadius: {
      sm: '6px',
      md: '8px',
      lg: '12px',
    },
  }
}
```

### 2️⃣ CSS Variables для dark mode (index.css)

```css
@layer base {
  :root {
    /* Light theme */
    --color-bg-primary: 249 250 251;    /* gray-50 */
    --color-bg-card: 255 255 255;       /* white */
    --color-text-primary: 17 24 39;     /* gray-900 */
    --color-border: 229 231 235;        /* gray-200 */
  }

  .dark {
    /* Dark theme */
    --color-bg-primary: 17 24 39;       /* gray-900 */
    --color-bg-card: 24 33 47;          /* gray-850 */
    --color-text-primary: 243 244 246;  /* gray-100 */
    --color-border: 55 65 81;           /* gray-700 */
  }

  body {
    @apply bg-[rgb(var(--color-bg-primary))] text-[rgb(var(--color-text-primary))];
  }
}
```

**Преимущества**:
- 10 строк вместо 90+
- Легко менять темы
- Нет `!important`

### 3️⃣ Компонентная библиотека (Atomic Design)

```
src/components/
├── ui/              # Атомы (atoms)
│   ├── Button/
│   │   ├── Button.tsx
│   │   ├── Button.types.ts
│   │   └── index.ts
│   ├── Input/
│   ├── Badge/
│   └── Spinner/
├── common/          # Молекулы (molecules)
│   ├── FormField/   # Input + Label + Error
│   ├── SearchBar/
│   └── Card/
└── layout/          # Организмы (organisms)
    ├── Navbar/
    └── Sidebar/
```

### 4️⃣ Единый API для компонентов

#### Button с вариантами (CVA pattern)

```tsx
// src/components/ui/Button/Button.tsx
import { cva, type VariantProps } from 'class-variance-authority'

const buttonVariants = cva(
  // Base styles
  'inline-flex items-center justify-center rounded-lg font-medium transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed',
  {
    variants: {
      variant: {
        primary: 'bg-primary-500 text-white hover:bg-primary-600 focus:ring-primary-500',
        secondary: 'bg-white text-primary-600 border-2 border-primary-500 hover:bg-primary-50',
        success: 'bg-success-500 text-white hover:bg-success-600 focus:ring-success-500',
        danger: 'bg-error-500 text-white hover:bg-error-600 focus:ring-error-500',
        ghost: 'bg-transparent text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800',
      },
      size: {
        sm: 'h-8 px-3 text-sm',
        md: 'h-10 px-6',
        lg: 'h-12 px-8 text-lg',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  }
)

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement>, VariantProps<typeof buttonVariants> {
  isLoading?: boolean
}

export const Button = ({ variant, size, isLoading, children, ...props }: ButtonProps) => {
  return (
    <button className={buttonVariants({ variant, size })} {...props}>
      {isLoading ? <Spinner /> : children}
    </button>
  )
}
```

**Преимущества CVA**:
- Type-safe варианты
- Автокомплит в IDE
- Легко добавлять новые варианты
- Нет дублирования с CSS

### 5️⃣ Utility Functions для цветов

```typescript
// src/utils/cn.ts
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Использование:
<div className={cn('bg-white p-4', isActive && 'bg-primary-50')} />
```

### 6️⃣ Типизация Theme

```typescript
// src/types/theme.ts
export const colors = {
  primary: 'primary',
  success: 'success',
  warning: 'warning',
  error: 'error',
} as const

export type ColorScheme = keyof typeof colors

export const sizes = {
  sm: 'sm',
  md: 'md',
  lg: 'lg',
} as const

export type Size = keyof typeof sizes
```

## 📦 Рекомендуемые пакеты

```bash
npm install clsx tailwind-merge class-variance-authority
npm install -D @tailwindcss/forms @tailwindcss/typography
```

## 🎯 Roadmap улучшений

### Phase 1: Design Tokens (1-2 часа)
- [ ] Расширить `tailwind.config.js` токенами spacing, borderRadius, shadows
- [ ] Перевести `index.css` на CSS Variables
- [ ] Удалить хардкод из `@layer base`

### Phase 2: Утилиты (30 минут)
- [ ] Добавить `cn()` utility
- [ ] Создать `src/utils/variants.ts` для CVA паттерна
- [ ] Типизировать colors, sizes, variants

### Phase 3: UI Kit (2-3 часа)
- [ ] Переписать Button с CVA
- [ ] Переписать Input с CVA
- [ ] Добавить Badge, Spinner, Alert
- [ ] Создать FormField (Input + Label + Error)

### Phase 4: Документация (1 час)
- [ ] Создать Storybook или примеры компонентов
- [ ] Документировать API каждого компонента
- [ ] Создать guideline по добавлению новых компонентов

## 💡 Примеры из реальных проектов

### Shadcn/ui подход (рекомендуемый)
```
- Копируемые компоненты (не npm пакет)
- CVA для вариантов
- Radix UI для доступности
- Tailwind для стилей
```

### Material-UI паттерн
```
- Themed components
- sx prop для override
- Централизованная тема
```

### Ant Design
```
- ConfigProvider для глобальной темы
- Token система
- CSS-in-JS + CSS Variables
```

## 🔥 Быстрый старт (MVP за 2 часа)

1. **Добавить пакеты**:
```bash
npm install clsx tailwind-merge class-variance-authority
```

2. **Создать `src/utils/cn.ts`**:
```typescript
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

3. **Обновить `index.css`** (убрать 90 строк хардкода):
```css
@layer base {
  :root {
    --bg-primary: 249 250 251;
    --bg-card: 255 255 255;
    --text-primary: 17 24 39;
  }

  .dark {
    --bg-primary: 17 24 39;
    --bg-card: 24 33 47;
    --text-primary: 243 244 246;
  }

  body {
    @apply bg-[rgb(var(--bg-primary))] text-[rgb(var(--text-primary))];
  }
}
```

4. **Переписать 1 компонент** (Button) с CVA:
```tsx
import { cva } from 'class-variance-authority'

const button = cva('base-classes', {
  variants: { /* ... */ }
})
```

5. **Тестировать** на странице логина/регистрации

## 📚 Полезные ресурсы

- [Shadcn/ui](https://ui.shadcn.com/) - примеры компонентов
- [CVA docs](https://cva.style/) - документация class-variance-authority
- [Tailwind UI](https://tailwindui.com/) - готовые паттерны
- [Radix UI](https://www.radix-ui.com/) - доступные примитивы
- [TailwindCSS best practices](https://tailwindcss.com/docs/reusing-styles)

## ⚡ Ожидаемый результат

### До:
```tsx
// 90 строк хардкода в CSS
.dark .bg-white { background-color: rgb(24 33 47) !important; }

// Дублирование в компоненте
<button className="bg-primary-500 text-white px-6 py-2.5..." />
```

### После:
```tsx
// 10 строк CSS Variables
:root { --bg-card: 255 255 255; }

// Типизированный компонент
<Button variant="primary" size="md">Войти</Button>
```

**Выгоды**:
- ✅ Меньше кода (90 строк → 10)
- ✅ Type-safe API
- ✅ Легче поддерживать
- ✅ Переиспользуемые компоненты
- ✅ Нет `!important`
- ✅ Автокомплит в IDE
