using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NullReference.PropertyChanged;

namespace NullReference.Tests.PropertyChanged
{
	public class TestClass : BindableBase
	{
		public virtual int IntProperty { get; set; }
		public virtual string StringProperty { get; set; }
	}
}
