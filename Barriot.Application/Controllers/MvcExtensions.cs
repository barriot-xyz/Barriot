using Barriot.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barriot.Application.Controllers
{
    public static class MvcExtensions
    {
        /// <summary>
        ///     Builds a <see cref="ContentResultBuilder"/> into a new <see cref="ContentResult"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContentResult Build(this ContentResultBuilder builder)
        {
            var (payload, code, contentType) = builder.Fetch();

            return new ContentResult()
            {
                Content = payload,
                StatusCode = code,
                ContentType = contentType
            };
        }
    }
}
