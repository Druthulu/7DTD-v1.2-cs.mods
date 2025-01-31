﻿using System;
using System.Collections.Generic;

namespace UniLinq
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class OrderedSequence<TElement, TKey> : OrderedEnumerable<TElement>
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public OrderedSequence(IEnumerable<TElement> source, Func<TElement, TKey> key_selector, IComparer<TKey> comparer, SortDirection direction) : base(source)
		{
			this.selector = key_selector;
			this.comparer = (comparer ?? Comparer<TKey>.Default);
			this.direction = direction;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public OrderedSequence(OrderedEnumerable<TElement> parent, IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, SortDirection direction) : this(source, keySelector, comparer, direction)
		{
			this.parent = parent;
		}

		public override IEnumerator<TElement> GetEnumerator()
		{
			return base.GetEnumerator();
		}

		public override SortContext<TElement> CreateContext(SortContext<TElement> current)
		{
			SortContext<TElement> sortContext = new SortSequenceContext<TElement, TKey>(this.selector, this.comparer, this.direction, current);
			if (this.parent != null)
			{
				return this.parent.CreateContext(sortContext);
			}
			return sortContext;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerable<TElement> Sort(IEnumerable<TElement> source)
		{
			return QuickSort<TElement>.Sort(source, this.CreateContext(null));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public OrderedEnumerable<TElement> parent;

		[PublicizedFrom(EAccessModifier.Private)]
		public Func<TElement, TKey> selector;

		[PublicizedFrom(EAccessModifier.Private)]
		public IComparer<TKey> comparer;

		[PublicizedFrom(EAccessModifier.Private)]
		public SortDirection direction;
	}
}
