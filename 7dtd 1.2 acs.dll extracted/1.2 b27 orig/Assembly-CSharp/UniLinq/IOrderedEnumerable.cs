using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	public interface IOrderedEnumerable<TElement> : IEnumerable<!0>, IEnumerable
	{
		IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
	}
}
