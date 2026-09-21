using QRCoder;

namespace BuisnessLogicLayer.Services
{
    public class QRCodeService
    {
        public byte[] GenerateQRCode(string data)
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();

            QRCodeData qrCodeData = qrGenerator.CreateQrCode(
                data,
                QRCodeGenerator.ECCLevel.Q
            );

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }
    }
}