using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace speech2text
{
    public class AsyncStringOutputFormatter : TextOutputFormatter
    {
        public AsyncStringOutputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type? type) => typeof(IAsyncEnumerable<string>).IsAssignableFrom(type);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            await response.Body.FlushAsync();

            if (context.Object == null)
                return;

            var iae = (IAsyncEnumerable<string>)context.Object;

            await foreach (var item in iae)
            {
                var buffer = Encoding.UTF8.GetBytes(item);
                await response.Body.WriteAsync(buffer);
                await response.Body.FlushAsync();
            }
        }
    }
}