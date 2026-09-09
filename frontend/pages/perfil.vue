<template>
  <div>
    <nav>
      <NuxtLink to="/perfil">Perfil</NuxtLink>
      <NuxtLink to="/analisis">Análisis léxico</NuxtLink>
      <button @click="salir">Cerrar sesión</button>
    </nav>
    <h1>Mi perfil</h1>
    <div v-if="usuario?.debeCambiarPassword">
      <h2>Debes cambiar tu contraseña</h2>

      <p>
        Estás utilizando una contraseña temporal. Debes establecer una nueva contraseña antes de continuar.
      </p>
    </div>

    <p><strong>Correo:</strong> {{ usuario?.correo }}</p>
    <p><strong>Teléfono:</strong> {{ usuario?.telefono }}</p>
    <p><strong>Fecha de nacimiento:</strong> {{ usuario?.fechaNacimiento }}</p>
    <p><strong>Nickname:</strong> {{ usuario?.nickname }}</p>
    <p><strong>Preferencia de notificación:</strong> {{ usuario?.preferenciaNotificacionId }}</p>
    <p><strong>Rol:</strong> {{ usuario?.rolId }}</p>
    <p><strong>Activo:</strong> {{ usuario?.activo }}</p>
    <p><strong>Fecha de registro:</strong> {{ usuario?.fechaRegistro }}</p>

    <h2>Cambiar contraseña</h2>

    <input
      v-model="passwordActual"
      type="password"
      placeholder="Contraseña actual"
    />

    <input
      v-model="nuevaPassword"
      type="password"
      placeholder="Nueva contraseña"
    />

    <input
      v-model="confirmarNuevaPassword"
      type="password"
      placeholder="Confirmar nueva contraseña"
    />

    <button @click="cambiarPassword">
      Cambiar contraseña
    </button>
    
    <p v-if="errorPassword">
      {{ errorPassword }}
    </p>

    <p v-if="mensajePassword">
      {{ mensajePassword }}
    </p>
    
  </div>
</template>

<script setup lang="ts">

const { usuario , cerrarSesion} = useAuth()
const passwordActual = ref('')
const nuevaPassword = ref('')
const confirmarNuevaPassword = ref('')

const mensajePassword = ref('')
const errorPassword = ref('')

const cambiarPassword = async () => {
  mensajePassword.value = ''
  errorPassword.value = ''

  if (!passwordActual.value || !nuevaPassword.value || !confirmarNuevaPassword.value) {
    errorPassword.value = 'Todos los campos son obligatorios.'
    return
  }

  if (nuevaPassword.value !== confirmarNuevaPassword.value) {
    errorPassword.value = 'Las nuevas contraseñas no coinciden.'
    return
  }

  if (
    nuevaPassword.value.length < 8 ||
    !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(nuevaPassword.value)
  ) {
    errorPassword.value =
      'La nueva contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.'
    return
  }

  try {
  const token = localStorage.getItem('token')

  const respuesta = await $fetch<{ mensaje: string }>(
    'http://localhost:5283/api/usuario/password',
    {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${token}`
      },
      body: {
        passwordActual: passwordActual.value,
        nuevaPassword: nuevaPassword.value,
        confirmarNuevaPassword: confirmarNuevaPassword.value
      }
    }
  )

  mensajePassword.value = respuesta.mensaje
  if (usuario.value) {
    usuario.value.debeCambiarPassword = false
  }

} catch (error: any) {
  errorPassword.value =
    error?.data?.mensaje || 'Ocurrió un error al cambiar la contraseña.'
}


} 

definePageMeta({
  middleware: 'auth'
})

const salir = async () => {
  cerrarSesion()
  await navigateTo('/login')
}

</script>