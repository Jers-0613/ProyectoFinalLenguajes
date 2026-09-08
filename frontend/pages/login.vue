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

      <!-- Botón utilizado durante el desarrollo para comprobar que el token permite acceder a un endpoint protegido. Se eliminará cuando terminemos las pruebas de autenticación. -->
      <button
        type="button"
        @click="probarEndpointProtegido"
      >
        Probar acceso protegido
      </button>

      <!-- Botón temporal de desarrollo. El cierre de sesión definitivo se realiza desde el perfil. -->
      <button
        type="button"
        @click="cerrarSesion"
      >
        Cerrar sesión
      </button>

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

import { ref } from 'vue'
import { useAuth } from '~/composables/useAuth'

//Datos que el user ingresa en el formulario
const formulario = ref({
  correo: '',
  password: ''
})

//Arreglo de errores y mensaje mostrado despues de una operacion exitosa
const errores = ref<string[]>([])
const mensajeExito = ref('')

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
}

// Representa la respuesta completa del endpoint de login.
interface RespuestaLogin {
  mensaje: string
  token: string
  usuario: Usuario
}

const { guardarSesion, cerrarSesion: cerrarSesionAuth } = useAuth()

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

// Prueba temporal para comprobar el acceso a un endpoint protegido utilizando el JWT almacenado en localStorage.
const probarEndpointProtegido = async () => {
  try {
    const token = localStorage.getItem('token')

    const respuesta = await $fetch('http://localhost:5283/api/protegido', {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })

    console.log('Respuesta del endpoint protegido:', respuesta)

  } catch (error: any) {
    console.error('Error al acceder al endpoint protegido:', error)
  }
}

const cerrarSesion = () => {
  cerrarSesionAuth()
  mensajeExito.value = 'Sesión cerrada correctamente.'
}

</script>