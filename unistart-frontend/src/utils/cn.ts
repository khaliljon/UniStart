import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * Утилита для объединения и слияния Tailwind CSS классов
 * Использует clsx для условных классов и tailwind-merge для разрешения конфликтов
 * 
 * @example
 * cn('px-4 py-2', isActive && 'bg-blue-500', 'px-6') // => 'py-2 bg-blue-500 px-6'
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
