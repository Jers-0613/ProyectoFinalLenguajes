<template>
  <div>
    <h1>Recuperar contraseña</h1>

    <form @submit.prevent="recuperarPassword">
      <input
        v-model="correo"
        type="email"
        placeholder="Correo electrónico"
      />

      <button type="submit">
        Recuperar contraseña
      </button>
    </form>

    <p v-if="error">
      {{ error }}
    </p>

    <p v-if="mensaje">
      {{ mensaje }}
    </p>

    <NuxtLink to="/login">
      Volver al inicio de sesión
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">

const correo = ref('')
const error = ref('')
const mensaje = ref('')

const recuperarPassword = async () => {
  error.value = ''
  mensaje.value = ''

  if (!correo.value) {
    error.value = 'El correo electrónico es obligatorio.'
    return
  }

  try {
    const respuesta = await $fetch<{ mensaje: string }>(
      'http://localhost:5283/api/recuperar-password',
      {
        method: 'POST',
        body: {
          correo: correo.value
        }
      }
    )

    mensaje.value = respuesta.mensaje
  } catch (error: any) {
    error.value =
      error?.data?.mensaje ||
      'Ocurrió un error al solicitar la recuperación.'
  }
}

</script>