//Prueba para validar sesiones validas

<template>
  <div>
    <h1>Página protegida</h1>
    <p>Si puedes ver esta página, estás autenticado.</p>
  </div>
</template>

<script setup lang="ts">

definePageMeta({
  middleware: 'auth'
})

</script>