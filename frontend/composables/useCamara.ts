//Manejo de la camara

import { nextTick, ref, onBeforeUnmount } from 'vue'

export const useCamara = () => {

  const video = ref<HTMLVideoElement | null>(null)

  const stream = ref<MediaStream | null>(null)

  const iniciarCamara = async () => {

    try {

      stream.value = await navigator.mediaDevices.getUserMedia({
        video: true
      })

      await nextTick()

      if (video.value) {
        video.value.srcObject = stream.value
        video.value.muted = true

        await video.value.play()
      }

    } catch (error) {

      console.error(
        'Error al acceder a la cámara:',
        error
      )

      throw error
    }
  }


  const tomarFoto = (): string | null => {

    if (!video.value) {
      return null
    }

    const canvas = document.createElement('canvas')

    canvas.width = video.value.videoWidth
    canvas.height = video.value.videoHeight

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      return null
    }

    contexto.drawImage(
      video.value,
      0,
      0,
      canvas.width,
      canvas.height
    )

    const foto = canvas.toDataURL('image/jpeg')

    detenerCamara()

    return foto
  }


  const detenerCamara = () => {

    if (stream.value) {

      stream.value
        .getTracks()
        .forEach(track => track.stop())

      stream.value = null
    }
  }


  onBeforeUnmount(() => {
    detenerCamara()
  })


  return {
    video,
    stream,
    iniciarCamara,
    tomarFoto,
    detenerCamara
  }
}