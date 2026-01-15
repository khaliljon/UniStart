import { ReactNode } from 'react'
import { motion } from 'framer-motion'
import { cn } from '../../utils/cn'

interface CardProps {
  children: ReactNode
  className?: string
  hoverable?: boolean
  onClick?: () => void
}

const Card = ({ children, className, hoverable = false, onClick }: CardProps) => {
  return (
    <motion.div
      whileHover={hoverable ? { y: -4 } : undefined}
      className={cn(
        'bg-[rgb(var(--bg-card))] rounded-xl shadow-md p-6 text-[rgb(var(--text-primary))]',
        hoverable && 'cursor-pointer card-hover',
        className
      )}
      onClick={onClick}
    >
      {children}
    </motion.div>
  )
}

export default Card
