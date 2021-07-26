﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SecurityDriven.Inferno
{
	public static class Utils
	{
		internal const bool AllowOnlyFipsAlgorithms = true; // cache the FIPS flag

		public static readonly UTF8Encoding SafeUTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		#region CreateSetter<T,V>
		// static reflection setter for fields
		internal static Action<T, V> CreateSetter<T, V>(this FieldInfo field)
		{
			var targetExp = Expression.Parameter(typeof(T));
			var valueExp = Expression.Parameter(typeof(V));

			// Expression.Property can be used here as well
			var fieldExp = Expression.Field(targetExp, field);
			var assignExp = Expression.Assign(fieldExp, valueExp);

			var setter = Expression.Lambda<Action<T, V>>(assignExp, targetExp, valueExp).Compile();
			return setter;
		}// CreateSetter()
		#endregion

		#region CreateGetter<T,V>
		internal static Func<T, V> CreateGetter<T, V>(this FieldInfo field)
		{
			var targetExp = Expression.Parameter(typeof(T));

			// Expression.Property can be used here as well
			var fieldExp = Expression.Field(targetExp, field);

			var getter = Expression.Lambda<Func<T, V>>(fieldExp, targetExp).Compile();
			return getter;
		}// CreateGetter()
		#endregion

		#region ConstantTimeEqual() - byte arrays & ArraySegments
		/// <exception cref="System.NullReferenceException">Thrown when either array is null.</exception>
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public static bool ConstantTimeEqual(byte[] x, int xOffset, byte[] y, int yOffset, int length)
		{
			// based on https://github.com/CodesInChaos/Chaos.NaCl/blob/55e84738252932fa123eaa7bb0dd9cb99de0ceb9/Chaos.NaCl/CryptoBytes.cs (public domain)
			// another sanity reference: https://golang.org/src/crypto/subtle/constant_time.go
			// Null checks of "x" and "y" are skipped. Appropriate exceptions will be raised anyway.

			if (xOffset < 0)
				throw new ArgumentOutOfRangeException("xOffset", "xOffset < 0");
			if (yOffset < 0)
				throw new ArgumentOutOfRangeException("yOffset", "yOffset < 0");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "length < 0");
			if (checked(xOffset + length) > x.Length)
				throw new ArgumentException("xOffset + length > x.Length");
			if (checked(yOffset + length) > y.Length)
				throw new ArgumentException("yOffset + length > y.Length");

			int differentbits = 0;
			unchecked
			{
				for (int i = 0; i < length; ++i)
				{
					differentbits |= x[xOffset + i] ^ y[yOffset + i];
				}
			}
			return differentbits == 0;
		}// ConstantTimeEqual()

		public static bool ConstantTimeEqual(ArraySegment<byte> x, ArraySegment<byte> y)
		{
			int xCount = x.Count;
			if (xCount != y.Count)
				throw new ArgumentException("x.Count must equal y.Count");

			return ConstantTimeEqual(x.Array, x.Offset, y.Array, y.Offset, xCount);
		}// ConstantTimeEqual()

		/// <exception cref="System.NullReferenceException">Thrown when either array is null.</exception>
		public static bool ConstantTimeEqual(byte[] x, byte[] y)
		{
			int xLength = x.Length;
			if (xLength != y.Length)
				throw new ArgumentException("x.Length must equal y.Length");

			return ConstantTimeEqual(x, 0, y, 0, xLength);
		}// ConstantTimeEqual()
		#endregion

		#region ConstantTimeEqual() - strings
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		static bool ConstantTimeEqual(string x, int xOffset, string y, int yOffset, int length)
		{
			// Null checks of "x" and "y" are skipped. Appropriate exceptions will be raised anyway.
			if (xOffset < 0)
				throw new ArgumentOutOfRangeException("xOffset", "xOffset < 0");
			if (yOffset < 0)
				throw new ArgumentOutOfRangeException("yOffset", "yOffset < 0");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "length < 0");
			if (checked(xOffset + length) > x.Length)
				throw new ArgumentException("xOffset + length > x.Length");
			if (checked(yOffset + length) > y.Length)
				throw new ArgumentException("yOffset + length > y.Length");

			int differentbits = 0;
			unchecked
			{
				for (int i = 0; i < length; ++i)
				{
					differentbits |= x[xOffset + i] ^ y[yOffset + i];
				}
			}
			return differentbits == 0;
		}// ConstantTimeEqual()

		/// <exception cref="System.NullReferenceException">Thrown when either string is null.</exception>
		public static bool ConstantTimeEqual(string x, string y)
		{
			int xLength = x.Length;
			if (xLength != y.Length)
				throw new ArgumentException("x.Length must equal y.Length");

			return ConstantTimeEqual(x, 0, y, 0, xLength);
		}// ConstantTimeEqual()
		#endregion

		#region IntStruct
		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct IntStruct
		{
			[FieldOffset(0)]
			public int IntValue;
			[FieldOffset(0)]
			public uint UintValue;

			[FieldOffset(0)]
			public byte B1;
			[FieldOffset(1)]
			public byte B2;
			[FieldOffset(2)]
			public byte B3;
			[FieldOffset(3)]
			public byte B4;

			/// <summary>
			/// To Big-Endian
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ToBEBytes(byte[] buffer, int offset = 0)
			{
				if (BitConverter.IsLittleEndian)
				{
					Unsafe.As<byte, uint>(ref buffer[offset]) = Utils.ReverseEndianness(UintValue);
				}
				else
				{
					Unsafe.As<byte, uint>(ref buffer[offset]) = UintValue;
				}
			}// ToBEBytes()
		}// IntStruct
		#endregion

		#region LongStruct
		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct LongStruct
		{
			[FieldOffset(0)]
			public long LongValue;
			[FieldOffset(0)]
			public ulong UlongValue;

			[FieldOffset(0)]
			public byte B1;
			[FieldOffset(1)]
			public byte B2;
			[FieldOffset(2)]
			public byte B3;
			[FieldOffset(3)]
			public byte B4;
			[FieldOffset(4)]
			public byte B5;
			[FieldOffset(5)]
			public byte B6;
			[FieldOffset(6)]
			public byte B7;
			[FieldOffset(7)]
			public byte B8;

			/// <summary>
			/// To Big-Endian
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ToBEBytes(byte[] buffer, int offset = 0)
			{
				if (BitConverter.IsLittleEndian)
				{
					Unsafe.As<byte, ulong>(ref buffer[offset]) = Utils.ReverseEndianness(UlongValue);
				}
				else
				{
					Unsafe.As<byte, ulong>(ref buffer[offset]) = UlongValue;
				}
			}// ToBEBytes()
		}// LongStruct
		#endregion

		internal static readonly Action<Array, int, Array, int, int> BlockCopy = Buffer.BlockCopy;

		#region Combine byte arrays & segments
		public static byte[] Combine(ArraySegment<byte> a, ArraySegment<byte> b)
		{
			byte[] combinedArray = new byte[checked(a.Count + b.Count)];
			BlockCopy(a.Array, a.Offset, combinedArray, 0, a.Count);
			BlockCopy(b.Array, b.Offset, combinedArray, a.Count, b.Count);
			return combinedArray;
		}// Combine(two byte array segments)

		public static byte[] Combine(byte[] a, byte[] b) { return Combine(a.AsArraySegment(), b.AsArraySegment()); }// Combine(two byte arrays)

		public static byte[] Combine(ArraySegment<byte> a, ArraySegment<byte> b, ArraySegment<byte> c)
		{
			byte[] combinedArray = new byte[checked(a.Count + b.Count + c.Count)];
			BlockCopy(a.Array, a.Offset, combinedArray, 0, a.Count);
			BlockCopy(b.Array, b.Offset, combinedArray, a.Count, b.Count);
			BlockCopy(c.Array, c.Offset, combinedArray, a.Count + b.Count, c.Count);
			return combinedArray;
		}// Combine(three byte array segments)

		public static byte[] Combine(byte[] a, byte[] b, byte[] c) { return Combine(a.AsArraySegment(), b.AsArraySegment(), c.AsArraySegment()); }// Combine(three byte arrays)

		public static byte[] Combine(params byte[][] arrays)
		{
			int combinedArrayLength = 0, combinedArrayOffset = 0;
			for (int i = 0; i < arrays.Length; ++i) checked { combinedArrayLength += arrays[i].Length; }
			byte[] array, combinedArray = new byte[combinedArrayLength];

			for (int i = 0; i < arrays.Length; ++i)
			{
				array = arrays[i];
				BlockCopy(array, 0, combinedArray, combinedArrayOffset, array.Length);
				combinedArrayOffset += array.Length;
			}
			return combinedArray;
		}// Combine(params byte[][])

		public static byte[] Combine(params ArraySegment<byte>[] arraySegments)
		{
			int combinedArrayLength = 0, combinedArrayOffset = 0;
			for (int i = 0; i < arraySegments.Length; ++i) checked { combinedArrayLength += arraySegments[i].Count; }
			byte[] combinedArray = new byte[combinedArrayLength];

			for (int i = 0; i < arraySegments.Length; ++i)
			{
				var segment = arraySegments[i];
				BlockCopy(segment.Array, segment.Offset, combinedArray, combinedArrayOffset, segment.Count);
				combinedArrayOffset += segment.Count;
			}
			return combinedArray;
		}// Combine(params ArraySegment<byte>[])
		#endregion

		#region Xor
		[StructLayout(LayoutKind.Explicit, Pack = 0)]
		internal struct Union
		{
			[FieldOffset(0)]
			public byte[] Bytes;

			[FieldOffset(0)]
			public long[] Longs;
		}// struct Union

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Xor(byte[] dest, int destOffset, byte[] left, int leftOffset, byte[] right, int rightOffset, int byteCount)
		{
			int i = 0;
			if ((byteCount > Extensions.ByteArrayExtensions.SHORT_BYTECOPY_THRESHOLD) && (((destOffset | leftOffset | rightOffset) & 7) == 0)) // all offsets must be multiples of 8 for long-sized xor
			{
				long[] destUnionLongs = new Union { Bytes = dest }.Longs, leftUnionLongs = new Union { Bytes = left }.Longs, rightUnionLongs = new Union { Bytes = right }.Longs;
				int longDestOffset = destOffset >> 3, longLeftOffset = leftOffset >> 3, longRightOffset = rightOffset >> 3, longCount = byteCount >> 3;
				for (; i < longCount; ++i) destUnionLongs[longDestOffset + i] = leftUnionLongs[longLeftOffset + i] ^ rightUnionLongs[longRightOffset + i];
				i = longCount << 3;
			}
			for (; i < byteCount; ++i) dest[destOffset + i] = (byte)(left[leftOffset + i] ^ right[rightOffset + i]);
		}// Xor()

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Xor(byte[] dest, int destOffset, byte[] left, int leftOffset, int byteCount)
		{
			int i = 0;
			if ((byteCount > Extensions.ByteArrayExtensions.SHORT_BYTECOPY_THRESHOLD) && (((destOffset | leftOffset) & 7) == 0)) // all offsets must be multiples of 8 for long-sized xor
			{
				long[] destUnionLongs = new Union { Bytes = dest }.Longs, leftUnionLongs = new Union { Bytes = left }.Longs;
				int longDestOffset = destOffset >> 3, longLeftOffset = leftOffset >> 3, longCount = byteCount >> 3;
				for (; i < longCount; ++i) destUnionLongs[longDestOffset + i] ^= leftUnionLongs[longLeftOffset + i];
				i = longCount << 3;
			}
			for (; i < byteCount; ++i) dest[destOffset + i] ^= left[leftOffset + i];
		}// Xor()
		#endregion

		#region ReverseEndianness
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ulong ReverseEndianness(ulong value)
		{
			return ((ulong)ReverseEndianness((uint)value) << 32) + ReverseEndianness((uint)(value >> 32));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static uint ReverseEndianness(uint value)
		{
			(uint xx_zz, uint ww_yy) = (value & 0x00FF00FF, value & 0xFF00FF00);
			return ((xx_zz >> 8) | (xx_zz << 24)) | ((ww_yy << 8) | (ww_yy >> 24));
		}
		#endregion
	}// class Utils

	public static class ArraySegmentExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArraySegment<T> AsArraySegment<T>(this T[] arr) => new ArraySegment<T>(arr);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArraySegment<T>? AsNullableArraySegment<T>(this T[] arr) => new ArraySegment<T>?(new ArraySegment<T>(arr));
	}// class ArraySegmentExtensions
}//ns