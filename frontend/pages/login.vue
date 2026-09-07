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
        @click="probarEndpointProtegido"
      >
        Probar acceso protegido
      </button>

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

const formulario = ref({
  correo: '',
  password: ''
})

const errores = ref<string[]>([])
const mensajeExito = ref('')
interface RespuestaLogin {
  mensaje: string
  token: string
  usuario: {
    id: number
    correo: string
    nickname: string
    rolId: number
    activo: boolean
  }
}

const iniciarSesion = async () => {
  errores.value = []
  mensajeExito.value = ''
  localStorage.removeItem('token')// Eliminar token actual

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

  if (errores.value.length > 0) {
    return
  }

  try {
    const respuesta = await $fetch<RespuestaLogin>(
      'http://localhost:5283/api/login',
      {
        method: 'POST',
        body: formulario.value
      }
    )

    mensajeExito.value = respuesta.mensaje

    localStorage.setItem('token', respuesta.token)

    console.log('Respuesta del login:', respuesta)

  } catch (error: any) {
    errores.value.push(
      error?.data?.mensaje || 'Ocurrió un error al iniciar sesión.'
    )

    console.error('Error al iniciar sesión:', error)
  }
}

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
  localStorage.removeItem('token')
  mensajeExito.value = 'Sesión cerrada correctamente.'
}

</script>