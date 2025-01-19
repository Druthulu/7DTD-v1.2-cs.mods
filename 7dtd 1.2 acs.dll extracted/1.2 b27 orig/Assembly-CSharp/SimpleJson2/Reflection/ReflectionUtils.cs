using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleJson2.Reflection
{
	[GeneratedCode("reflection-utils", "1.0.0")]
	[PublicizedFrom(EAccessModifier.Internal)]
	public class ReflectionUtils
	{
		public static Type GetTypeInfo(Type type)
		{
			return type;
		}

		public static Attribute GetAttribute(MemberInfo info, Type type)
		{
			if (info == null || type == null || !Attribute.IsDefined(info, type))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(info, type);
		}

		public static Type GetGenericListElementType(Type type)
		{
			foreach (Type type2 in ((IEnumerable<Type>)type.GetInterfaces()))
			{
				if (ReflectionUtils.IsTypeGeneric(type2) && type2.GetGenericTypeDefinition() == typeof(IList<>))
				{
					return ReflectionUtils.GetGenericTypeArguments(type2)[0];
				}
			}
			return ReflectionUtils.GetGenericTypeArguments(type)[0];
		}

		public static Attribute GetAttribute(Type objectType, Type attributeType)
		{
			if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(objectType, attributeType);
		}

		public static Type[] GetGenericTypeArguments(Type type)
		{
			return type.GetGenericArguments();
		}

		public static bool IsTypeGeneric(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsGenericType;
		}

		public static bool IsTypeGenericeCollectionInterface(Type type)
		{
			if (!ReflectionUtils.IsTypeGeneric(type))
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(IEnumerable<>);
		}

		public static bool IsAssignableFrom(Type type1, Type type2)
		{
			return ReflectionUtils.GetTypeInfo(type1).IsAssignableFrom(ReflectionUtils.GetTypeInfo(type2));
		}

		public static bool IsTypeDictionary(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type) || (ReflectionUtils.GetTypeInfo(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<, >));
		}

		public static bool IsNullableType(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static object ToNullableType(object obj, Type nullableType)
		{
			if (obj != null)
			{
				return Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture);
			}
			return null;
		}

		public static bool IsValueType(Type type)
		{
			return ReflectionUtils.GetTypeInfo(type).IsValueType;
		}

		public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
		{
			return type.GetConstructors();
		}

		public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
		{
			foreach (ConstructorInfo constructorInfo in ReflectionUtils.GetConstructors(type))
			{
				ParameterInfo[] parameters = constructorInfo.GetParameters();
				if (argsType.Length == parameters.Length)
				{
					int num = 0;
					bool flag = true;
					ParameterInfo[] parameters2 = constructorInfo.GetParameters();
					for (int i = 0; i < parameters2.Length; i++)
					{
						if (parameters2[i].ParameterType != argsType[num])
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return constructorInfo;
					}
				}
			}
			return null;
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static IEnumerable<FieldInfo> GetFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetGetMethod(true);
		}

		public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetSetMethod(true);
		}

		public static ReflectionUtils.ConstructorDelegate GetContructor(ConstructorInfo constructorInfo)
		{
			return ReflectionUtils.GetConstructorByExpression(constructorInfo);
		}

		public static ReflectionUtils.ConstructorDelegate GetContructor(Type type, params Type[] argsType)
		{
			return ReflectionUtils.GetConstructorByExpression(type, argsType);
		}

		public static ReflectionUtils.ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo)
		{
			return (object[] args) => constructorInfo.Invoke(args);
		}

		public static ReflectionUtils.ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
		{
			ConstructorInfo constructorInfo = ReflectionUtils.GetConstructorInfo(type, argsType);
			if (!(constructorInfo == null))
			{
				return ReflectionUtils.GetConstructorByReflection(constructorInfo);
			}
			return null;
		}

		public static ReflectionUtils.ConstructorDelegate GetConstructorByExpression(ConstructorInfo constructorInfo)
		{
			ParameterInfo[] parameters = constructorInfo.GetParameters();
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), "args");
			Expression[] array = new Expression[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				Expression index = Expression.Constant(i);
				Type parameterType = parameters[i].ParameterType;
				Expression expression = Expression.Convert(Expression.ArrayIndex(parameterExpression, index), parameterType);
				array[i] = expression;
			}
			Expression<Func<object[], object>> expression2 = Expression.Lambda<Func<object[], object>>(Expression.New(constructorInfo, array), new ParameterExpression[]
			{
				parameterExpression
			});
			Func<object[], object> compiledLambda = expression2.Compile();
			return (object[] args) => compiledLambda(args);
		}

		public static ReflectionUtils.ConstructorDelegate GetConstructorByExpression(Type type, params Type[] argsType)
		{
			ConstructorInfo constructorInfo = ReflectionUtils.GetConstructorInfo(type, argsType);
			if (!(constructorInfo == null))
			{
				return ReflectionUtils.GetConstructorByExpression(constructorInfo);
			}
			return null;
		}

		public static ReflectionUtils.GetDelegate GetGetMethod(PropertyInfo propertyInfo)
		{
			return ReflectionUtils.GetGetMethodByExpression(propertyInfo);
		}

		public static ReflectionUtils.GetDelegate GetGetMethod(FieldInfo fieldInfo)
		{
			return ReflectionUtils.GetGetMethodByExpression(fieldInfo);
		}

		public static ReflectionUtils.GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = ReflectionUtils.GetGetterMethodInfo(propertyInfo);
			return (object source) => methodInfo.Invoke(source, ReflectionUtils.EmptyObjects);
		}

		public static ReflectionUtils.GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
		{
			return (object source) => fieldInfo.GetValue(source);
		}

		public static ReflectionUtils.GetDelegate GetGetMethodByExpression(PropertyInfo propertyInfo)
		{
			MethodInfo getterMethodInfo = ReflectionUtils.GetGetterMethodInfo(propertyInfo);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
			UnaryExpression instance = (!ReflectionUtils.IsValueType(propertyInfo.DeclaringType)) ? Expression.TypeAs(parameterExpression, propertyInfo.DeclaringType) : Expression.Convert(parameterExpression, propertyInfo.DeclaringType);
			Func<object, object> compiled = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instance, getterMethodInfo), typeof(object)), new ParameterExpression[]
			{
				parameterExpression
			}).Compile();
			return (object source) => compiled(source);
		}

		public static ReflectionUtils.GetDelegate GetGetMethodByExpression(FieldInfo fieldInfo)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
			MemberExpression expression = Expression.Field(Expression.Convert(parameterExpression, fieldInfo.DeclaringType), fieldInfo);
			ReflectionUtils.GetDelegate compiled = Expression.Lambda<ReflectionUtils.GetDelegate>(Expression.Convert(expression, typeof(object)), new ParameterExpression[]
			{
				parameterExpression
			}).Compile();
			return (object source) => compiled(source);
		}

		public static ReflectionUtils.SetDelegate GetSetMethod(PropertyInfo propertyInfo)
		{
			return ReflectionUtils.GetSetMethodByExpression(propertyInfo);
		}

		public static ReflectionUtils.SetDelegate GetSetMethod(FieldInfo fieldInfo)
		{
			return ReflectionUtils.GetSetMethodByExpression(fieldInfo);
		}

		public static ReflectionUtils.SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = ReflectionUtils.GetSetterMethodInfo(propertyInfo);
			return delegate(object source, object value)
			{
				methodInfo.Invoke(source, new object[]
				{
					value
				});
			};
		}

		public static ReflectionUtils.SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo)
		{
			return delegate(object source, object value)
			{
				fieldInfo.SetValue(source, value);
			};
		}

		public static ReflectionUtils.SetDelegate GetSetMethodByExpression(PropertyInfo propertyInfo)
		{
			MethodInfo setterMethodInfo = ReflectionUtils.GetSetterMethodInfo(propertyInfo);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
			UnaryExpression instance = (!ReflectionUtils.IsValueType(propertyInfo.DeclaringType)) ? Expression.TypeAs(parameterExpression, propertyInfo.DeclaringType) : Expression.Convert(parameterExpression, propertyInfo.DeclaringType);
			UnaryExpression unaryExpression = (!ReflectionUtils.IsValueType(propertyInfo.PropertyType)) ? Expression.TypeAs(parameterExpression2, propertyInfo.PropertyType) : Expression.Convert(parameterExpression2, propertyInfo.PropertyType);
			Action<object, object> compiled = Expression.Lambda<Action<object, object>>(Expression.Call(instance, setterMethodInfo, new Expression[]
			{
				unaryExpression
			}), new ParameterExpression[]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
			return delegate(object source, object val)
			{
				compiled(source, val);
			};
		}

		public static ReflectionUtils.SetDelegate GetSetMethodByExpression(FieldInfo fieldInfo)
		{
			ParameterExpression parameterExpression;
			ParameterExpression parameterExpression2;
			Action<object, object> compiled = Expression.Lambda<Action<object, object>>(ReflectionUtils.Assign(Expression.Field(Expression.Convert(parameterExpression, fieldInfo.DeclaringType), fieldInfo), Expression.Convert(parameterExpression2, fieldInfo.FieldType)), new ParameterExpression[]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
			return delegate(object source, object val)
			{
				compiled(source, val);
			};
		}

		public static BinaryExpression Assign(Expression left, Expression right)
		{
			MethodInfo method = typeof(ReflectionUtils.Assigner<>).MakeGenericType(new Type[]
			{
				left.Type
			}).GetMethod("Assign");
			return Expression.Add(left, right, method);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly object[] EmptyObjects = new object[0];

		public delegate object GetDelegate(object source);

		public delegate void SetDelegate(object source, object value);

		public delegate object ConstructorDelegate(params object[] args);

		public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);

		[PublicizedFrom(EAccessModifier.Private)]
		public static class Assigner<T>
		{
			public static T Assign(ref T left, T right)
			{
				left = right;
				return right;
			}
		}

		public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			public ThreadSafeDictionary(ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
			{
				this._valueFactory = valueFactory;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public TValue Get(TKey key)
			{
				if (this._dictionary == null)
				{
					return this.AddValue(key);
				}
				TValue result;
				if (!this._dictionary.TryGetValue(key, out result))
				{
					return this.AddValue(key);
				}
				return result;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public TValue AddValue(TKey key)
			{
				TValue tvalue = this._valueFactory(key);
				object @lock = this._lock;
				lock (@lock)
				{
					if (this._dictionary == null)
					{
						this._dictionary = new Dictionary<TKey, TValue>();
						this._dictionary[key] = tvalue;
					}
					else
					{
						TValue result;
						if (this._dictionary.TryGetValue(key, out result))
						{
							return result;
						}
						Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this._dictionary);
						dictionary[key] = tvalue;
						this._dictionary = dictionary;
					}
				}
				return tvalue;
			}

			public void Add(TKey key, TValue value)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(TKey key)
			{
				return this._dictionary.ContainsKey(key);
			}

			public ICollection<TKey> Keys
			{
				get
				{
					return this._dictionary.Keys;
				}
			}

			public bool Remove(TKey key)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(TKey key, out TValue value)
			{
				value = this[key];
				return true;
			}

			public ICollection<TValue> Values
			{
				get
				{
					return this._dictionary.Values;
				}
			}

			public TValue this[TKey key]
			{
				get
				{
					return this.Get(key);
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public void Add(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get
				{
					return this._dictionary.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				return this._dictionary.GetEnumerator();
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public IEnumerator GetEnumerator()
			{
				return this._dictionary.GetEnumerator();
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly object _lock = new object();

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;

			[PublicizedFrom(EAccessModifier.Private)]
			public Dictionary<TKey, TValue> _dictionary;
		}
	}
}
