using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Wombat.Extensions.DataTypeExtensions
{
    /// <summary>
    /// 数据转换
    /// </summary>
    public  static partial class DataTypeExtensions
    {
        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] byteArray)
        {
            return string.Join(" ", byteArray.Select(t => t.ToString("X2")));
        }

        /// <summary>
        /// 16进制字符串转字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strict">严格模式（严格按两个字母间隔一个空格）</param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(this string str, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Trim().Replace(" ", "").Length % 2 != 0)
                throw new ArgumentException("请传入有效的参数");

            if (strict)
            {
                return str.Split(' ').Where(t => t?.Length == 2).Select(t => Convert.ToByte(t, 16)).ToArray();
            }
            else
            {
                str = str.Trim().Replace(" ", "");
                var list = new List<byte>();
                for (int i = 0; i < str.Length; i++)
                {
                    var string16 = str[i].ToString() + str[++i].ToString();
                    list.Add(Convert.ToByte(string16, 16));
                }
                return list.ToArray();
            }
        }

        /// <summary>
        /// Asciis字符串数组字符串装字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        public static byte[] AsciiStringToBytes(this string str, bool strict = true)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Trim().Replace(" ", "").Length % 2 != 0)
                throw new ArgumentException("请传入有效的参数");

            if (strict)
            {
                List<string> stringList = new List<string>();
                foreach (var item in str.Split(' '))
                {
                    stringList.Add(((char)(Convert.ToByte(item, 16))).ToString());
                }
                return HexStringToBytes(string.Join("", stringList), false);
            }
            else
            {
                str = str.Trim().Replace(" ", "");
                var stringList = new List<string>();
                for (int i = 0; i < str.Length; i++)
                {
                    var stringAscii = str[i].ToString() + str[++i].ToString();
                    stringList.Add(((char)Convert.ToByte(stringAscii, 16)).ToString());
                }
                return HexStringToBytes(string.Join("", stringList), false);
            }
        }

        /// <summary>
        /// Asciis数组字符串装字节数组
        /// 如：30 31 =》 00 01
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] AsciiArrayToByteArray(this byte[] str)
        {
            if (!str?.Any() ?? true)
                throw new ArgumentException("请传入有效的参数");

            List<string> stringList = new List<string>();
            foreach (var item in str)
            {
                stringList.Add(((char)item).ToString());
            }
            return HexStringToBytes(string.Join("", stringList), false);
        }

        /// <summary>
        /// 字节数组转换成Ascii字节数组
        /// 如：00 01 => 30 31
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ByteArrayToAsciiArray(this byte[] str)
        {
            return Encoding.ASCII.GetBytes(string.Join("", str.Select(t => t.ToString("X2"))));
        }

        /// <summary>
        /// Int转二进制
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength">补0长度</param>
        /// <returns></returns>
        public static string IntToBinaryArray(this int value, int minLength = 0)
        {
            //Convert.ToString(12,2); // 将12转为2进制字符串，结果 “1100”
            return Convert.ToString(value, 2).PadLeft(minLength, '0');
        }

        /// <summary>
        /// 二进制转Int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int BinaryArrayToInt(this string value)
        {
            //Convert.ToInt("1100",2); // 将2进制字符串转为整数，结果 12
            return Convert.ToInt32(value, 2);
        }


        /// <summary>
        /// 转为bool
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool ToBool(this string str)
        {
            return bool.Parse(str);
        }

        /// <summary>
        /// 转为字节数组
        /// </summary>
        /// <param name="base64Str">base64字符串</param>
        /// <returns></returns>
        public static byte[] ToBytes_FromBase64Str(this string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }

        /// <summary>
        /// 转换为MD5加密后的字符串（默认加密为32位）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMD5String(this string str)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            md5.Dispose();

            return sb.ToString();
        }

        /// <summary>
        /// 转换为MD5加密后的字符串（16位）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMD5String16(this string str)
        {
            return str.ToMD5String().Substring(8, 16);
        }

        /// <summary>
        /// Base64加密
        /// 注:默认采用UTF8编码
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <returns>加密后的字符串</returns>
        public static string Base64Encode(this string source)
        {
            return Base64Encode(source, Encoding.UTF8);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <param name="encoding">加密采用的编码方式</param>
        /// <returns></returns>
        public static string Base64Encode(this string source, Encoding encoding)
        {
            string encode = string.Empty;
            byte[] bytes = encoding.GetBytes(source);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64解密
        /// 注:默认使用UTF8编码
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(this string result)
        {
            return Base64Decode(result, Encoding.UTF8);
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <param name="encoding">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(this string result, Encoding encoding)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encoding.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        /// <summary>
        /// Base64Url编码
        /// </summary>
        /// <param name="text">待编码的文本字符串</param>
        /// <returns>编码的文本字符串</returns>
        public static string Base64UrlEncode(this string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            var base64 = Convert.ToBase64String(plainTextBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return base64;
        }

        /// <summary>
        /// Base64Url解码
        /// </summary>
        /// <param name="base64UrlStr">使用Base64Url编码后的字符串</param>
        /// <returns>解码后的内容</returns>
        public static string Base64UrlDecode(this string base64UrlStr)
        {
            base64UrlStr = base64UrlStr.Replace('-', '+').Replace('_', '/');
            switch (base64UrlStr.Length % 4)
            {
                case 2:
                    base64UrlStr += "==";
                    break;
                case 3:
                    base64UrlStr += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(base64UrlStr);

            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 计算SHA1摘要
        /// 注：默认使用UTF8编码
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static byte[] ToSHA1Bytes(this string str)
        {
            return str.ToSHA1Bytes(Encoding.UTF8);
        }

        /// <summary>
        /// 计算SHA1摘要
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static byte[] ToSHA1Bytes(this string str, Encoding encoding)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] inputBytes = encoding.GetBytes(str);
            byte[] outputBytes = sha1.ComputeHash(inputBytes);

            return outputBytes;
        }

        /// <summary>
        /// 转为SHA1哈希加密字符串
        /// 注：默认使用UTF8编码
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToSHA1String(this string str)
        {
            return str.ToSHA1String(Encoding.UTF8);
        }

        /// <summary>
        /// 转为SHA1哈希
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string ToSHA1String(this string str, Encoding encoding)
        {
            byte[] sha1Bytes = str.ToSHA1Bytes(encoding);
            string resStr = BitConverter.ToString(sha1Bytes);
            return resStr.Replace("-", "").ToLower();
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToSHA256String(this string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// HMACSHA256算法
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="secret">密钥</param>
        /// <returns></returns>
        public static string ToHMACSHA256String(this string text, string secret)
        {
            secret = secret ?? "";
            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(text);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        /// <summary>
        /// string转int
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
                return 0;
            return Convert.ToInt32(str);
        }

        /// <summary>
        /// string转long
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static long ToLong(this string str)
        {
            str = str.Replace("\0", "");
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToInt64(str);
        }

        /// <summary>
        /// 二进制字符串转为Int
        /// </summary>
        /// <param name="str">二进制字符串</param>
        /// <returns></returns>
        public static int ToInt_FromBinString(this string str)
        {
            return Convert.ToInt32(str, 2);
        }

        /// <summary>
        /// 将16进制字符串转为Int
        /// </summary>
        /// <param name="str">数值</param>
        /// <returns></returns>
        public static int ToInt0X(this string str)
        {
            int num = Int32.Parse(str, NumberStyles.HexNumber);
            return num;
        }

        /// <summary>
        /// 转换为double
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }

        /// <summary>
        /// string转byte[]
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        /// <summary>
        /// string转byte[]
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="theEncoding">需要的编码</param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str, Encoding theEncoding)
        {
            return theEncoding.GetBytes(str);
        }

        /// <summary>
        /// 将16进制字符串转为Byte数组
        /// </summary>
        /// <param name="str">16进制字符串(2个16进制字符表示一个Byte)</param>
        /// <returns></returns>
        public static byte[] To0XBytes(this string str)
        {
            List<byte> resBytes = new List<byte>();
            for (int i = 0; i < str.Length; i = i + 2)
            {
                string numStr = $@"{str[i]}{str[i + 1]}";
                resBytes.Add((byte)numStr.ToInt0X());
            }

            return resBytes.ToArray();
        }

        /// <summary>
        /// 将ASCII码形式的字符串转为对应字节数组
        /// 注：一个字节一个ASCII码字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static byte[] ToASCIIBytes(this string str)
        {
            return str.ToList().Select(x => (byte)x).ToArray();
        }

        /// <summary>
        /// 转换为日期格式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str)
        {
            return Convert.ToDateTime(str);
        }

        /// <summary>
        /// 删除Json字符串中键中的@符号
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        /// <returns></returns>
        public static string RemoveAt(this string jsonStr)
        {
            Regex reg = new Regex("\"@([^ \"]*)\"\\s*:\\s*\"(([^ \"]+\\s*)*)\"");
            string strPatten = "\"$1\":\"$2\"";
            return reg.Replace(jsonStr, strPatten);
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static T XmlStrToObject<T>(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<T>(jsonJsonStr);
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <param name="xmlStr">XML字符串</param>
        /// <returns></returns>
        public static JObject XmlStrToJObject(this string xmlStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            string jsonJsonStr = JsonConvert.SerializeXmlNode(doc);

            return JsonConvert.DeserializeObject<JObject>(jsonJsonStr);
        }

        /// <summary>
        /// 将Json字符串转为List'T'
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this string jsonStr)
        {
            return string.IsNullOrEmpty(jsonStr) ? null : JsonConvert.DeserializeObject<List<T>>(jsonStr);
        }

        /// <summary>
        /// 将Json字符串转为DataTable
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this string jsonStr)
        {
            return jsonStr == null ? null : JsonConvert.DeserializeObject<DataTable>(jsonStr);
        }

        /// <summary>
        /// 将Json字符串转为JObject
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static JObject ToJObject(this string jsonStr)
        {
            return jsonStr == null ? JObject.Parse("{}") : JObject.Parse(jsonStr.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 将Json字符串转为JArray
        /// </summary>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static JArray ToJArray(this string jsonStr)
        {
            return jsonStr == null ? JArray.Parse("[]") : JArray.Parse(jsonStr.Replace("&nbsp;", ""));
        }


        /// <summary>
        /// 转为首字母大写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToFirstUpperStr(this string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// 转为首字母小写
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static string ToFirstLowerStr(this string str)
        {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        /// <summary>
        /// 转为网络终结点IPEndPoint
        /// </summary>=
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static IPEndPoint ToIPEndPoint(this string str)
        {
            IPEndPoint iPEndPoint = null;
            try
            {
                string[] strArray = str.Split(':').ToArray();
                string addr = strArray[0];
                int port = Convert.ToInt32(strArray[1]);
                iPEndPoint = new IPEndPoint(IPAddress.Parse(addr), port);
            }
            catch
            {
                iPEndPoint = null;
            }

            return iPEndPoint;
        }

        /// <summary>
        /// 将枚举类型的文本转为枚举类型
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="enumText">枚举文本</param>
        /// <returns></returns>
        public static TEnum ToEnum<TEnum>(this string enumText) where TEnum : struct
        {
            Enum.TryParse(enumText, out TEnum value);

            return value;
        }

        /// <summary>
        /// 转为MurmurHash
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static uint ToMurmurHash(this string str)
        {
            return MurmurHash2.Hash(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 是否为弱密码
        /// 注:密码必须包含数字、小写字母、大写字母和其他符号中的两种并且长度大于8
        /// </summary>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public static bool IsWeakPwd(this string pwd)
        {
            if (pwd.IsNullOrEmpty())
                throw new Exception("pwd不能为空");

            string pattern = "(^[0-9]+$)|(^[a-z]+$)|(^[A-Z]+$)|(^.{0,8}$)";
            if (Regex.IsMatch(pwd, pattern))
                return true;
            else
                return false;
        }


        internal class MurmurHash2
        {
            public static UInt32 Hash(Byte[] data)
            {
                return Hash(data, 0xc58f1a7b);
            }
            const UInt32 m = 0x5bd1e995;
            const Int32 r = 24;

            [StructLayout(LayoutKind.Explicit)]
            struct BytetoUInt32Converter
            {
                [FieldOffset(0)]
                public Byte[] Bytes;

                [FieldOffset(0)]
                public UInt32[] UInts;
            }

            public static UInt32 Hash(Byte[] data, UInt32 seed)
            {
                Int32 length = data.Length;
                if (length == 0)
                    return 0;
                UInt32 h = seed ^ (UInt32)length;
                Int32 currentIndex = 0;
                // array will be length of Bytes but contains Uints
                // therefore the currentIndex will jump with +1 while length will jump with +4
                UInt32[] hackArray = new BytetoUInt32Converter { Bytes = data }.UInts;
                while (length >= 4)
                {
                    UInt32 k = hackArray[currentIndex++];
                    k *= m;
                    k ^= k >> r;
                    k *= m;

                    h *= m;
                    h ^= k;
                    length -= 4;
                }
                currentIndex *= 4; // fix the length
                switch (length)
                {
                    case 3:
                        h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
                        h ^= (UInt32)data[currentIndex] << 16;
                        h *= m;
                        break;
                    case 2:
                        h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
                        h *= m;
                        break;
                    case 1:
                        h ^= data[currentIndex];
                        h *= m;
                        break;
                    default:
                        break;
                }

                // Do a few final mixes of the hash to ensure the last few
                // bytes are well-incorporated.

                h ^= h >> 13;
                h *= m;
                h ^= h >> 15;

                return h;
            }

        }

    }
}
