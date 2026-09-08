<template>
  <div>
    <h1>Mi perfil</h1>

    <p><strong>Correo:</strong> {{ usuario?.correo }}</p>
    <p><strong>Teléfono:</strong> {{ usuario?.telefono }}</p>
    <p><strong>Fecha de nacimiento:</strong> {{ usuario?.fechaNacimiento }}</p>
    <p><strong>Nickname:</strong> {{ usuario?.nickname }}</p>
    <p><strong>Preferencia de notificación:</strong> {{ usuario?.preferenciaNotificacionId }}</p>
    <p><strong>Rol:</strong> {{ usuario?.rolId }}</p>
    <p><strong>Activo:</strong> {{ usuario?.activo }}</p>
    <p><strong>Fecha de registro:</strong> {{ usuario?.fechaRegistro }}</p>
    <button @click="salir"> Cerrar sesión </button>
  </div>
</template>

<script setup lang="ts">

const { usuario , cerrarSesion} = useAuth()

definePageMeta({
  middleware: 'auth'
})

const salir = async () => {
  cerrarSesion()
  await navigateTo('/login')
}

</script>