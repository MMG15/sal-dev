const SESION_KEY = 'ssal_sesion'

function getToken(): string | null {
  try {
    const raw = localStorage.getItem(SESION_KEY)
    return raw ? JSON.parse(raw).token : null
  } catch {
    return null
  }
}

export async function apiFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const token = getToken()
  const isFormData = options.body instanceof FormData
  return fetch(path, {
    ...options,
    headers: {
      // No forzar Content-Type en multipart: el browser lo pone con el boundary
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  })
}
