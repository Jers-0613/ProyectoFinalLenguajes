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
import { nextTick } from 'vue'

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

let human: any = null

const video = ref<HTMLVideoElement | null>(null) //Elemento video, donde se verá la camara 
const fotoOriginal = ref<string | null>(null)
const rostroRecortado = ref<string | null>(null)//para reconocimiento facial
const fotoModificada = ref<string | null>(null) //Foto para modificar posteriormente
const filtroSeleccionado = ref('ninguno')
const cantidadStickers = ref(0)

const stream = ref<MediaStream | null>(null) // Conexion con la camara

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


const iniciarCamara = async () => {
  cantidadStickers.value = 0
  fotoOriginal.value = null
  fotoModificada.value = null
  rostroRecortado.value = null
  try {
    stream.value = await navigator.mediaDevices.getUserMedia({ //Solicita permiso al navegador
      video: true
    })

    await nextTick()

    if (video.value) {
      video.value.srcObject = stream.value
    }
  } catch (error) {
    errores.value.push('No fue posible acceder a la cámara.')
    console.error('Error al acceder a la cámara:', error)
  }
}

const tomarFoto = async () => {
  if (!video.value) {
    return
  }

  const canvas = document.createElement('canvas')
  canvas.width = video.value.videoWidth
  canvas.height = video.value.videoHeight

  const contexto = canvas.getContext('2d')

  if (!contexto) {
    errores.value.push('No fue posible capturar la fotografía.')
    return
  }

  contexto.drawImage(
    video.value,
    0,
    0,
    canvas.width,
    canvas.height
  )

  fotoOriginal.value = canvas.toDataURL('image/jpeg')
  

  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())  // Detiene la cámara después de tomar la fotografía.
    stream.value = null
  }

  await detectarRostro() 
  }

const aplicarFiltroGris = () => {
  if (!rostroRecortado.value) {
    return
  }

  const imagen = new Image()

  imagen.src = rostroRecortado.value

  imagen.onload = () => {
    const canvas = document.createElement('canvas')

    canvas.width = imagen.width
    canvas.height = imagen.height

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      errores.value.push('No fue posible aplicar el filtro.')
      return
    }

    contexto.drawImage(imagen, 0, 0)

    const datos = contexto.getImageData(
      0,
      0,
      canvas.width,
      canvas.height
    )

    for (let i = 0; i < datos.data.length; i += 4) {
      const gris =
        datos.data[i] * 0.299 +
        datos.data[i + 1] * 0.587 +
        datos.data[i + 2] * 0.114

      datos.data[i] = gris
      datos.data[i + 1] = gris
      datos.data[i + 2] = gris
    }

    contexto.putImageData(datos, 0, 0)

    fotoModificada.value = canvas.toDataURL('image/jpeg')
  }
}

const aplicarFiltroSepia = () => {

  if (!rostroRecortado.value) {
    return
  }

  const imagen = new Image()
  imagen.src = rostroRecortado.value

  imagen.onload = () => {

    const canvas = document.createElement('canvas')
    canvas.width = imagen.width
    canvas.height = imagen.height

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      errores.value.push('No fue posible aplicar el filtro.')
      return
    }

    contexto.drawImage(imagen, 0, 0)

    const datos = contexto.getImageData(
      0,
      0,
      canvas.width,
      canvas.height
    )

    for (let i = 0; i < datos.data.length; i += 4) {

      const rojo = datos.data[i]
      const verde = datos.data[i + 1]
      const azul = datos.data[i + 2]

      datos.data[i] =
        Math.min(255, rojo * 0.393 + verde * 0.769 + azul * 0.189)

      datos.data[i + 1] =
        Math.min(255, rojo * 0.349 + verde * 0.686 + azul * 0.168)

      datos.data[i + 2] =
        Math.min(255, rojo * 0.272 + verde * 0.534 + azul * 0.131)
    }

    contexto.putImageData(datos, 0, 0)

    fotoModificada.value = canvas.toDataURL('image/jpeg')
  }
}

