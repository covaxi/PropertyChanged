using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullReference.PropertyChanged
{
	public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyChanging
	{
		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual event PropertyChangingEventHandler PropertyChanging;
		protected virtual void RaisePropertyChanged<T>(string propertyName, T oldValue, T currentValue) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs<T>(propertyName, oldValue, currentValue));
		protected virtual bool RaisePropertyChanging<T>(string propertyName, T currentValue, T newValue)
		{
			var args = new PropertyChangingEventArgs<T>(propertyName, currentValue, newValue);
			PropertyChanging?.Invoke(this, args);
			return args.Cancel;
		}
	}
}
