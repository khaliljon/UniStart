import { ReactNode } from 'react'
import { motion } from 'framer-motion'

interface CardProps {
  children: ReactNode
  className?: string
  hoverable?: boolean
  onClick?: () => void
}

const Card = ({ children, className = '', hoverable = false, onClick }: CardProps) => {
  return (
    <motion.div
      whileHover={hoverable ? { y: -4 } : undefined}
      className={`
        bg-white rounded-xl shadow-md p-6
        ${hoverable ? 'cursor-pointer card-hover' : ''}
        ${className}
      `}
      onClick={onClick}
    >
      {children}
    </motion.div>
  )
}

export default Card
