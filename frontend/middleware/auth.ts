//Proteccion en paginas para evitar que accedan directamente a la ruta
//tambien valida si el usuario está autenticado para dejarlo pasar o mandarlo al login

export default defineNuxtRouteMiddleware(async () => {

  if (import.meta.server) {
    return
  }

  const {
    estaAutenticado,
    sesionInicializada,
    inicializarSesion
  } = useAuth()

  // Al recargar la página, se debe comprobar primero si existe 
  // un token válido antes de decidir si el usuario puede continuar.
  if (!sesionInicializada.value) {
    await inicializarSesion()
  }

  // Si no existe una sesión válida, se impide el acceso 
  // y se envía al usuario a la página de inicio de sesión.
  if (!estaAutenticado.value) {
    return navigateTo('/login')
  }

})