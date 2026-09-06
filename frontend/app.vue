<template>
  <div>
    <h1>Proyecto Final — Autómatas y Lenguajes Formales</h1>

    <p>{{ mensaje }}</p>
  </div>
</template>

<script setup lang="ts">
const mensaje = ref('Conectando con la API...')

onMounted(async () => {
  try {
    const respuesta = await $fetch<string>('http://localhost:5283/api/test')
    mensaje.value = respuesta
  } catch (error) {
    mensaje.value = 'Error al conectar con la API'
    console.error(error)
  }
})
</script>