﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Wombat.Extensions.JsonRpc.Server
{
    public partial class JsonRpcServer
    {
        private class HandlerInfo
        {
            public object Instance { get; internal set; }
            public MethodInfo Method { get; internal set; }
            public ParameterInfo[] Parameters { get; internal set; }
        }

        readonly ConcurrentDictionary<string, HandlerInfo> _handlers = new ConcurrentDictionary<string, HandlerInfo>();

        public void Bind(object handler, string prefix = "")
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (prefix.Any(c => char.IsWhiteSpace(c)))
                throw new ArgumentException("Prefix string can not contain any whitespace");

            foreach (var m in handler.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                RegisterMethod(handler.GetType().Name, prefix, m, handler);
        }

        public void Bind(Type type, string prefix = "")
        {
            if (prefix.Any(c => char.IsWhiteSpace(c)))
                throw new ArgumentException("Prefix string can not contain any whitespace");

            foreach (var m in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
                RegisterMethod(type.Name, prefix, m);
        }

        private void RegisterMethod(string className, string prefix, MethodInfo m, object instance = null)
        {
            var name = prefix + m.Name;
            if (_handlers.TryGetValue(name, out var existing))
                throw new JsonRpcException($"The method '{name}' is already handled by the class {existing.Instance.GetType().Name}");

            var info = new HandlerInfo {
                Instance = instance,
                Method = m,
                Parameters = m.GetParameters()
            };
            _handlers.TryAdd(name, info);
            Debug.WriteLine($"Added handler for '{name}' on {className}.{m.Name}");
        }

        public void Bind(string method, Delegate call)
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Can not be null or empty", nameof(method));
            if (call == null)
                throw new ArgumentNullException(nameof(call));

            if (_handlers.TryGetValue(method, out var existing))
                throw new JsonRpcException($"The method '{method}' is already handled by the class {existing.Instance.GetType().Name}");

            var info = new HandlerInfo {
                Instance = call.Target,
                Method = call.Method,
                Parameters = call.Method.GetParameters()
            };
            _handlers.TryAdd(method, info);
            Debug.WriteLine($"Added handler for '{method}' on Delegate");
        }

        internal void ExecuteHandler(IClient client, int? id, string method, object[] args)
        {
            if (_handlers.TryGetValue(method, out var info))
            {
                try
                {
                    // Make sure parameters are correct for the function call
                    FixParameters(info, ref args);

                    // Now actually do the actual function call on the users class
                    object result = Invoke(args, info);
                    if (!id.HasValue)
                        return;     // Was a notify, so don't reply

                    // Reply to client
                    if (info.Method.ReturnParameter.ParameterType != typeof(void))
                    {
                        SendResponse(client, id.Value, result);
                    }
                    else
                    {
                        SendResponse(client, id.Value);
                    }
                }
                catch (Exception ex)
                {
                    if(id.HasValue)
                        SendError(client, id.Value, $"Handler '{method}' threw an exception: {ex.Message}");
                }
            }
            else
            {
                if(id.HasValue)
                    SendUnknownMethodError(client, id.Value, method);
            }
        }

        private static void SendError(IClient client, int id, string message)
        {
            var response = new Response() {
                JsonRpc = "2.0",
                Id = id,
                Error = new Error() { Code = -1, Message = message }
            };
            client.WriteAsJson(response);
        }

        private static void SendUnknownMethodError(IClient client, int id, string method)
        {
            var response = new Response() {
                JsonRpc = "2.0",
                Id = id,
                Error = new Error() { Code = -32601, Message = $"Unknown method '{method}'" }
            };
            client.WriteAsJson(response);
        }

        private static void SendResponse(IClient client, int id)
        {
            var response = new Response() {
                JsonRpc = "2.0",
                Id = id
            };
            client.WriteAsJson(response);
        }

        private static void SendResponse(IClient client, int id, object result)
        {
            var response = new Response<object>() {
                JsonRpc = "2.0",
                Id = id,
                Result = result
            };
            client.WriteAsJson(response);
        }

        private static object Invoke(object[] args, HandlerInfo info)
        {
            // Use reflection invoke instead of delegate because we have optional parameters
            if (info.Instance != null)
            {
                // Instance function
                return info.Method.Invoke(info.Instance,
                    BindingFlags.OptionalParamBinding | BindingFlags.InvokeMethod | BindingFlags.CreateInstance, null, args, null);
            }
            else
            {
                // Static function
                return info.Method.Invoke(null, BindingFlags.OptionalParamBinding | BindingFlags.InvokeMethod, null, args, null);
            }
        }

        private void FixParameters(HandlerInfo info, ref object[] args)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if(info.Parameters == null)
                throw new ArgumentException("info.Parameters can not be null");
            if (args == null)
                return;

            int neededArgs = info.Parameters.Count(x => !x.HasDefaultValue);
            if (neededArgs > args.Length)
                throw new JsonRpcException($"Argument count mismatch (Expected at least {neededArgs}, but got only {args.Length}");

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    continue;  // Skip

                var inputType = args[i].GetType();
                var outputType = info.Parameters[i].ParameterType;

                if (inputType == outputType)
                    continue;  // Already the right type

                if (inputType.IsPrimitive)
                {
                    // Cast primitives
                    args[i] = Convert.ChangeType(args[i], outputType);
                }
                else if (inputType == typeof(string))
                {
                    // Convert string to TimeSpan
                    if (outputType == typeof(TimeSpan))
                        args[i] = TimeSpan.Parse((string)args[i]);
                    // Convert string to DateTime
                    else if (outputType == typeof(DateTime))
                        args[i] = DateTime.Parse((string)args[i]);
                    // Convert string to DateTimeOffset
                    else if (outputType == typeof(DateTimeOffset))
                        args[i] = DateTimeOffset.Parse((string)args[i]);
                }
                else if (inputType == typeof(List<object>))
                {
                    var list = (List<object>)args[i];
                    args[i] = MakeTypedArray(outputType.GetElementType(), list);
                }
                //else if (at == typeof(JArray))
                //{
                //    // Convert array to a real typed array
                //    var a = args[i] as JArray;
                //    args[i] = a.ToObject(outputType);
                //}
                else
                {
                    Serializer.ConvertToConcreteType(inputType, outputType, ref args[i]);
                }
            }

            if (args.Length < info.Parameters.Length)
            {
                // Missing optional arguments are set to Type.Missing
                args = args.Concat(Enumerable.Repeat(Type.Missing, info.Parameters.Length - args.Length)).ToArray();
            }
        }

        /// <summary>
        /// Create a new array with the target element type and copy values over
        /// </summary>
        private static Array MakeTypedArray(Type elementType, List<object> list)
        {
            if (list == null)
                return null;

            var a = Array.CreateInstance(elementType, list.Count);
            for (int j = 0; j < list.Count; j++)
                a.SetValue(list[j], j);
            return a;
        }
    }
}
