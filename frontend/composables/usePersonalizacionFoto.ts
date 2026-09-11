import { ref } from 'vue'

export const usePersonalizacionFoto = () => {

  const filtroSeleccionado = ref('ninguno')

  const cantidadStickers = ref(0)

  const aplicarFiltro = (
    imagen: string,
    filtro: string
  ): Promise<string> => {

    return new Promise((resolve, reject) => {

      const imagenOriginal = new Image()

      imagenOriginal.src = imagen

      imagenOriginal.onload = () => {

        const canvas = document.createElement('canvas')

        canvas.width = imagenOriginal.width
        canvas.height = imagenOriginal.height

        const contexto = canvas.getContext('2d')

        if (!contexto) {
          reject(new Error('No fue posible crear el contexto de la imagen.'))
          return
        }

        contexto.drawImage(
          imagenOriginal,
          0,
          0
        )

        const datos = contexto.getImageData(
          0,
          0,
          canvas.width,
          canvas.height
        )

        const pixeles = datos.data

        for (let i = 0; i < pixeles.length; i += 4) {

          const rojo = pixeles[i]
          const verde = pixeles[i + 1]
          const azul = pixeles[i + 2]

          if (filtro === 'grises') {

            const gris =
              0.299 * rojo +
              0.587 * verde +
              0.114 * azul

            pixeles[i] = gris
            pixeles[i + 1] = gris
            pixeles[i + 2] = gris

          } else if (filtro === 'sepia') {

            pixeles[i] =
              rojo * 0.393 +
              verde * 0.769 +
              azul * 0.189

            pixeles[i + 1] =
              rojo * 0.349 +
              verde * 0.686 +
              azul * 0.168

            pixeles[i + 2] =
              rojo * 0.272 +
              verde * 0.534 +
              azul * 0.131

          } else if (filtro === 'negativo') {

            pixeles[i] = 255 - rojo
            pixeles[i + 1] = 255 - verde
            pixeles[i + 2] = 255 - azul
          }
        }

        contexto.putImageData(
          datos,
          0,
          0
        )

        resolve(
          canvas.toDataURL('image/jpeg')
        )
      }

      imagenOriginal.onerror = () => {
        reject(
          new Error('No fue posible cargar la imagen.')
        )
      }
    })
  }


  const reiniciarStickers = () => {

    cantidadStickers.value = 0
  }



  const agregarSticker = (
  sticker: string,
  imagenBase: string
    ): Promise<string> => {

  return new Promise((resolve, reject) => {

    if (cantidadStickers.value >= 10) {
      reject(new Error('Se alcanzó el límite de stickers.'))
      return
    }

    cantidadStickers.value++

    const imagen = new Image()

    imagen.src = imagenBase

    imagen.onload = () => {

      const canvas = document.createElement('canvas')

      canvas.width = imagen.width
      canvas.height = imagen.height

      const contexto = canvas.getContext('2d')

      if (!contexto) {
        reject(
          new Error('No fue posible agregar el sticker.')
        )
        return
      }

      contexto.drawImage(
        imagen,
        0,
        0
      )

      const tamaño = 50

      const x =
        Math.random() * (canvas.width - tamaño)

      const y =
        Math.random() * (canvas.height - tamaño)

      contexto.font = `${tamaño}px Arial`

      contexto.fillText(
        sticker,
        x,
        y
      )

      resolve(
        canvas.toDataURL('image/jpeg')
      )
    }

    imagen.onerror = () => {
      reject(
        new Error('No fue posible cargar la imagen.')
      )
    }
  })
}
  return {
    filtroSeleccionado,
    cantidadStickers,
    aplicarFiltro,
    reiniciarStickers,
    agregarSticker
  }
}