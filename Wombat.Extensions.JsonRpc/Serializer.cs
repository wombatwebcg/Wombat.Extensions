using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace Wombat.Extensions.JsonRpc
{
    internal static class Serializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Serialize<T>(T input) where T : new()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(input));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<T>(Stream stream, T input)
        {
          var buffer =  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(input));
            // 将JSON字符串转换为MemoryStream
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(buffer);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin); // 将流的位置设置回开头
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize<T>(string json) where T : new()
        {

            //return Utf8.Deserialize<T>(MemoryMarshal.AsBytes(json.AsSpan()));


            return JsonConvert.DeserializeObject<T>(json);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize<T>(Span<byte> span)
        {
            //return Utf8.Deserialize<T>(span);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(span.ToArray()));

        }

        public static bool ConvertToConcreteType(Type inputType, Type outputType, ref object value)
        {
            //if (value is SpanJsonDynamic<byte> sjd)
            //{
            //    return sjd.TryConvert(outputType, out value);
            //}
            //else if (value is SpanJsonDynamicArray<byte> nt)
            //{
            //    var et = outputType.GetElementType();
            //    var a = Array.CreateInstance(et, nt.Length);
            //    int i = 0;
            //    foreach (SpanJsonDynamic<byte> ev in nt)
            //    {
            //        if (!ev.TryConvert(et, out object v))
            //            return false;
            //        a.SetValue(v, i++);
            //    }

            //    value = a;
            //    return true;
            //}
            return false;
        }
    }
}
