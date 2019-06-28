using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Wraps an object that provides a span of bytes, for example a Memory object.
    /// </summary>
    public interface IBuffer
    {
        Span<byte> Bytes { get; }
    }

    /// <summary>
    /// Represents a buffer with a name 
    /// </summary>
    public interface INamedBuffer : IBuffer
    {
        string Name { get; }
    }

    /// <summary>
    /// A memory buffer is a concrete implementation of IBuffer
    /// </summary>
    public class MemoryBuffer<T> : IBuffer where T : struct
    {
        public MemoryBuffer(Memory<T> memory) => Memory = memory;
        public Memory<T> Memory { get; }
        public Span<byte> Bytes => MemoryMarshal.Cast<T, byte>(Memory.Span);
    }

    /// <summary>
    /// A memory buffer with a name which implements INamedBuffer
    /// </summary>
    public class NamedMemoryBuffer<T> : MemoryBuffer<T>, INamedBuffer where T : struct
    {
        public NamedMemoryBuffer(Memory<T> memory, string name = "")
            : base(memory)
        => Name = name;

        public string Name { get; }
    }

    /// <summary>
    /// Helper functions for working with buffers 
    /// </summary>
    public static class BufferExtensions
    {
        public static IBuffer ToBuffer<T>(this Memory<T> memory) where T : struct
            => new MemoryBuffer<T>(memory);

        public static IBuffer ToBuffer<T>(this T[] xs) where T : struct
            => new Memory<T>(xs).ToBuffer();

        public static Span<T> AsSpan<T>(this IBuffer buffer) where T : struct
            => MemoryMarshal.Cast<byte, T>(buffer.Bytes);

        public static string AsString(this IBuffer buffer)
            => System.Text.Encoding.UTF8.GetString(buffer.Bytes.ToArray());

        public static string[] AsStrings(this IBuffer buffer)
            => buffer.AsString().Split('\0');
    }
}