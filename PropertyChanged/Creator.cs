using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PropertyChanged
{
	public static class Creator
	{
		static IDictionary<Type, Type> types = new Dictionary<Type, Type>();
		static AssemblyBuilder assemblyBuilder { get; }
		static ModuleBuilder moduleBuilder { get; }
		static Creator()
		{
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Dynamic"), AssemblyBuilderAccess.RunAndCollect);
			moduleBuilder = assemblyBuilder.DefineDynamicModule("Dynamic.dll", "Dynamic.dll");
		}
		static Type CreateType<T>(params object[] args) where T : BindableBase
		{
			var typeBuilder = moduleBuilder.DefineType("<Dynamic>" + typeof(T).Name, TypeAttributes.Public, typeof(T));

			CreatePassThroughConstructors(typeBuilder, typeof(T));
			foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite && p.SetMethod.IsVirtual && !p.SetMethod.IsAbstract && p.CanRead && !p.GetMethod.IsAbstract))
				OverridePropertySetter(typeBuilder, property);

			return types[typeof(T)] = typeBuilder.CreateType();
		}

		public static T CreateViewModel<T>(params object[] args) where T : BindableBase
		{
			return (T)Activator.CreateInstance(types.ContainsKey(typeof(T)) ? types[typeof(T)] : CreateType<T>(args), args);
		}

		public static void OverridePropertySetter(TypeBuilder typeBuilder, PropertyInfo property)
		{
			var propertyBuilder = typeBuilder.DefineProperty(property.Name, System.Reflection.PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);

			var methodBuilder = typeBuilder.DefineMethod(
				property.SetMethod.Name,
				(MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | property.SetMethod.Attributes) & ~MethodAttributes.Abstract & ~MethodAttributes.NewSlot,
				null,
				new[] { property.PropertyType });

			var equalityComparer = Type.GetType("System.Collections.Generic.EqualityComparer`1").MakeGenericType(property.PropertyType);

			var il = methodBuilder.GetILGenerator();
			var ret = il.DefineLabel();
			il.DeclareLocal(property.PropertyType);
			il.Emit(OpCodes.Nop);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Callvirt, property.GetMethod);
			il.Emit(OpCodes.Stloc_0);
			il.Emit(OpCodes.Call, equalityComparer.GetProperty("Default").GetMethod);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, equalityComparer.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
			il.Emit(OpCodes.Brtrue_S, ret);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, property.Name);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, typeof(BindableBase).GetMethod("RaisePropertyChanging", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.NonPublic).MakeGenericMethod(property.PropertyType));
			il.Emit(OpCodes.Brtrue_S, ret);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, property.SetMethod);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, property.Name);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, typeof(BindableBase).GetMethod("RaisePropertyChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.NonPublic).MakeGenericMethod(property.PropertyType));
			il.MarkLabel(ret);
			il.Emit(OpCodes.Ret);

			propertyBuilder.SetSetMethod(methodBuilder);
		}

		// http://stackoverflow.com/questions/6879279/using-typebuilder-to-create-a-pass-through-constructor-for-the-base-class

		/// <summary>Creates one constructor for each public constructor in the base class. Each constructor simply
		/// forwards its arguments to the base constructor, and matches the base constructor's signature.
		/// Supports optional values, and custom attributes on constructors and parameters.
		/// Does not support n-ary (variadic) constructors</summary>
		public static void CreatePassThroughConstructors(this TypeBuilder builder, Type baseType)
		{
			foreach (var constructor in baseType.GetConstructors())
			{
				var parameters = constructor.GetParameters();
				if (parameters.Length > 0 && parameters.Last().IsDefined(typeof(ParamArrayAttribute), false))
				{
					//throw new InvalidOperationException("Variadic constructors are not supported");
					continue;
				}

				var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
				var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
				var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

				var ctor = builder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
				for (var i = 0; i < parameters.Length; ++i)
				{
					var parameter = parameters[i];
					var parameterBuilder = ctor.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
					if (((int)parameter.Attributes & (int)ParameterAttributes.HasDefault) != 0)
					{
						parameterBuilder.SetConstant(parameter.RawDefaultValue);
					}

					foreach (var attribute in BuildCustomAttributes(parameter.GetCustomAttributesData()))
					{
						parameterBuilder.SetCustomAttribute(attribute);
					}
				}

				foreach (var attribute in BuildCustomAttributes(constructor.GetCustomAttributesData()))
				{
					ctor.SetCustomAttribute(attribute);
				}

				var il = ctor.GetILGenerator();
				il.Emit(OpCodes.Nop);

				// Load `this` and call base constructor with arguments
				il.Emit(OpCodes.Ldarg_0);
				for (var i = 1; i <= parameters.Length; ++i)
				{
					il.Emit(OpCodes.Ldarg, i);
				}
				il.Emit(OpCodes.Call, constructor);

				il.Emit(OpCodes.Ret);
			}
		}

		private static CustomAttributeBuilder[] BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
		{
			return customAttributes.Select(attribute =>
			{
				var attributeArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
				var namedPropertyInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
				var namedPropertyValues = attribute.NamedArguments.Where(a => a.MemberInfo is PropertyInfo).Select(a => a.TypedValue.Value).ToArray();
				var namedFieldInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<FieldInfo>().ToArray();
				var namedFieldValues = attribute.NamedArguments.Where(a => a.MemberInfo is FieldInfo).Select(a => a.TypedValue.Value).ToArray();
				return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, namedPropertyInfos, namedPropertyValues, namedFieldInfos, namedFieldValues);
			}).ToArray();
		}
	}
}