// Abre la ventana de redacción de Gmail en una pestaña nueva, en lugar de usar un link
// "mailto:" (que depende de qué programa tenga configurado el sistema operativo como
// cliente de correo por defecto — en Windows suele ser Outlook, aunque el laboratorio
// use Gmail para todo). Requiere estar logueado en Gmail en el navegador.
export function abrirGmailCompose(destinatario: string, asunto: string, cuerpo?: string) {
  const params = new URLSearchParams({ view: 'cm', fs: '1', to: destinatario, su: asunto })
  if (cuerpo) params.set('body', cuerpo)
  window.open(`https://mail.google.com/mail/?${params}`, '_blank')
}
