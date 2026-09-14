
import jsQR from 'jsqr'

export const useQrScanner = () => {

  const escanearQr = (
    video: HTMLVideoElement
  ): string | null => {

    if (
      !video.videoWidth ||
      !video.videoHeight
    ) {
      return null
    }

    const canvas = document.createElement('canvas')

    canvas.width = video.videoWidth
    canvas.height = video.videoHeight

    const contexto = canvas.getContext('2d')

    if (!contexto) {
      return null
    }

    contexto.drawImage(
      video,
      0,
      0,
      canvas.width,
      canvas.height
    )

    const imagen = contexto.getImageData(
      0,
      0,
      canvas.width,
      canvas.height
    )

    const resultado = jsQR(
      imagen.data,
      imagen.width,
      imagen.height
    )

    return resultado?.data ?? null
  }


  return {
    escanearQr
  }
}
