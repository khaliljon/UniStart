import { useTheme } from '../../context/ThemeContext'

const themes = [
  { id: 'light' as const, name: 'Светлая', icon: '☀️', description: 'Классическая светлая тема' },
  { id: 'dark' as const, name: 'Тёмная', icon: '🌙', description: 'Классическая тёмная тема' },
  { id: 'ocean' as const, name: 'Океан', icon: '🌊', description: 'Глубокие морские оттенки' },
  { id: 'synthwave' as const, name: 'Synthwave', icon: '🎮', description: 'Ретро-футуризм 80-х' },
  { id: 'high-contrast' as const, name: 'Контраст', icon: '♿', description: 'Высокая контрастность' },
]

const ThemeSwitcher = () => {
  const { theme, setTheme } = useTheme()

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-[rgb(var(--text-primary))]">
          Выбор темы
        </h3>
        <span className="text-sm text-[rgb(var(--text-secondary))]">
          Текущая: {themes.find(t => t.id === theme)?.name}
        </span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
        {themes.map((t) => (
          <button
            key={t.id}
            onClick={() => setTheme(t.id)}
            className={`
              p-4 rounded-lg border-2 transition-all duration-normal
              ${theme === t.id 
                ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20' 
                : 'border-[rgb(var(--border))] bg-[rgb(var(--bg-card))] hover:border-[rgb(var(--border-hover))]'
              }
            `}
          >
            <div className="flex items-start gap-3">
              <span className="text-2xl">{t.icon}</span>
              <div className="text-left flex-1">
                <div className="font-medium text-[rgb(var(--text-primary))]">
                  {t.name}
                </div>
                <div className="text-xs text-[rgb(var(--text-secondary))] mt-1">
                  {t.description}
                </div>
              </div>
              {theme === t.id && (
                <svg className="w-5 h-5 text-primary-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
              )}
            </div>
          </button>
        ))}
      </div>

      {/* Quick toggle для light/dark */}
      <div className="pt-4 border-t border-[rgb(var(--border))]">
        <button
          onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}
          className="w-full py-2 px-4 rounded-lg bg-[rgb(var(--bg-secondary))] hover:bg-[rgb(var(--border))] transition-colors duration-normal text-[rgb(var(--text-primary))]"
        >
          Быстрое переключение: {theme === 'light' ? '🌙 На тёмную' : '☀️ На светлую'}
        </button>
      </div>
    </div>
  )
}

export default ThemeSwitcher
