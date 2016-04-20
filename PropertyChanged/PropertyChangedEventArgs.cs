using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullReference.PropertyChanged
{
	public class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
	{
		public T OldValue { get; }
		public T CurrentValue { get; }
		public PropertyChangedEventArgs(string propertyName, T oldValue, T currentValue) : base(propertyName)
		{
			OldValue = oldValue;
			CurrentValue = currentValue;
		}
	}
}
