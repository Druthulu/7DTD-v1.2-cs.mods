﻿using System;
using System.Collections.Generic;

public class CachedStringFormatter<T1, T2, T3, T4, T5>
{
	public CachedStringFormatter(global::Func<T1, T2, T3, T4, T5, string> _formatterFunc)
	{
		this.formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4, T5 _v5)
	{
		bool flag;
		return this.Format(_v1, _v2, _v3, _v4, _v5, out flag);
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4, T5 _v5, out bool _valueChanged)
	{
		_valueChanged = (this.cachedResult == null);
		if (!this.comparer1.Equals(this.oldValue1, _v1))
		{
			this.oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!this.comparer2.Equals(this.oldValue2, _v2))
		{
			this.oldValue2 = _v2;
			_valueChanged = true;
		}
		if (!this.comparer3.Equals(this.oldValue3, _v3))
		{
			this.oldValue3 = _v3;
			_valueChanged = true;
		}
		if (!this.comparer4.Equals(this.oldValue4, _v4))
		{
			this.oldValue4 = _v4;
			_valueChanged = true;
		}
		if (!this.comparer5.Equals(this.oldValue5, _v5))
		{
			this.oldValue5 = _v5;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			this.cachedResult = this.formatter(_v1, _v2, _v3, _v4, _v5);
		}
		return this.cachedResult;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly global::Func<T1, T2, T3, T4, T5, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T3 oldValue3;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T3> comparer3 = EqualityComparer<T3>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T4 oldValue4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T4> comparer4 = EqualityComparer<T4>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T5 oldValue5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T5> comparer5 = EqualityComparer<T5>.Default;
}
