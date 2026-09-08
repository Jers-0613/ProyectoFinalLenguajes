// Componente raíz de la aplicación.
// Inicializa la sesión del usuario y muestra la página actual.

<script setup lang="ts">

const { inicializarSesion } = useAuth()

// Componente raíz de la aplicación.
// Inicializa la sesión del usuario y muestra la página actual.
onMounted(() => {
  inicializarSesion()
})

</script>

<template>
  <NuxtPage />
</template>