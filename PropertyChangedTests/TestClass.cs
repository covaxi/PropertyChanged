using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyChangedTests
{
	public class TestClass : BindableBase
	{
		public virtual int IntProperty { get; set; }
		public virtual string StringProperty { get; set; }
	}
}
