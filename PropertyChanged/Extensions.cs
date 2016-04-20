using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullReference.PropertyChanged
{
	public static class Extensions
	{
		public static PropertyChangedEventArgs<T> As<T>(this PropertyChangedEventArgs args)
		{
			return (PropertyChangedEventArgs<T>)args;
		}

		public static PropertyChangingEventArgs<T> As<T>(this PropertyChangingEventArgs args)
		{
			return (PropertyChangingEventArgs<T>)args;
		}

		public static CancellablePropertyChangingEventArgs AsCancellable(this PropertyChangingEventArgs args)
		{
			return (CancellablePropertyChangingEventArgs)args;
		}
	}
}
