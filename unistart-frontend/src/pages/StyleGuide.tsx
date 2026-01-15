import Card from '../components/common/Card'
import Button from '../components/common/Button'
import Input from '../components/common/Input'
import ThemeSwitcher from '../components/common/ThemeSwitcher'

const StyleGuide = () => {
  return (
    <div className="min-h-screen bg-[rgb(var(--bg-primary))] p-8">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Header */}
        <div className="text-center space-y-2">
          <h1 className="text-4xl font-bold text-[rgb(var(--text-primary))]">
            UniStart Style Guide
          </h1>
          <p className="text-lg text-[rgb(var(--text-secondary))]">
            Обновлённая архитектура стилей с CSS Variables и CVA
          </p>
        </div>

        {/* Theme Switcher */}
        <Card>
          <ThemeSwitcher />
        </Card>

        {/* Buttons */}
        <Card>
          <h2 className="text-2xl font-bold mb-4 text-[rgb(var(--text-primary))]">
            Кнопки
          </h2>
          
          <div className="space-y-4">
            <div>
              <h3 className="text-sm font-semibold text-[rgb(var(--text-secondary))] mb-2">
                Варианты
              </h3>
              <div className="flex flex-wrap gap-3">
                <Button variant="primary">Primary</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="success">Success</Button>
                <Button variant="danger">Danger</Button>
                <Button variant="ghost">Ghost</Button>
                <Button variant="outline">Outline</Button>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-semibold text-[rgb(var(--text-secondary))] mb-2">
                Размеры
              </h3>
              <div className="flex flex-wrap items-center gap-3">
                <Button size="sm">Small</Button>
                <Button size="md">Medium</Button>
                <Button size="lg">Large</Button>
              </div>
            </div>

            <div>
              <h3 className="text-sm font-semibold text-[rgb(var(--text-secondary))] mb-2">
                Состояния
              </h3>
              <div className="flex flex-wrap gap-3">
                <Button isLoading>Loading</Button>
                <Button disabled>Disabled</Button>
              </div>
            </div>
          </div>
        </Card>

        {/* Inputs */}
        <Card>
          <h2 className="text-2xl font-bold mb-4 text-[rgb(var(--text-primary))]">
            Поля ввода
          </h2>
          
          <div className="space-y-4 max-w-md">
            <Input 
              label="Email" 
              placeholder="your@email.com"
              type="email"
            />
            
            <Input 
              label="Пароль" 
              placeholder="••••••••"
              type="password"
            />
            
            <Input 
              label="Поле с ошибкой" 
              error="Это обязательное поле"
              placeholder="Введите значение"
            />

            <div>
              <h3 className="text-sm font-semibold text-[rgb(var(--text-secondary))] mb-2">
                Размеры
              </h3>
              <div className="space-y-2">
                <Input size="sm" placeholder="Small input" />
                <Input size="md" placeholder="Medium input" />
                <Input size="lg" placeholder="Large input" />
              </div>
            </div>
          </div>
        </Card>

        {/* Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <Card>
            <h3 className="text-lg font-semibold mb-2 text-[rgb(var(--text-primary))]">
              Обычная карточка
            </h3>
            <p className="text-[rgb(var(--text-secondary))]">
              Использует CSS Variables для всех цветов
            </p>
          </Card>

          <Card hoverable>
            <h3 className="text-lg font-semibold mb-2 text-[rgb(var(--text-primary))]">
              Карточка с hover
            </h3>
            <p className="text-[rgb(var(--text-secondary))]">
              Наведите мышку для эффекта
            </p>
          </Card>

          <Card className="border-2 border-primary-500">
            <h3 className="text-lg font-semibold mb-2 text-primary-600">
              Кастомный стиль
            </h3>
            <p className="text-[rgb(var(--text-secondary))]">
              Можно переопределить через className
            </p>
          </Card>
        </div>

        {/* Design Tokens Info */}
        <Card>
          <h2 className="text-2xl font-bold mb-4 text-[rgb(var(--text-primary))]">
            Design Tokens
          </h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="font-semibold mb-2 text-[rgb(var(--text-primary))]">
                Цвета (CSS Variables)
              </h3>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2">
                  <div className="w-6 h-6 rounded bg-[rgb(var(--bg-primary))] border border-[rgb(var(--border))]"></div>
                  <code>--bg-primary</code>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-6 h-6 rounded bg-[rgb(var(--bg-card))] border border-[rgb(var(--border))]"></div>
                  <code>--bg-card</code>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-6 h-6 rounded bg-[rgb(var(--accent))]"></div>
                  <code>--accent</code>
                </div>
              </div>
            </div>

            <div>
              <h3 className="font-semibold mb-2 text-[rgb(var(--text-primary))]">
                Преимущества
              </h3>
              <ul className="space-y-1 text-sm text-[rgb(var(--text-secondary))]">
                <li>✅ Type-safe варианты с CVA</li>
                <li>✅ Автокомплит в IDE</li>
                <li>✅ Легко менять темы</li>
                <li>✅ Нет дублирования стилей</li>
                <li>✅ Централизованные токены</li>
              </ul>
            </div>
          </div>
        </Card>

        {/* Code Example */}
        <Card>
          <h2 className="text-2xl font-bold mb-4 text-[rgb(var(--text-primary))]">
            Примеры использования
          </h2>
          
          <div className="space-y-4">
            <div className="bg-[rgb(var(--bg-secondary))] p-4 rounded-lg">
              <pre className="text-sm text-[rgb(var(--text-primary))] overflow-x-auto">
{`// Кнопка с type-safe вариантами
<Button variant="primary" size="lg">
  Сохранить
</Button>

// Input с валидацией
<Input 
  label="Email" 
  error={errors.email}
  size="md"
/>

// Кастомные классы через cn()
<div className={cn(
  'base-class',
  isActive && 'active-class',
  className
)}>
  ...
</div>`}
              </pre>
            </div>
          </div>
        </Card>
      </div>
    </div>
  )
}

export default StyleGuide
