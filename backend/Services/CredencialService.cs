    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;
    using backend.Dtos;
    

    namespace backend.Services;

    public class CredencialService
    {
        private readonly QrService qrService;

        public CredencialService(QrService qrService)
        {
            this.qrService = qrService;
        }

        public byte[] GenerarCredencial(CredencialDto datos)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var qr = qrService.GenerarQr(
                datos.CodigoCredencial.ToString()
            );

            var documento = Document.Create(contenedor =>
            {
                contenedor.Page(pagina =>
                {
                    pagina.Size(842, 350);
                    pagina.Margin(20);
                    pagina.PageColor("#F8FAFC"); // Fondo de página suave y limpio
                    
                    pagina.Content()
                        .Column(contenido =>
                        {
                            contenido.Item()
                                .Background("#4F46E5") // Color índigo vibrante 
                                .Padding(12) 
                                .CornerRadius(6)
                                .Row(encabezado =>
                                {
                                    encabezado.RelativeItem()
                                        .Text("ANALIZADOR LÉXICO")
                                        .Bold()
                                        .FontSize(22)
                                        .FontColor("#FFFFFF");

                                    encabezado.ConstantItem(100)
                                        .AlignRight()
                                        .Background("#10B981") // Insignia verde esmeralda
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(8)
                                        .CornerRadius(4)
                                        .Text($"ID: {datos.Id:D3}")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor("#FFFFFF");
                                });
                            // Fila Principal (Fotografía, Datos y QR)
                            contenido.Item()
                                .PaddingTop(15)
                                .Row(fila =>
                                {
                                fila.RelativeItem(1)
                                    .Background("#FFFFFF")
                                    .Border(2)
                                    .BorderColor("#6366F1") // Borde violeta/índigo
                                    .CornerRadius(8)
                                    .Padding(10)
                                    .Image(ConvertirBase64ABytes(datos.Foto!));



                                fila.RelativeItem(2)
                                    .PaddingHorizontal(10)
                                    .Background("#EEF2FF") // Fondo suave con tinte pastel
                                    .Border(1)
                                    .BorderColor("#C7D2FE") // Borde sutil
                                    .CornerRadius(8)
                                    .Padding(15)
                                    .Column(columna =>
                                    {
                                        columna.Item()
                                            .Text("USUARIO")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor("#4338CA"); // Etiqueta en tono índigo oscuro

                                        columna.Item()
                                            .Text(datos.Nickname)
                                            .Bold()
                                            .FontSize(16)
                                            .FontColor("#1E1B4B");

                                        columna.Item()
                                            .PaddingTop(8)
                                            .Text("CORREO ELECTRÓNICO")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor("#4338CA");

                                        columna.Item()
                                            .Text(datos.Correo)
                                            .Bold()
                                            .FontSize(16)
                                            .FontColor("#1E1B4B");

                                        columna.Item()
                                            .PaddingTop(8)
                                            .Text("TELÉFONO")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor("#4338CA");

                                        columna.Item()
                                            .Text(datos.Telefono)
                                            .Bold()
                                            .FontSize(16)
                                            .FontColor("#1E1B4B");

                                        columna.Item()
                                            .PaddingTop(8)
                                            .Text("ROL")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor("#4338CA");

                                        columna.Item()
                                            .Text(datos.Rol)
                                            .Bold()
                                            .FontSize(16)
                                            .FontColor("#059669");
                                    });



                            fila.RelativeItem(1)
                                .Background("#FFFFFF")
                                .Border(2)
                                .BorderColor("#6366F1")
                                .CornerRadius(8)
                                .AlignCenter()
                                .AlignMiddle()
                                .Padding(10)
                                .Image(qr);
                            });
                        });
                });
            });

            return documento.GeneratePdf();
        }

        private byte[] ConvertirBase64ABytes(string imagen)
        {
            var base64 = imagen;

            if (imagen.Contains(","))
            {
                base64 = imagen.Split(',')[1];
            }

            return Convert.FromBase64String(base64);
        }



    }