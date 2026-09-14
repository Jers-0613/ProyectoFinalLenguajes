
using QRCoder;

namespace backend.Services;

public class QrService
{
    public byte[] GenerarQr(string contenido)
    {
        using var generador = new QRCodeGenerator();

        using var datosQr =
            generador.CreateQrCode(
                contenido,
                QRCodeGenerator.ECCLevel.Q
            );

        var qrCode = new PngByteQRCode(datosQr);

        return qrCode.GetGraphic(10);
    }
}

