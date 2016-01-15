using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PropertyChanged;

namespace PropertyChangedTests
{
	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void TestPropertyChangedInt()
		{
			var t = Creator.CreateViewModel<TestClass>();
			var eventRaised = false;
			t.IntProperty = 1;
			t.PropertyChanged += (s, e) =>
			{
				Assert.AreEqual(e.PropertyName, "IntProperty");
				var ee = e.As<int>();
				Assert.AreEqual(ee.OldValue, 1);
				Assert.AreEqual(ee.CurrentValue, 2);
				eventRaised = true;
			};
			t.IntProperty = 2;
			Assert.AreEqual(eventRaised, true);
		}

		[TestMethod]
		public void TestPropertyChangedString()
		{
			var t = Creator.CreateViewModel<TestClass>();
			var eventRaised = false;
			t.StringProperty = "one";
			t.PropertyChanged += (s, e) =>
			{
				Assert.AreEqual(e.PropertyName, "StringProperty");
				var ee = e.As<string>();
				Assert.AreEqual(ee.OldValue, "one");
				Assert.AreEqual(ee.CurrentValue, "two");
				eventRaised = true;
			};
			t.StringProperty = "two";
			Assert.AreEqual(eventRaised, true);
		}

		[TestMethod]
		public void TestPropertyChangingInt()
		{
			var t = Creator.CreateViewModel<TestClass>();
			var eventRaised = false;
			t.IntProperty = 1;

			t.PropertyChanging += (s, e) =>
			{
				Assert.AreEqual(e.PropertyName, "IntProperty");
				var ee = e.As<int>();
				Assert.AreEqual(ee.CurrentValue, 1);
				Assert.AreEqual(ee.NewValue, 2);
				eventRaised = true;
			};

			t.IntProperty = 2;

			Assert.AreEqual(eventRaised, true);
		}

		[TestMethod]
		public void TestPropertyChangingString()
		{
			var t = Creator.CreateViewModel<TestClass>();
			var eventRaised = false;
			t.StringProperty = "one";

			t.PropertyChanging += (s, e) =>
			{
				Assert.AreEqual(e.PropertyName, "StringProperty");
				var ee = e.As<string>();
				Assert.AreEqual(ee.CurrentValue, "one");
				Assert.AreEqual(ee.NewValue, "two");
				eventRaised = true;
			};

			t.StringProperty = "two";

			Assert.AreEqual(eventRaised, true);
		}

		[TestMethod]
		public void TestCancel()
		{
			var t = Creator.CreateViewModel<TestClass>();
			t.IntProperty = 1;
			t.StringProperty = "one";

			t.PropertyChanging += (s, e) => e.AsCancellable().Cancel = true;

			t.IntProperty = 2;
			t.StringProperty = "two";

			Assert.AreEqual(t.IntProperty, 1);
			Assert.AreEqual(t.StringProperty, "one");
		}
	}
}
