using System;
using System.Collections.Generic;
using NCalc;

public class BindingItemNcalc : BindingItem
{
	public BindingItemNcalc(BindingInfo _parent, XUiView _view, string _sourceText) : base(_sourceText)
	{
		if (this.FieldName[0] == '#')
		{
			this.FieldName = this.FieldName.Substring(1);
		}
		else if (this.FieldName.EndsWith("|once", StringComparison.OrdinalIgnoreCase))
		{
			this.FieldName = this.FieldName.Substring(0, this.FieldName.Length - "|once".Length);
		}
		this.parent = _parent;
		this.view = _view;
		this.expression = new Expression(this.FieldName, EvaluateOptions.IgnoreCase | EvaluateOptions.NoCache | EvaluateOptions.UseDoubleForAbsFunction | EvaluateOptions.ReuseInstances)
		{
			Parameters = this.expressionParamDict
		};
		this.expression.EvaluateFunction += this.NCalcEvaluateFunction;
		this.expression.EvaluateParameter += this.NCalcEvaluateParameter;
		this.CurrentValue = this.EvaluateExpression();
		if (this.usesIndeterministicFunctions || this.bindings.Count == 0)
		{
			for (XUiController controller = _view.Controller; controller != null; controller = controller.Parent)
			{
				if (controller.GetType() != typeof(XUiController))
				{
					this.DataContext = controller;
					this.DataContext.AddBinding(_parent);
					return;
				}
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (this.BindingType == BindingItem.BindingTypes.Complete && !_forceAll)
		{
			return this.CurrentValue;
		}
		bool flag = this.usesIndeterministicFunctions;
		foreach (KeyValuePair<string, BindingItemNcalc.BindingState> keyValuePair in this.bindings)
		{
			if (keyValuePair.Value.RefreshValue())
			{
				this.expressionParamDict[keyValuePair.Key] = keyValuePair.Value.CurrentValue;
				flag = true;
			}
		}
		if (!flag)
		{
			return this.CurrentValue;
		}
		this.CurrentValue = this.EvaluateExpression();
		if (this.BindingType == BindingItem.BindingTypes.Once)
		{
			this.BindingType = BindingItem.BindingTypes.Complete;
		}
		return this.CurrentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string EvaluateExpression()
	{
		string text = null;
		try
		{
			object obj = this.expression.Evaluate();
			if (obj == null)
			{
				text = "";
			}
			else if (obj is decimal)
			{
				text = ((decimal)obj).ToCultureInvariantString("0.########");
			}
			else if (obj is float)
			{
				text = ((float)obj).ToCultureInvariantString();
			}
			else if (obj is double)
			{
				text = ((double)obj).ToCultureInvariantString();
			}
			else
			{
				text = obj.ToString();
			}
		}
		catch (ArgumentException e)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + this.SourceText + " --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			Log.Exception(e);
		}
		catch (Exception e2)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + this.SourceText + " --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			Log.Exception(e2);
			text = "";
		}
		if (text != null && text.Contains("{cvar("))
		{
			text = base.ParseCVars(text);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		BindingItemNcalc.BindingState bindingState;
		if (this.bindings.TryGetValue(_name, out bindingState))
		{
			_args.Result = bindingState.CurrentValue;
			return;
		}
		for (XUiController controller = this.view.Controller; controller != null; controller = controller.Parent)
		{
			if (controller.GetType() != typeof(XUiController))
			{
				string text = "";
				if (controller.GetBindingValue(ref text, _name))
				{
					controller.AddBinding(this.parent);
					bindingState = new BindingItemNcalc.BindingState(_name, controller, text);
					this.bindings[_name] = bindingState;
					this.expressionParamDict[_name] = text;
					_args.Result = text;
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("localization"))
		{
			this.localization(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("cvar"))
		{
			this.usesIndeterministicFunctions = true;
			this.cvar(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("format"))
		{
			this.format(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("length"))
		{
			this.length(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("int"))
		{
			this.toInt(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("float"))
		{
			this.toFloat(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("bound"))
		{
			this.isBound(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("always"))
		{
			this.usesIndeterministicFunctions = true;
			this.always(_args);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localization(FunctionArgs _args)
	{
		_args.Result = "<ERROR>";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"localization",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'localization': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			return;
		}
		_args.Result = Localization.Get(obj.ToString(), false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cvar(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"cvar",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			_args.Result = 1f;
			return;
		}
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			_args.Result = 1f;
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			_args.Result = 1f;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'cvar': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			_args.Result = 1f;
			return;
		}
		_args.Result = primaryPlayer.GetCVar(obj.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void format(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length < 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected at least {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"format",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'format': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			return;
		}
		object[] array = new object[_args.Parameters.Length - 1];
		for (int i = 1; i < _args.Parameters.Length; i++)
		{
			array[i - 1] = _args.Parameters[i].Evaluate();
		}
		string result = string.Format(obj.ToString(), array);
		_args.Result = result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void length(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"length",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'length': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			return;
		}
		_args.Result = obj.ToString().Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toInt(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"toInt",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			_args.Result = 0;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'toInt': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			_args.Result = 0;
			return;
		}
		try
		{
			_args.Result = Convert.ToInt32(obj);
		}
		catch (Exception e)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}': Argument does not evaluate to a number. Binding expression: '{1}', argument '{2}' --- hierarchy: {3}", new object[]
			{
				"toInt",
				this.SourceText,
				obj,
				this.view.Controller.GetXuiHierarchy()
			}));
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toFloat(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"toFloat",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			_args.Result = 0.0;
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			Log.Error("[XUi] Binding expression calling function 'toFloat': Can not evaluate argument. Binding expression: '" + this.SourceText + "' --- hierarchy: " + this.view.Controller.GetXuiHierarchy());
			_args.Result = 0.0;
			return;
		}
		try
		{
			_args.Result = Convert.ToDouble(obj);
		}
		catch (Exception e)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}': Argument does not evaluate to a number. Binding expression: '{1}', argument '{2}' --- hierarchy: {3}", new object[]
			{
				"toFloat",
				this.SourceText,
				obj,
				this.view.Controller.GetXuiHierarchy()
			}));
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void isBound(FunctionArgs _args)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void always(FunctionArgs _args)
	{
		_args.Result = "";
		if (_args.Parameters.Length != 1)
		{
			Log.Error(string.Format("[XUi] Binding expression calling function '{0}' with invalid number of arguments ({1}, expected {2}). Binding expression: '{3}' --- hierarchy: {4}", new object[]
			{
				"always",
				_args.Parameters.Length,
				1,
				this.SourceText,
				this.view.Controller.GetXuiHierarchy()
			}));
			return;
		}
		_args.Result = _args.Parameters[0].Evaluate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BindingInfo parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView view;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, BindingItemNcalc.BindingState> bindings = new Dictionary<string, BindingItemNcalc.BindingState>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public class BindingState
	{
		public BindingState(string _bindingName, XUiController _controller, string _initialValue)
		{
			this.bindingName = _bindingName;
			this.controller = _controller;
			this.CurrentValue = _initialValue;
		}

		public bool RefreshValue()
		{
			string text = "";
			if (!this.controller.GetBindingValue(ref text, this.bindingName))
			{
				return false;
			}
			if (string.Equals(this.CurrentValue, text, StringComparison.Ordinal))
			{
				return false;
			}
			this.CurrentValue = text;
			return true;
		}

		public override string ToString()
		{
			return "Binding:" + this.bindingName + "=" + this.CurrentValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string bindingName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiController controller;

		public string CurrentValue;
	}
}
