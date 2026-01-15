import { motion, HTMLMotionProps } from 'framer-motion'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '../../utils/cn'

const buttonVariants = cva(
  // Base styles
  'inline-flex items-center justify-center font-medium rounded-lg transition-all duration-normal focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed',
  {
    variants: {
      variant: {
        primary: 'bg-primary-500 text-white hover:bg-primary-600 focus:ring-primary-500 dark:bg-primary-600 dark:hover:bg-primary-700',
        secondary: 'bg-[rgb(var(--bg-card))] text-primary-600 border-2 border-primary-500 hover:bg-primary-50 dark:text-primary-400 dark:border-primary-400 dark:hover:bg-gray-700',
        success: 'bg-success-500 text-white hover:bg-success-600 focus:ring-success-500',
        danger: 'bg-error-500 text-white hover:bg-error-600 focus:ring-error-500',
        ghost: 'bg-transparent text-[rgb(var(--text-primary))] hover:bg-[rgb(var(--bg-secondary))]',
        outline: 'bg-transparent text-primary-600 border-2 border-primary-500 hover:bg-primary-50 dark:text-primary-400 dark:border-primary-400 dark:hover:bg-gray-800',
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

interface ButtonProps extends Omit<HTMLMotionProps<"button">, 'variant'>, VariantProps<typeof buttonVariants> {
  isLoading?: boolean
}

const Button = ({
  children,
  variant,
  size,
  isLoading = false,
  className,
  disabled,
  ...props
}: ButtonProps) => {
  return (
    <motion.button
      whileTap={{ scale: disabled || isLoading ? 1 : 0.95 }}
      className={cn(buttonVariants({ variant, size }), className)}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <span className="flex items-center justify-center">
          <svg className="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          Загрузка...
        </span>
      ) : (
        children
      )}
    </motion.button>
  )
}

export default Button
