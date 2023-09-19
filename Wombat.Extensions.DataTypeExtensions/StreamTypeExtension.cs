using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Extensions.DataTypeExtensions
{
    public static partial class DataTypeExtensions
    {
        /// <summary>
        /// 将流读为字符串
        /// 注：默认使用UTF-8编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">指定编码</param>
        /// <returns></returns>
        public static async Task<string> ReadToStringAsync(this Stream stream, Encoding encoding = null)
        {
            int sss = 1;
            sss.ToAscllStr();
            if (!stream.CanRead)
            {
                return string.Empty;
            }
            if (encoding == null)
                encoding = Encoding.UTF8;

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            string resStr = string.Empty;
            resStr = await new StreamReader(stream, encoding).ReadToEndAsync();

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return resStr;
        }
    }
}
