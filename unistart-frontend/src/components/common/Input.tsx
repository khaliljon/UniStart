import { InputHTMLAttributes, forwardRef } from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '../../utils/cn'

const inputVariants = cva(
  // Base styles
  'w-full rounded-lg border transition-all duration-normal focus:outline-none focus:ring-2 focus:border-transparent bg-[rgb(var(--bg-card))] text-[rgb(var(--text-primary))] border-[rgb(var(--border))] placeholder:text-[rgb(var(--text-muted))]',
  {
    variants: {
      size: {
        sm: 'h-8 px-3 text-sm',
        md: 'h-10 px-4',
        lg: 'h-12 px-5 text-lg',
      },
      variant: {
        default: 'focus:ring-primary-500',
        error: 'border-error-500 focus:ring-error-500',
        success: 'border-success-500 focus:ring-success-500',
      },
    },
    defaultVariants: {
      size: 'md',
      variant: 'default',
    },
  }
)

interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'>, VariantProps<typeof inputVariants> {
  label?: string
  error?: string
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className, size, variant, ...props }, ref) => {
    return (
      <div className="w-full">
        {label && (
          <label className="block text-sm font-medium text-[rgb(var(--text-primary))] mb-1.5">
            {label}
          </label>
        )}
        <input
          ref={ref}
          className={cn(inputVariants({ size, variant: error ? 'error' : variant }), className)}
          {...props}
        />
        {error && (
          <p className="mt-1.5 text-sm text-error-600">{error}</p>
        )}
      </div>
    )
  }
)

Input.displayName = 'Input'

export default Input
