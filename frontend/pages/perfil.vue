<template>
  <div>
    <nav>
      <NuxtLink to="/perfil">Perfil</NuxtLink>
      <NuxtLink to="/analisis">Análisis léxico</NuxtLink>
      <button @click="salir">Cerrar sesión</button>
    </nav>
    <h1>Mi perfil</h1>
    <div>
      <h2>Fotografía</h2>

      <!-- Foto actual -->
      <div v-if="fotoModificada">

        <img
          :src="fotoModificada"
          alt="Fotografía de perfil"
          width="305"
        />

      </div>

      <!-- Cambiar fotografía -->
      <button
        type="button"
        @click="iniciarCamara"
      >
        Cambiar fotografía
      </button>

      <!-- Cámara -->
      <div v-if="stream">

        <video
          ref="video"
          autoplay
          playsinline
          width="320"
          height="240"
        ></video>

        <br />

        <button
          type="button"
          @click="tomarFotoNueva"
        >
          Tomar fotografía
        </button>

      </div>

      <!-- Nueva fotografía -->
      <div v-if="rostroRecortadoNuevo">

        <h3>Nueva fotografía</h3>

        <img
          :src="fotoModificadaNueva || rostroRecortadoNuevo"
          alt="Nueva fotografía"
          width="305"
        />

        <div>

          <label>Filtro:</label>

          <select
            v-model="filtroSeleccionado"
            @change="aplicarFiltroNueva"
          >
            <option value="ninguno">Ninguno</option>
            <option value="grises">Grises</option>
            <option value="sepia">Sepia</option>
            <option value="negativo">Negativo</option>
          </select>

        </div>

        <div>

          <p>Stickers</p>

          <button
            :disabled="cantidadStickers >= 10"
            type="button"
            @click="agregarStickerNuevo('😎')"
          >
            😎
          </button>

          <button
            :disabled="cantidadStickers >= 10"
            type="button"
            @click="agregarStickerNuevo('👑')"
          >
            👑
          </button>

          <button
            :disabled="cantidadStickers >= 10"
            type="button"
            @click="agregarStickerNuevo('❤️')"
          >
            ❤️
          </button>

          <button
            :disabled="cantidadStickers >= 10"
            type="button"
            @click="agregarStickerNuevo('⭐')"
          >
            ⭐
          </button>

          <button
            :disabled="cantidadStickers >= 10"
            type="button"
            @click="agregarStickerNuevo('🔥')"
          >
            🔥
          </button>

        </div>

        <br />

        <button
          type="button"
          @click="guardarNuevaFotografia"
        >
          Guardar cambios
        </button>

      </div>

    </div>

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

const fotoModificada = ref<string | null>(null)
const fotoOriginalNueva = ref<string | null>(null)
const rostroRecortadoNuevo = ref<string | null>(null)
const fotoModificadaNueva = ref<string | null>(null)

const { usuario , cerrarSesion} = useAuth()
const passwordActual = ref('')
const nuevaPassword = ref('')
const confirmarNuevaPassword = ref('')

const mensajePassword = ref('')
const errorPassword = ref('')



const {
  video,
  stream,
  iniciarCamara,
  tomarFoto: capturarFoto,
  detenerCamara
} = useCamara()

const {
  detectarRostro
} = useReconocimientoFacial()

const {
  filtroSeleccionado,
  cantidadStickers,
  aplicarFiltro: aplicarFiltroBase,
  agregarSticker: agregarStickerBase,
  reiniciarStickers
} = usePersonalizacionFoto()


const tomarFotoNueva = async () => {

  cantidadStickers.value = 0

  fotoModificadaNueva.value = null

  const foto = capturarFoto()

  if (!foto) {
    errorPassword.value =
      'No fue posible capturar la fotografía.'
    return
  }

  fotoOriginalNueva.value = foto

  const rostro = await detectarRostro(
    fotoOriginalNueva.value
  )

  if (!rostro) {
    errorPassword.value =
      'No se detectó ningún rostro en la fotografía.'
    return
  }

  rostroRecortadoNuevo.value = rostro
}


const aplicarFiltroNueva = async () => {

  cantidadStickers.value = 0

  if (!rostroRecortadoNuevo.value) {
    return
  }

  if (filtroSeleccionado.value === 'ninguno') {
    fotoModificadaNueva.value = null
    return
  }

  try {

    fotoModificadaNueva.value =
      await aplicarFiltroBase(
        rostroRecortadoNuevo.value,
        filtroSeleccionado.value
      )

  } catch (error) {

    errorPassword.value =
      'No fue posible aplicar el filtro.'

    console.error(
      'Error al aplicar filtro:',
      error
    )
  }
}


const agregarStickerNuevo = async (sticker: string) => {

  if (!rostroRecortadoNuevo.value) {
    return
  }

  try {

    const imagenBase =
      fotoModificadaNueva.value ||
      rostroRecortadoNuevo.value

    fotoModificadaNueva.value =
      await agregarStickerBase(
        sticker,
        imagenBase
      )

  } catch (error) {

    errorPassword.value =
      'No fue posible agregar el sticker.'

    console.error(
      'Error al agregar sticker:',
      error
    )
  }
}


const guardarNuevaFotografia = async () => {

  if (
    !fotoOriginalNueva.value ||
    !rostroRecortadoNuevo.value
  ) {
    errorPassword.value =
      'Debes tomar una fotografía antes de guardarla.'
    return
  }

  try {

    const fotoFinal =
      fotoModificadaNueva.value ||
      rostroRecortadoNuevo.value

    const respuesta = await $fetch<{ mensaje: string }>(
      `http://localhost:5283/api/usuarios/${usuario.value?.id}/fotos`,
      {
        method: 'PUT',

        body: {
          fotoOriginal: fotoOriginalNueva.value,
          rostroRecortado: rostroRecortadoNuevo.value,
          fotoModificada: fotoFinal
        }
      }
    )

    fotoModificada.value = fotoFinal

    fotoOriginalNueva.value = null
    rostroRecortadoNuevo.value = null
    fotoModificadaNueva.value = null

    filtroSeleccionado.value = 'ninguno'
    reiniciarStickers()

    mensajePassword.value = respuesta.mensaje

  } catch (error: any) {

    errorPassword.value =
      error?.data?.mensaje ||
      'No fue posible guardar la fotografía.'

    console.error(
      'Error al guardar fotografía:',
      error
    )
  }
}



const obtenerFoto = async () => {

  if (!usuario.value) {
    return
  }

  try {

    const respuesta = await $fetch<{
      fotoModificada: string | null
    }>(
      `http://localhost:5283/api/usuarios/${usuario.value.id}/fotos`
    )

    fotoModificada.value = respuesta.fotoModificada

  } catch (error) {

    console.error(
      'Error al obtener la fotografía:',
      error
    )

  }
}

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

onMounted(async () => {
  await obtenerFoto()
})

const salir = async () => {
  cerrarSesion()
  await navigateTo('/login')
}

</script>