import type { ReactElement } from 'react'

export const MODULO_ICONOS: Record<string, ReactElement> = {
  inicio: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <path d="M3 9.5L10 3l7 6.5V17a1 1 0 01-1 1H4a1 1 0 01-1-1V9.5z" strokeLinejoin="round" />
      <path d="M7 18v-6h6v6" strokeLinejoin="round" />
    </svg>
  ),
  clientes: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <circle cx="7" cy="7" r="3" />
      <path d="M1 18c0-3.3 2.7-6 6-6" strokeLinecap="round" />
      <circle cx="14" cy="7" r="3" />
      <path d="M13 12c3.3 0 6 2.7 6 6" strokeLinecap="round" />
    </svg>
  ),
  consultas: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <path d="M3 4h14v10H3z" strokeLinejoin="round" />
      <path d="M7 18l3-4 3 4" strokeLinejoin="round" />
    </svg>
  ),
  presupuestos: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <rect x="4" y="2" width="12" height="16" rx="1" strokeLinejoin="round" />
      <path d="M7 7h6M7 10h6M7 13h4" strokeLinecap="round" />
    </svg>
  ),
  sse: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <rect x="4" y="2" width="12" height="16" rx="1" strokeLinejoin="round" />
      <path d="M8 2v3h4V2" strokeLinejoin="round" />
      <path d="M7 10h6M7 13h4" strokeLinecap="round" />
    </svg>
  ),
  analisis: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <path d="M7 2v7L4 16h12l-3-7V2" strokeLinejoin="round" />
      <path d="M7 2h6" strokeLinecap="round" />
    </svg>
  ),
  resultados: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <rect x="2" y="10" width="4" height="8" rx="0.5" />
      <rect x="8" y="6" width="4" height="12" rx="0.5" />
      <rect x="14" y="2" width="4" height="16" rx="0.5" />
    </svg>
  ),
  facturacion: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <rect x="3" y="2" width="14" height="16" rx="1" strokeLinejoin="round" />
      <path d="M10 6v8M7 8.5C7 7.1 8.3 6 10 6s3 1.1 3 2.5S11.7 11 10 11s-3 1.1-3 2.5S8.3 16 10 16" strokeLinecap="round" />
    </svg>
  ),
  firma: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <path d="M3 14l2-2 8-8 2 2-8 8-2 2H3v-2z" strokeLinejoin="round" />
      <path d="M13 4l2 2" strokeLinecap="round" />
      <path d="M3 18h14" strokeLinecap="round" />
    </svg>
  ),
  usuarios: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <circle cx="10" cy="7" r="4" />
      <path d="M2 18c0-4.4 3.6-8 8-8s8 3.6 8 8" strokeLinecap="round" />
    </svg>
  ),
  portal: (
    <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.6">
      <ellipse cx="10" cy="10" rx="8" ry="4" />
      <circle cx="10" cy="10" r="2.5" />
    </svg>
  ),
}

/** Marca simplificada del laboratorio (anillo + "G" molecular) para el sidebar. */
export function MarcaLaboratorio({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 40 40" className={className} aria-hidden="true">
      <circle cx="20" cy="20" r="17" fill="none" stroke="#b9bcc2" strokeWidth="3" opacity="0.55" />
      <path
        d="M28 13.5A11 11 0 1020.9 31.4v-8.9h6.6"
        fill="none"
        stroke="var(--teal)"
        strokeWidth="5.5"
        strokeLinecap="round"
      />
      <circle cx="20" cy="20" r="2" fill="var(--teal)" />
    </svg>
  )
}
