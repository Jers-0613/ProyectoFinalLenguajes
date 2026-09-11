export const useReconocimientoFacial = () => {

  let human: any = null

  const detectarRostro = async (
    fotoOriginal: string
  ): Promise<string | null> => {

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

      imagen.src = fotoOriginal

      await new Promise<void>((resolve, reject) => {

        imagen.onload = () => resolve()

        imagen.onerror = () => reject()
      })

      const resultado = await human.detect(imagen)

      console.log(
        'Resultado de detección facial:',
        resultado
      )

      if (resultado.face.length === 0) {

        return null
      }

      console.log(
        'Rostros detectados:',
        resultado.face.length
      )

      console.log(
        'Datos del rostro:',
        resultado.face[0]
      )

      const rostro = resultado.face[0]

      const [x, y, ancho, alto] = rostro.box

      const canvasRostro =
        document.createElement('canvas')

      canvasRostro.width = ancho
      canvasRostro.height = alto

      const contextoRostro =
        canvasRostro.getContext('2d')

      if (!contextoRostro) {

        return null
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

      return canvasRostro.toDataURL('image/jpeg')

    } catch (error) {

      console.error(
        'Error en detección facial:',
        error
      )

      throw error
    }
  }

  return {
    detectarRostro
  }
}