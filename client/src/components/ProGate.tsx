import { type ReactNode } from 'react';

interface ProGateProps {
  children: ReactNode;
  hasAccess?: boolean;
  featureName?: string;
}

export function ProGate({ children }: ProGateProps) {
  return <>{children}</>;
}

export default ProGate;
