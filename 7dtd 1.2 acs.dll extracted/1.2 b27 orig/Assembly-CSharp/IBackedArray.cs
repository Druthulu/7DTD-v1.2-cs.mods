﻿using System;
using System.Runtime.CompilerServices;

public interface IBackedArray<[IsUnmanaged] T> : IDisposable where T : struct, ValueType
{
	int Length { get; }

	IBackedArrayHandle GetMemory(int start, int length, out Memory<T> memory);

	IBackedArrayHandle GetReadOnlyMemory(int start, int length, out ReadOnlyMemory<T> memory);

	unsafe IBackedArrayHandle GetMemoryUnsafe(int start, int length, out T* arrayPtr);

	unsafe IBackedArrayHandle GetReadOnlyMemoryUnsafe(int start, int length, out T* arrayPtr);

	IBackedArrayHandle GetSpan(int start, int length, out Span<T> span);

	IBackedArrayHandle GetReadOnlySpan(int start, int length, out ReadOnlySpan<T> span);
}
