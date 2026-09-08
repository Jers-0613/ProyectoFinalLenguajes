
//si el usuario recarga la página, Vue pierde el estado de usuario y estaAutenticado, 
// pero el token sigue guardado.

// Gestiona el estado y las operaciones relacionadas
// con la autenticación del usuario.

interface Usuario {
  id: number
  correo: string
  telefono: string
  fechaNacimiento: string
  nickname: string
  preferenciaNotificacionId: number
  rolId: number
  activo: boolean
  fechaRegistro: string
}

export const useAuth = () => {

  // Información del usuario actualmente autenticado.
  const usuario = useState<Usuario | null>('usuario', () => null) //Podemos contener null o la informacion de api/usuario

  // Indica si existe una sesión válida.
  const estaAutenticado = useState<boolean>('estaAutenticado', () => false)

  //usestate hace que el estado de autenticacion sea compartido e Indica si ya terminó la comprobación inicial de la sesión.
  const sesionInicializada = useState<boolean>('sesionInicializada', () => false) 

  // Comprueba si existe un token y valida la sesión
  // consultando al backend.
  const inicializarSesion = async () => {

    sesionInicializada.value = false

    // localStorage solamente está disponible en el navegador.
    if (import.meta.server) {
      return
    }

    const token = localStorage.getItem('token')

    // Si no existe token, no hay una sesión activa
    if (!token) {
      estaAutenticado.value = false
      usuario.value = null
      sesionInicializada.value = true
      return
      }

    try {
      // El backend valida el token y devuelve
      // la información del usuario autenticado.
      const respuesta = await $fetch<Usuario>(
        'http://localhost:5283/api/usuario',
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      )

      usuario.value = respuesta
      estaAutenticado.value = true
      sesionInicializada.value = true

    } catch (error) {

      localStorage.removeItem('token')  // Si el token no es válido, se elimina la sesión local.
      usuario.value = null
      estaAutenticado.value = false
      sesionInicializada.value = true

      console.error('La sesión no es válida:', error)
    }

  }

  // Guarda el token y los datos del usuario después
  // de un inicio de sesión exitoso.
  const guardarSesion = (datosUsuario: Usuario, token: string) => {

    localStorage.setItem('token', token)

    usuario.value = datosUsuario
    estaAutenticado.value = true

  }

  const cerrarSesion = () => {

    localStorage.removeItem('token')

    usuario.value = null
    estaAutenticado.value = false

  }

    return {
    usuario,
    estaAutenticado,
    sesionInicializada,
    inicializarSesion,
    guardarSesion,
    cerrarSesion
  }

}