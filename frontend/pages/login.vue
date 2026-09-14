<template>
  <div>
    <h1>Iniciar sesión</h1>
    
    <form @submit.prevent="iniciarSesion">

      <div>
        <label>Correo electrónico</label>
        <input
          v-model="formulario.correo"
          type="email"
          placeholder="correo@ejemplo.com"
        />
      </div>

      <div>
        <label>Contraseña</label>
        <input
          v-model="formulario.password"
          type="password"
          placeholder="Contraseña"
        />
      </div>

      <button type="submit">
        Iniciar sesión
      </button>

      <button
        type="button"
        @click="mostrarLoginQr = true"
        >
        Iniciar sesión con QR
      </button>

      <NuxtLink to="/recuperar-password">
        ¿Olvidaste tu contraseña?
      </NuxtLink>
      <br>
      <br>
      <NuxtLink to="/registro">
        ¿No tienes una Cuenta? ¡Registrate!
      </NuxtLink>

      <div v-if="mostrarLoginQr">

        <h2>Iniciar sesión con QR</h2>

        <video
          ref="video"
          autoplay
          playsinline
          muted
        ></video>

        <br>
        <br>

        <button
          type="button"
          @click="mostrarLoginQr = false"
        >
          Volver al inicio de sesión
        </button>

      </div>

      



      <div v-if="errores.length > 0">
        <p></p>

        <ul>
          <li v-for="error in errores" :key="error">
            {{ error }}
          </li>
        </ul>
      </div>

      <p v-if="mensajeExito">
        {{ mensajeExito }}
      </p>
    </form>
  </div>
</template>



<script setup lang="ts">

import { ref, watch, onBeforeUnmount  } from 'vue'
import { useAuth } from '~/composables/useAuth'
import { useCamara } from '~/composables/useCamara'
import { useQrScanner } from '~/composables/useQrScanner'

//Datos que el user ingresa en el formulario
const formulario = ref({
  correo: '',
  password: ''
})

//Arreglo de errores y mensaje mostrado despues de una operacion exitosa
const errores = ref<string[]>([])
const mensajeExito = ref('')
const mostrarLoginQr = ref(false)
let intervaloQr: ReturnType<typeof setInterval> | null = null

// Representa la información del usuario que devuelve el backend.
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
  debeCambiarPassword: boolean
}

// Representa la respuesta completa del endpoint de login.
interface RespuestaLogin {
  mensaje: string
  token: string
  usuario: Usuario
}

const { guardarSesion, cerrarSesion: cerrarSesionAuth } = useAuth()
const {
  video,
  iniciarCamara,
  detenerCamara
} = useCamara()

const {
  escanearQr
} = useQrScanner()

watch(mostrarLoginQr, async (mostrar) => {

  if (mostrar) {

    try {

      await iniciarCamara()

      intervaloQr = setInterval(async () => {

        if (!video.value) {
          return
        }

        const codigo = escanearQr(video.value)

        if (codigo) {

          console.log(
            'Código QR detectado:',
            codigo
          )

          if (intervaloQr) {
            clearInterval(intervaloQr)
            intervaloQr = null
          }

          detenerCamara()

          await iniciarSesionQr(codigo)
        }

      }, 300)

    } catch (error) {

      errores.value.push(
        'No fue posible acceder a la cámara.'
      )

      mostrarLoginQr.value = false

    }

  } else {

    if (intervaloQr) {

      clearInterval(intervaloQr)
      intervaloQr = null

    }

    detenerCamara()

  }

})

const iniciarSesion = async () => {
  errores.value = []
  mensajeExito.value = ''
  

  if (!formulario.value.correo || !formulario.value.password) {
    errores.value.push('El correo y la contraseña son obligatorios.')
  }

  const correoValido = /^[^\s@]+@(gmail\.com|miumg\.edu\.gt)$/.test(
    formulario.value.correo
  )

  if (!correoValido) {
    errores.value.push(
      'El correo debe ser de Gmail o de la Universidad Mariano Gálvez.'
    )
  }

  // Si existe algún error de validación, no se realiza la solicitud al backend.
  if (errores.value.length > 0) {
    return
  }

  try {
    // Envía las credenciales al endpoint de autenticación.
    const respuesta = await $fetch<RespuestaLogin>(
      'http://localhost:5283/api/login',
      {
        method: 'POST',
        body: formulario.value
      }
    )

    mensajeExito.value = respuesta.mensaje

    guardarSesion(respuesta.usuario, respuesta.token) 
    //console.log('¿Debe cambiar contraseña?', respuesta.usuario.debeCambiarPassword) se utilizó para verificar que se comunicó que se debia cambiar contraseña

    // Después de autenticarse, el usuario entra a su perfil.
    console.log('Respuesta del login:', respuesta)
    await navigateTo('/perfil')

  } catch (error: any) {
    errores.value.push(
      error?.data?.mensaje || 'Ocurrió un error al iniciar sesión.'
    )

    console.error('Error al iniciar sesión:', error)
  }
}

const iniciarSesionQr = async (codigoCredencial: string) => {

  errores.value = []
  mensajeExito.value = ''

  try {

    const respuesta = await $fetch<RespuestaLogin>(
      'http://localhost:5283/api/login/qr',
      {
        method: 'POST',
        body: {
          codigoCredencial
        }
      }
    )

    mensajeExito.value = respuesta.mensaje

    guardarSesion(
      respuesta.usuario,
      respuesta.token
    )

    console.log(
      'Respuesta del login mediante QR:',
      respuesta
    )

    await navigateTo('/perfil')

  } catch (error: any) {

    errores.value.push(
      error?.data?.mensaje ||
      'No fue posible iniciar sesión mediante QR.'
    )

    console.error(
      'Error al iniciar sesión mediante QR:',
      error
    )

    mostrarLoginQr.value = false
  }
}

const cerrarSesion = () => {
  cerrarSesionAuth()
  mensajeExito.value = 'Sesión cerrada correctamente.'
}

onBeforeUnmount(() => {

  if (intervaloQr) {

    clearInterval(intervaloQr)
    intervaloQr = null

  }

  detenerCamara()

})

</script>