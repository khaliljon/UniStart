import { type ReactNode } from 'react';

interface ProGateProps {
  children: ReactNode;
  hasAccess?: boolean;
  featureName?: string;
}

/**
 * Subscriptions/Pro were removed from the platform. This component now simply
 * renders its children (kept so existing call sites don't need changes).
 */
export function ProGate({ children }: ProGateProps) {
  return <>{children}</>;
}

export default ProGate;
