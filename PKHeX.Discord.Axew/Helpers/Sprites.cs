using System;
using System.Drawing;
using PKHeX.Core;
using QRCoder;

namespace PKHeX.Discord.Axew.Helpers
{
    public static class Sprites
    {
        public static Bitmap GetFullQR(PKM pkm)
        {
            var payload = Convert.ToBase64String(pkm.DecryptedPartyData);
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(data);
            return qrCode.GetGraphic(20);
        }
    }
}
