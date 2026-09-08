//Pagina de registro de usuarios, debera estar conectada con el api correspondiente

<template>
  <div>
    <h1>Crear una cuenta</h1>

    <form @submit.prevent="registrarUsuario">

      <div>
        <label>Correo electrónico</label>
        <input
          v-model="formulario.correo"
          type="email"
        />
      </div>

      <div>
        <label>Teléfono</label>
        <input
          v-model="formulario.telefono"
          type="tel"
        />
      </div>

      <div>
        <label>Fecha de nacimiento</label>
        <input
          v-model="formulario.fechaNacimiento"
          type="date"
        />
      </div>

      <div>
        <label>Nickname</label>
        <input
          v-model="formulario.nickname"
          type="text"
        />
      </div>

      <div>
        <label>Contraseña</label>
        <input
          v-model="formulario.password"
          type="password"
        />
      </div>

      <div>
        <label>Confirmar contraseña</label>
        <input
          v-model="formulario.confirmarPassword"
          type="password"
        />
      </div>

      <div>
        <label>Preferencia de notificación</label>

        <select v-model="formulario.preferenciaNotificacionId">
          <option :value="1">Correo electrónico</option>
          <option :value="2">WhatsApp</option>
          <option :value="3">Ambos</option>
        </select>
      </div>

      <button type="submit">
        Registrarse
      </button>
      <!-- Muestra todos los errores encontrados durante la validación. -->
      <div v-if="errores.length > 0">
  <p>Por favor, corrige los siguientes errores:</p>

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

// Contiene los datos que serán enviados al backend.
const formulario = ref({
  correo: '',
  telefono: '',
  fechaNacimiento: '',
  nickname: '',
  password: '',
  confirmarPassword: '',
  preferenciaNotificacionId: 1
})

const errores = ref<string[]>([])
const mensajeExito = ref('')

// Representa la estructura de la respuesta enviada por el endpoint de registro.
interface RespuestaRegistro {
  mensaje: string
  usuario: {
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
}

// Valida los datos del formulario y registra // al usuario mediante el API.
const registrarUsuario = async () => {
    errores.value = []
    mensajeExito.value = ''
  //Validar campos vacios
  if (
    !formulario.value.correo ||
    !formulario.value.telefono ||
    !formulario.value.fechaNacimiento ||
    !formulario.value.nickname ||
    !formulario.value.password ||
    !formulario.value.confirmarPassword
  ) {
    errores.value.push('Todos los campos son obligatorios.')
  }
  //Valida que las contraseñas coincidan
  if (formulario.value.password !== formulario.value.confirmarPassword) {
    errores.value.push('Las contraseñas no coinciden.')
  }
  // Contraseña de 8 digitos o mas
  if (formulario.value.password.length < 8) {
    errores.value.push('La contraseña debe tener al menos 8 caracteres.')
  }
  //valida que la contraseña tenga mayusculas, minusculas y digitos mediante una er
  const contraseñaSegura = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/

  if (!contraseñaSegura.test(formulario.value.password)) {
    errores.value.push(
      'La contraseña debe tener al menos una mayúscula, una minúscula y un número.'
    )
  }
  //Valida que el correo sea de extension gmail o miumg meidante una er
  const correoValido = /^[^\s@]+@(gmail\.com|miumg\.edu\.gt)$/.test(
    formulario.value.correo
  )

  if (!correoValido) {
    errores.value.push(
      'El correo debe ser de Gmail o de la Universidad Mariano Gálvez.'
    )
  }
  //Valida que el telefono sea de 8 digitos
  const telefonoValido = /^\d{8}$/.test(
    formulario.value.telefono
  )

  if (!telefonoValido) {
    errores.value.push(
      'El teléfono debe contener exactamente 8 dígitos.'
    )
  }
  //Valida si se agregó algun error a la lista
  if (errores.value.length > 0) {
    return
  }

    try {
      // Envía los datos validados al endpoint de registro.
      const respuesta = await $fetch<RespuestaRegistro>('http://localhost:5283/api/usuarios', {
        method: 'POST',
        body: formulario.value
      })
      
      mensajeExito.value = respuesta.mensaje  
      
      setTimeout(() => {
        navigateTo('/login')
      }, 2000)

      console.log('Respuesta de la API:', respuesta)

    } catch (error: any) {
      errores.value.push(
      error?.data?.mensaje || 'Ocurrió un error al registrar el usuario.'
    )

      console.error('Error al registrar usuario:', error)
    }
} 

</script>