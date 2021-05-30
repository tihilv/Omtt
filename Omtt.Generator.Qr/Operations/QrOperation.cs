using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Omtt.Api.Generation;
using Omtt.Api.TemplateModel;
using QRCoder;

namespace Omtt.Generator.Operations
{
    internal sealed class QrOperation: ITemplateOperation
    {
        public String Name => "qr";

        public async Task PerformAsync(OperationTemplatePart part, IGeneratorContext ctx)
        {
            var content = await GetQrContent(part, ctx);
            var code = GetQrCode(content);

            if (ctx.FragmentType?.Equals(CommonFragmentTypes.Html, StringComparison.InvariantCultureIgnoreCase)??false)
            {
                var result = $"<img src=\"data:image/png;base64, {Convert.ToBase64String(code)}\"/>";
                await ctx.WriteAsync(result);
            }
            else
            {
                await ctx.WriteAsync(Convert.ToBase64String(code));
            }
        }

        private static Byte[] GetQrCode(String content)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new BitmapByteQRCode(qrCodeData))
                return qrCode.GetGraphic(3);
        }

        private async Task<String> GetQrContent(OperationTemplatePart part, IGeneratorContext ctx)
        {
            if (part.InnerPart == null)
                throw new ArgumentNullException("Operation content is null.");

            using (var memoryStream = new MemoryStream())
            {
                using (ctx.OverloadStream(memoryStream))
                {
                    await ctx.ExecuteAsync(part.InnerPart!);
                }

                memoryStream.Position = 0;
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public Task PerformAsync(OperationTemplatePart part, ISourceSchemeContext ctx)
        {
            return ctx.ExecuteAsync(part.InnerPart!);
        }
    }
}