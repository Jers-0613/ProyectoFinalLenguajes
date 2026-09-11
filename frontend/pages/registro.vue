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

      <div>
        <h2>Fotografía</h2>

        <button type="button" @click="iniciarCamara">
          Activar cámara
        </button>

        <button type="submit">
          Registrarse
        </button>

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
            @click="tomarFoto()"
          >
            Tomar fotografía
          </button>
        </div>

         


        <div v-if="rostroRecortado">
          <h3>Rostro</h3>

          <img
            :src="fotoModificada || rostroRecortado"
            alt="Rostro detectado"
            width="305"
          />

          <div>
              <label>Filtro:</label>
              <select v-model="filtroSeleccionado" @change="aplicarFiltro">
                <option value="ninguno">Ninguno</option>
                <option value="grises">Grises</option>
                <option value="sepia">Sepia</option>
                <option value="negativo">Negativo</option>
              </select>
          </div>

          <div v-if="rostroRecortado">
            <p>Stickers</p>

            <button
              :disabled="cantidadStickers >= 10"
              type="button"
              @click="agregarSticker('😎')"
            >
              😎
            </button>

            <button
              type="button"
              :disabled="cantidadStickers >= 10"
              @click="agregarSticker('👑')"
            >
              👑
            </button>

            <button
              type="button"
              :disabled="cantidadStickers >= 10"
              @click="agregarSticker('❤️')"
            >
              ❤️
            </button>

            <button
              type="button"
              :disabled="cantidadStickers >= 10"
              @click="agregarSticker('⭐')"
            >
              ⭐
            </button>

            <button
              type="button"
              :disabled="cantidadStickers >= 10"
              @click="agregarSticker('🔥')"
            >
              🔥
            </button>




          </div>

        </div>

        

      </div>

      
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
//Composables
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
  reiniciarStickers,
  agregarSticker: agregarStickerBase
} = usePersonalizacionFoto()

const errores = ref<string[]>([])
const mensajeExito = ref('')



const fotoOriginal = ref<string | null>(null)
const rostroRecortado = ref<string | null>(null)//para reconocimiento facial
const fotoModificada = ref<string | null>(null) //Foto para modificar posteriormente



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

const tomarFoto = async () => {

  cantidadStickers.value = 0

  const foto = capturarFoto() 

  if (!foto) {
    errores.value.push(
      'No fue posible capturar la fotografía.'
    )
    return
  }

  fotoOriginal.value = foto


  const rostro = await detectarRostro(
    fotoOriginal.value
  )

  if (!rostro) {
    errores.value.push(
      'No se detectó ningún rostro en la fotografía.'
    )
    return
  }

  rostroRecortado.value = rostro

}

const aplicarFiltro = async () => {

  cantidadStickers.value = 0

  if (!rostroRecortado.value) {
    return
  }

  if (filtroSeleccionado.value === 'ninguno') {
    fotoModificada.value = null
    return
  }

  try {

    fotoModificada.value = await aplicarFiltroBase(
      rostroRecortado.value,
      filtroSeleccionado.value
    )

  } catch (error) {

    errores.value.push(
      'No fue posible aplicar el filtro.'
    )

    console.error(
      'Error al aplicar filtro:',
      error
    )
  }
}

const agregarSticker = async (sticker: string) => {

  if (!rostroRecortado.value) {
    return
  }

  try {

    const imagenBase =
      fotoModificada.value ||
      rostroRecortado.value

    fotoModificada.value =
      await agregarStickerBase(
        sticker,
        imagenBase
      )

  } catch (error) {

    errores.value.push(
      'No fue posible agregar el sticker.'
    )

    console.error(
      'Error al agregar sticker:',
      error
    )
  }
}


// Valida los datos del formulario y registra // al usuario mediante el API.
const registrarUsuario = async () => {
    errores.value = []
    mensajeExito.value = ''

  if (!fotoOriginal.value || !rostroRecortado.value) {
    errores.value.push('La fotografía es obligatoria.')
  }
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

  if (!fotoModificada.value) {
    fotoModificada.value = rostroRecortado.value
  }


  const datosRegistro = {
    ...formulario.value,
    fotoOriginal: fotoOriginal.value,
    rostroRecortado: rostroRecortado.value,
    fotoModificada: fotoModificada.value
  }

    try {
      // Envía los datos validados al endpoint de registro.
      const respuesta = await $fetch<RespuestaRegistro>('http://localhost:5283/api/usuarios', {
        method: 'POST',
        body: datosRegistro
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