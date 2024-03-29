﻿using System;
using System.Buffers;

namespace Wombat.Extensions.JsonRpc
{
    public struct RentedBuffer : IDisposable
    {
        private readonly IMemoryOwner<byte> _memory;
        private readonly int _length;

        public bool IsEmpty => _memory == null;
        public Span<byte> Span => ( _memory != null) ? _memory.Memory.Span.Slice(0, _length) : Span<byte>.Empty;

        public RentedBuffer(IMemoryOwner<byte> memory, int length)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
            _length = length;
        }

        public void Dispose()
        {
            _memory.Dispose();
        }
    }
}
