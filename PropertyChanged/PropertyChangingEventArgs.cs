using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChanged
{
	public class CancellablePropertyChangingEventArgs : PropertyChangingEventArgs
	{
		public bool Cancel { get; set; }

		public CancellablePropertyChangingEventArgs(string propertyName) : base(propertyName)
		{

		}
	}

	public class PropertyChangingEventArgs<T> : CancellablePropertyChangingEventArgs
	{
		public T CurrentValue { get; }
		public T NewValue { get; }
		
		public PropertyChangingEventArgs(string propertyName, T currentValue, T newValue) : base(propertyName)
		{
			CurrentValue = currentValue;
			NewValue = newValue;
		}
	}

}
