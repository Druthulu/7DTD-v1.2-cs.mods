﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

public static class ReflectionHelpers
{
	public static void FindTypesImplementingBase(Type _searchType, Action<Type> _typeFoundCallback, bool _allowAbstract = false)
	{
		try
		{
			List<Assembly> loadedAssemblies = ModManager.GetLoadedAssemblies();
			loadedAssemblies.Insert(0, Assembly.GetExecutingAssembly());
			for (int i = 0; i < loadedAssemblies.Count; i++)
			{
				Assembly assembly = loadedAssemblies[i];
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsClass && (_allowAbstract || !type.IsAbstract) && _searchType.IsAssignableFrom(type))
						{
							try
							{
								if (i == 0 && Attribute.GetCustomAttribute(type, typeof(PreserveAttribute)) == null)
								{
									Log.Error(string.Format("Type:{0} is missing the UnityEngine.Scripting.Preserve attribute, this will fail on consoles", type));
								}
								if (_typeFoundCallback != null)
								{
									_typeFoundCallback(type);
								}
							}
							catch (Exception e)
							{
								Log.Error("Error invoking found type callback for '" + type.FullName + "'");
								Log.Exception(e);
							}
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Log.Error(string.Concat(new string[]
					{
						"Error loading types from assembly '",
						assembly.FullName,
						"' (",
						assembly.Location,
						")"
					}));
					Log.Exception(ex);
					Console.WriteLine();
					Console.WriteLine("Successfully loaded Types:");
					int num = 1;
					foreach (Type type2 in ex.Types)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (type2 != null) ? type2.FullName : "NULL"));
					}
					Console.WriteLine();
					Console.WriteLine("Exceptions:");
					num = 1;
					foreach (Exception ex2 in ex.LoaderExceptions)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (ex2 != null) ? ex2.Message : "NULL"));
					}
					Console.WriteLine();
				}
				catch (Exception e2)
				{
					Log.Error("Error loading types from assembly " + assembly.Location);
					Log.Exception(e2);
				}
			}
		}
		catch (Exception e3)
		{
			Log.Error("Error loading types");
			Log.Exception(e3);
		}
	}

	public static T Instantiate<T>(Type _type) where T : class
	{
		try
		{
			if (_type.Assembly == Assembly.GetExecutingAssembly() && Attribute.GetCustomAttribute(_type, typeof(PreserveAttribute)) == null)
			{
				Log.Error(string.Format("Type:{0} is missing the UnityEngine.Scripting.Preserve attribute, this will fail on consoles", _type));
			}
			ConstructorInfo constructor = _type.GetConstructor(Type.EmptyTypes);
			if (constructor != null)
			{
				return (T)((object)constructor.Invoke(Array.Empty<object>()));
			}
			Log.Warning("Class '" + _type.FullName + "' does not contain a parameterless constructor, skipping");
		}
		catch (Exception e)
		{
			Log.Error("Could not instantiate type '" + _type.FullName + "'");
			Log.Exception(e);
		}
		return default(T);
	}

	public static Type GetTypeWithPrefix(string _prefix, string _name)
	{
		int num = _name.IndexOfAny(ReflectionHelpers.classnameEndMarkers);
		if (num < 0)
		{
			num = _name.Length - 1;
		}
		int num2 = _name.LastIndexOf('.', num);
		string typeName;
		if (num2 >= 0)
		{
			typeName = _name.Insert(num2 + 1, _prefix);
		}
		else
		{
			typeName = _prefix + _name;
		}
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			if (type.Assembly == Assembly.GetExecutingAssembly() && Attribute.GetCustomAttribute(type, typeof(PreserveAttribute)) == null)
			{
				Log.Warning(string.Format("Type:{0} is missing the UnityEngine.Scripting.Preserve attribute, this will fail on consoles", type));
			}
			return type;
		}
		type = Type.GetType(_name);
		if (type != null)
		{
			if (type.Assembly == Assembly.GetExecutingAssembly() && Attribute.GetCustomAttribute(type, typeof(PreserveAttribute)) == null)
			{
				Log.Warning(string.Format("Type:{0} is missing the UnityEngine.Scripting.Preserve attribute, this will fail on consoles", type));
			}
			return type;
		}
		Log.Warning("Type:" + _name + " was missing when we looked it up via Type.GetType()");
		return null;
	}

	public static void FindTypesWithAttribute<T>(Action<Type> _typeFoundCallback, bool _allowAbstract = false) where T : Attribute
	{
		Type typeFromHandle = typeof(T);
		try
		{
			List<Assembly> loadedAssemblies = ModManager.GetLoadedAssemblies();
			loadedAssemblies.Insert(0, Assembly.GetExecutingAssembly());
			for (int i = 0; i < loadedAssemblies.Count; i++)
			{
				Assembly assembly = loadedAssemblies[i];
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsClass && (_allowAbstract || !type.IsAbstract) && ReflectionHelpers.MemberHasAttribute(type, typeFromHandle))
						{
							try
							{
								if (i == 0 && Attribute.GetCustomAttribute(type, typeof(PreserveAttribute)) == null)
								{
									Log.Error(string.Format("Type:{0} is missing the UnityEngine.Scripting.Preserve attribute, this will fail on consoles", type));
								}
								if (_typeFoundCallback != null)
								{
									_typeFoundCallback(type);
								}
							}
							catch (Exception e)
							{
								Log.Error("Error invoking found type callback for '" + type.FullName + "'");
								Log.Exception(e);
							}
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Log.Error(string.Concat(new string[]
					{
						"Error loading types from assembly '",
						assembly.FullName,
						"' (",
						assembly.Location,
						")"
					}));
					Log.Exception(ex);
					Console.WriteLine();
					Console.WriteLine("Successfully loaded Types:");
					int num = 1;
					foreach (Type type2 in ex.Types)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (type2 != null) ? type2.FullName : "NULL"));
					}
					Console.WriteLine();
					Console.WriteLine("Exceptions:");
					num = 1;
					foreach (Exception ex2 in ex.LoaderExceptions)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (ex2 != null) ? ex2.Message : "NULL"));
					}
					Console.WriteLine();
				}
				catch (Exception e2)
				{
					Log.Error("Error loading types from assembly " + assembly.Location);
					Log.Exception(e2);
				}
			}
		}
		catch (Exception e3)
		{
			Log.Error("Error loading types");
			Log.Exception(e3);
		}
	}

	public static void GetMethodsWithAttribute<T>(Action<MethodInfo> _methodFoundCallback) where T : Attribute
	{
		Type typeFromHandle = typeof(T);
		try
		{
			List<Assembly> loadedAssemblies = ModManager.GetLoadedAssemblies();
			loadedAssemblies.Insert(0, Assembly.GetExecutingAssembly());
			for (int i = 0; i < loadedAssemblies.Count; i++)
			{
				Assembly assembly = loadedAssemblies[i];
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsClass)
						{
							foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
							{
								if (ReflectionHelpers.MemberHasAttribute(methodInfo, typeFromHandle))
								{
									try
									{
										if (_methodFoundCallback != null)
										{
											_methodFoundCallback(methodInfo);
										}
									}
									catch (Exception e)
									{
										Log.Error(string.Concat(new string[]
										{
											"Error invoking found method callback for '",
											methodInfo.DeclaringType.FullName,
											".",
											methodInfo.Name,
											"'"
										}));
										Log.Exception(e);
									}
								}
							}
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Log.Error(string.Concat(new string[]
					{
						"Error loading types from assembly '",
						assembly.FullName,
						"' (",
						assembly.Location,
						")"
					}));
					Log.Exception(ex);
					Console.WriteLine();
					Console.WriteLine("Successfully loaded Types:");
					int num = 1;
					foreach (Type type2 in ex.Types)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (type2 != null) ? type2.FullName : "NULL"));
					}
					Console.WriteLine();
					Console.WriteLine("Exceptions:");
					num = 1;
					foreach (Exception ex2 in ex.LoaderExceptions)
					{
						Console.WriteLine(string.Format("{0}. {1}", num++, (ex2 != null) ? ex2.Message : "NULL"));
					}
					Console.WriteLine();
				}
				catch (Exception e2)
				{
					Log.Error("Error loading types from assembly " + assembly.Location);
					Log.Exception(e2);
				}
			}
		}
		catch (Exception e3)
		{
			Log.Error("Error loading types");
			Log.Exception(e3);
		}
	}

	public static void GetMethodsWithAttribute<T>(Type _containingType, Action<MethodInfo> _methodFoundCallback) where T : Attribute
	{
		Type typeFromHandle = typeof(T);
		try
		{
			if (_containingType.IsClass)
			{
				foreach (MethodInfo methodInfo in _containingType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (ReflectionHelpers.MemberHasAttribute(methodInfo, typeFromHandle))
					{
						try
						{
							if (_methodFoundCallback != null)
							{
								_methodFoundCallback(methodInfo);
							}
						}
						catch (Exception e)
						{
							Log.Error(string.Concat(new string[]
							{
								"Error invoking found method callback for '",
								methodInfo.DeclaringType.FullName,
								".",
								methodInfo.Name,
								"'"
							}));
							Log.Exception(e);
						}
					}
				}
			}
		}
		catch (Exception e2)
		{
			Log.Error("Error loading methods from type " + _containingType.FullName);
			Log.Exception(e2);
		}
	}

	public static bool MemberHasAttribute(MemberInfo _memberInfo, Type _attributeType)
	{
		using (IEnumerator<CustomAttributeData> enumerator = _memberInfo.CustomAttributes.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.AttributeType == _attributeType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool MethodCompatibleWithDelegate<DelegateType>(MethodInfo _method) where DelegateType : Delegate
	{
		MethodInfo method = typeof(DelegateType).GetMethod("Invoke");
		if (method.ReturnType != _method.ReturnType)
		{
			return false;
		}
		return (from _parameterInfo in method.GetParameters()
		select _parameterInfo.ParameterType).SequenceEqual(from _parameterInfo in _method.GetParameters()
		select _parameterInfo.ParameterType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] classnameEndMarkers = new char[]
	{
		'`',
		','
	};
}