const aplicarFiltroNegativo = () => {

  if (!rostroRecortado.value) {
    return
  }

  const imagen = new Image()
  imagen.src = rostroRecortado.value

  imagen.onload = () => {

    const canvas = document.createElement('canvas')
    canvas.width = imagen.width
    canvas.height = imagen.height

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      errores.value.push('No fue posible aplicar el filtro.')
      return
    }

    contexto.drawImage(imagen, 0, 0)

    const datos = contexto.getImageData(
      0,
      0,
      canvas.width,
      canvas.height
    )

    for (let i = 0; i < datos.data.length; i += 4) {

      datos.data[i] = 255 - datos.data[i]
      datos.data[i + 1] = 255 - datos.data[i + 1]
      datos.data[i + 2] = 255 - datos.data[i + 2]
    }

    contexto.putImageData(datos, 0, 0)

    fotoModificada.value = canvas.toDataURL('image/jpeg')
  }
}

const aplicarFiltro = () => {
  cantidadStickers.value = 0
  if (filtroSeleccionado.value === 'ninguno') {
    fotoModificada.value = null
    return
  }

  if (filtroSeleccionado.value === 'grises') {
    aplicarFiltroGris()
  }

  if (filtroSeleccionado.value === 'sepia') {
  aplicarFiltroSepia()
  }

  if (filtroSeleccionado.value === 'negativo') {
  aplicarFiltroNegativo()
  }

}


const agregarSticker = (sticker: string) => {

  if (cantidadStickers.value >= 10) {
    return
  }

  if (!rostroRecortado.value) {
    return
  }

    cantidadStickers.value++

  const imagen = new Image()
  imagen.src = fotoModificada.value || rostroRecortado.value

  imagen.onload = () => {

    const canvas = document.createElement('canvas')
    canvas.width = imagen.width
    canvas.height = imagen.height

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      errores.value.push('No fue posible agregar el sticker.')
      return
    }

    contexto.drawImage(imagen, 0, 0)

    const tamaño = 50

    const x = Math.random() * (canvas.width - tamaño)
    const y = Math.random() * (canvas.height - tamaño)

    contexto.font = `${tamaño}px Arial`
    contexto.fillText(sticker, x, y)

    fotoModificada.value = canvas.toDataURL('image/jpeg')
  }
}


const detectarRostro = async () => {
  if (!fotoOriginal.value) {
    return
  }

  try {

    if (!human) {
      const modulo = await import('@vladmandic/human')

      human = new modulo.default({
        modelBasePath: '/models/',
        backend: 'webgl',
        face: {
          enabled: true
        },
        body: {
          enabled: false
        },
        hand: {
          enabled: false
        },
        object: {
          enabled: false
        }
      })
    }

    const imagen = new Image()

    imagen.src = fotoOriginal.value

    await new Promise<void>((resolve, reject) => {
      imagen.onload = () => resolve()
      imagen.onerror = () => reject()
    })

    const resultado = await human.detect(imagen)

    console.log('Resultado de detección facial:', resultado)

    if (resultado.face.length === 0) {
      errores.value.push('No se detectó ningún rostro en la fotografía.')
      return
    }

    console.log('Rostros detectados:', resultado.face.length)
    console.log('Datos del rostro:', resultado.face[0])

    const rostro = resultado.face[0]
    const [x, y, ancho, alto] = rostro.box

    const canvasRostro = document.createElement('canvas')

    canvasRostro.width = ancho
    canvasRostro.height = alto

    const contextoRostro = canvasRostro.getContext('2d')

    if (!contextoRostro) {
      errores.value.push('No fue posible recortar el rostro.')
      return
    }

    contextoRostro.drawImage(
      imagen,
      x,
      y,
      ancho,
      alto,
      0,
      0,
      ancho,
      alto
    )

    rostroRecortado.value = canvasRostro.toDataURL('image/jpeg')



  } catch (error) {
    errores.value.push('Ocurrió un error al detectar el rostro.')
    console.error('Error en detección facial:', error)
  }
}

const detenerCamara = () => {
  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())
    stream.value = null
  }
}

onBeforeUnmount(() => {
  detenerCamara()
})



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