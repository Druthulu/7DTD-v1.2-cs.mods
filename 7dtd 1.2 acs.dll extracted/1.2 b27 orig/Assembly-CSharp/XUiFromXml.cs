using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NCalc;
using NCalc.Domain;
using UnityEngine;

public static class XUiFromXml
{
	public static XUiFromXml.DebugLevel DebugXuiLoading
	{
		get
		{
			return XUiFromXml.debugXuiLoading;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiFromXml()
	{
		string launchArgument = GameUtils.GetLaunchArgument("debugxui");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				XUiFromXml.debugXuiLoading = XUiFromXml.DebugLevel.Verbose;
				return;
			}
			XUiFromXml.debugXuiLoading = XUiFromXml.DebugLevel.Warning;
		}
	}

	public static void ClearLoadingData()
	{
		XUiFromXml.mainXuiXml = null;
		Dictionary<string, XElement> dictionary = XUiFromXml.windowData;
		if (dictionary != null)
		{
			dictionary.Clear();
		}
		XUiFromXml.windowData = null;
		Dictionary<string, XElement> dictionary2 = XUiFromXml.controlData;
		if (dictionary2 != null)
		{
			dictionary2.Clear();
		}
		XUiFromXml.controlData = null;
		IDictionary<string, int> dictionary3 = XUiFromXml.usedWindows;
		if (dictionary3 != null)
		{
			dictionary3.Clear();
		}
		XUiFromXml.usedWindows = null;
		Dictionary<string, Dictionary<string, object>> dictionary4 = XUiFromXml.controlDefaults;
		if (dictionary4 != null)
		{
			dictionary4.Clear();
		}
		XUiFromXml.controlDefaults = null;
		IDictionary<string, int> dictionary5 = XUiFromXml.usedControls;
		if (dictionary5 != null)
		{
			dictionary5.Clear();
		}
		XUiFromXml.usedControls = null;
		if (XUiFromXml.expressionCache != null)
		{
			Dictionary<string, Expression> dictionary6 = XUiFromXml.expressionCache;
			foreach (Expression expression in ((dictionary6 != null) ? dictionary6.Values : null))
			{
				expression.EvaluateFunction -= XUiFromXml.NCalcIdentifierDefinedFunction;
			}
		}
		Dictionary<string, Expression> dictionary7 = XUiFromXml.expressionCache;
		if (dictionary7 != null)
		{
			dictionary7.Clear();
		}
		XUiFromXml.expressionCache = null;
	}

	public static void ClearData()
	{
		XUiFromXml.ClearLoadingData();
		Dictionary<string, XUiFromXml.StyleData> dictionary = XUiFromXml.styles;
		if (dictionary != null)
		{
			dictionary.Clear();
		}
		XUiFromXml.styles = null;
	}

	public static bool HasData()
	{
		return XUiFromXml.mainXuiXml != null && XUiFromXml.windowData.Count > 0 && XUiFromXml.controlData.Count > 0 && XUiFromXml.styles.Count > 0;
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (XUi.Stopwatch == null)
		{
			XUi.Stopwatch = new MicroStopwatch();
		}
		if (!XUi.Stopwatch.IsRunning)
		{
			XUi.Stopwatch.Reset();
			XUi.Stopwatch.Start();
		}
		if (XUiFromXml.windowData == null)
		{
			XUiFromXml.windowData = new Dictionary<string, XElement>(StringComparer.Ordinal);
		}
		if (XUiFromXml.usedWindows == null)
		{
			XUiFromXml.usedWindows = new SortedDictionary<string, int>(StringComparer.Ordinal);
		}
		if (XUiFromXml.controlData == null)
		{
			XUiFromXml.controlData = new Dictionary<string, XElement>(StringComparer.Ordinal);
		}
		if (XUiFromXml.controlDefaults == null)
		{
			XUiFromXml.controlDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
		}
		if (XUiFromXml.usedControls == null)
		{
			XUiFromXml.usedControls = new SortedDictionary<string, int>(StringComparer.Ordinal);
		}
		if (XUiFromXml.styles == null)
		{
			XUiFromXml.styles = new CaseInsensitiveStringDictionary<XUiFromXml.StyleData>();
		}
		if (XUiFromXml.expressionCache == null)
		{
			XUiFromXml.expressionCache = new Dictionary<string, Expression>();
		}
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No elements found!");
		}
		string localName = root.Name.LocalName;
		if (!(localName == "xui"))
		{
			if (!(localName == "windows"))
			{
				if (!(localName == "styles"))
				{
					if (localName == "controls")
					{
						XUiFromXml.loadControls(_xmlFile);
					}
				}
				else
				{
					XUiFromXml.loadStyles(_xmlFile);
				}
			}
			else
			{
				XUiFromXml.loadWindows(_xmlFile);
			}
		}
		else
		{
			XUiFromXml.mainXuiXml = _xmlFile;
		}
		yield break;
	}

	public static void LoadDone(bool _logUnused)
	{
		if (!_logUnused)
		{
			return;
		}
		foreach (KeyValuePair<string, int> keyValuePair in XUiFromXml.usedControls)
		{
			if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off && (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Warning || keyValuePair.Value <= 0))
			{
				if (keyValuePair.Value > 0)
				{
					Log.Out(string.Format("[XUi] Control '{0}' used {1} times!", keyValuePair.Key, keyValuePair.Value));
				}
				else
				{
					Log.Warning("[XUi] Control '" + keyValuePair.Key + "' not used!");
				}
			}
		}
		foreach (KeyValuePair<string, int> keyValuePair2 in XUiFromXml.usedWindows)
		{
			if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off && (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Warning || keyValuePair2.Value <= 0))
			{
				if (keyValuePair2.Value > 0)
				{
					Log.Out(string.Format("[XUi] Window '{0}' used {1} times!", keyValuePair2.Key, keyValuePair2.Value));
				}
				else
				{
					Log.Warning("[XUi] Window '" + keyValuePair2.Key + "' not used!");
				}
			}
		}
	}

	public static void GetWindowGroupNames(out List<string> windowGroupNames)
	{
		windowGroupNames = new List<string>();
		foreach (XElement xelement in XUiFromXml.mainXuiXml.XmlDoc.Root.Elements("ruleset"))
		{
			foreach (XElement element in xelement.Elements("window_group"))
			{
				if (element.HasAttribute("name"))
				{
					string attribute = element.GetAttribute("name");
					if (!windowGroupNames.Contains(attribute))
					{
						windowGroupNames.Add(attribute);
					}
				}
			}
		}
	}

	public static void LoadXui(XUi _xui, string windowGroupToLoad)
	{
		XElement root = XUiFromXml.mainXuiXml.XmlDoc.Root;
		if (root.HasAttribute("ruleset"))
		{
			_xui.Ruleset = root.GetAttribute("ruleset");
		}
		foreach (XElement xelement in root.Elements("ruleset"))
		{
			if (!xelement.HasAttribute("name") || xelement.GetAttribute("name").EqualsCaseInsensitive(_xui.Ruleset))
			{
				if (xelement.HasAttribute("scale"))
				{
					_xui.SetScale(StringParsers.ParseFloat(xelement.GetAttribute("scale"), 0, -1, NumberStyles.Any));
				}
				if (xelement.HasAttribute("stackpanel_scale"))
				{
					_xui.SetStackPanelScale(StringParsers.ParseFloat(xelement.GetAttribute("stackpanel_scale"), 0, -1, NumberStyles.Any));
				}
				if (xelement.HasAttribute("ignore_missing_class"))
				{
					_xui.IgnoreMissingClass = StringParsers.ParseBool(xelement.GetAttribute("ignore_missing_class"), 0, -1, true);
				}
				foreach (XElement xelement2 in xelement.Elements("window_group"))
				{
					string text = "";
					if (xelement2.HasAttribute("name"))
					{
						text = xelement2.GetAttribute("name");
					}
					if (_xui.FindWindowGroupByName(text) == null && windowGroupToLoad.Equals(text))
					{
						XUiWindowGroup.EHasActionSetFor hasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
						if (xelement2.HasAttribute("actionSet"))
						{
							string a = xelement2.GetAttribute("actionSet").ToLower().Trim();
							if (!(a == "true"))
							{
								if (!(a == "false"))
								{
									if (!(a == "controller"))
									{
										if (a == "keyboard")
										{
											hasActionSetFor = XUiWindowGroup.EHasActionSetFor.OnlyKeyboard;
										}
									}
									else
									{
										hasActionSetFor = XUiWindowGroup.EHasActionSetFor.OnlyController;
									}
								}
								else
								{
									hasActionSetFor = XUiWindowGroup.EHasActionSetFor.None;
								}
							}
							else
							{
								hasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
							}
						}
						string defaultSelectedName = "";
						if (xelement2.HasAttribute("defaultSelected"))
						{
							defaultSelectedName = xelement2.GetAttribute("defaultSelected");
						}
						XUiWindowGroup xuiWindowGroup = new XUiWindowGroup(text, hasActionSetFor, defaultSelectedName)
						{
							xui = _xui
						};
						int stackPanelYOffset;
						if (xelement2.HasAttribute("stack_panel_y_offset") && int.TryParse(xelement2.GetAttribute("stack_panel_y_offset"), out stackPanelYOffset))
						{
							xuiWindowGroup.StackPanelYOffset = stackPanelYOffset;
						}
						int stackPanelPadding = 16;
						if (xelement2.HasAttribute("stack_panel_padding") && int.TryParse(xelement2.GetAttribute("stack_panel_padding"), out stackPanelYOffset))
						{
							xuiWindowGroup.StackPanelPadding = stackPanelPadding;
						}
						if (xelement2.HasAttribute("open_backpack_on_open"))
						{
							StringParsers.TryParseBool(xelement2.GetAttribute("open_backpack_on_open"), out xuiWindowGroup.openBackpackOnOpen, 0, -1, true);
						}
						if (xelement2.HasAttribute("close_compass_on_open"))
						{
							StringParsers.TryParseBool(xelement2.GetAttribute("close_compass_on_open"), out xuiWindowGroup.closeCompassOnOpen, 0, -1, true);
						}
						if (xelement2.HasAttribute("controller"))
						{
							string attribute = xelement2.GetAttribute("controller");
							Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", attribute);
							if (typeWithPrefix != null)
							{
								xuiWindowGroup.Controller = (XUiController)Activator.CreateInstance(typeWithPrefix);
								xuiWindowGroup.Controller.WindowGroup = xuiWindowGroup;
							}
							else
							{
								XUiFromXml.logForNode(_xui.IgnoreMissingClass ? LogType.Warning : LogType.Error, xelement2, "[XUi] Controller '" + attribute + "' not found, using base XUiController");
								xuiWindowGroup.Controller = new XUiController
								{
									WindowGroup = xuiWindowGroup
								};
							}
						}
						else
						{
							xuiWindowGroup.Controller = new XUiController
							{
								WindowGroup = xuiWindowGroup
							};
						}
						xuiWindowGroup.Controller.xui = _xui;
						XUiC_DragAndDropWindow xuiC_DragAndDropWindow = xuiWindowGroup.Controller as XUiC_DragAndDropWindow;
						if (xuiC_DragAndDropWindow != null)
						{
							_xui.dragAndDrop = xuiC_DragAndDropWindow;
						}
						XUiC_OnScreenIcons xuiC_OnScreenIcons = xuiWindowGroup.Controller as XUiC_OnScreenIcons;
						if (xuiC_OnScreenIcons != null)
						{
							_xui.onScreenIcons = xuiC_OnScreenIcons;
						}
						foreach (XElement xelement3 in xelement2.Elements("window"))
						{
							string text2 = "";
							if (xelement3.HasAttribute("name"))
							{
								text2 = xelement3.GetAttribute("name");
							}
							bool flag = false;
							if (xelement3.HasElements)
							{
								flag = true;
								XUiV_Window xuiV_Window = (XUiV_Window)XUiFromXml.parseViewComponents(xelement3, xuiWindowGroup, xuiWindowGroup.Controller, "", null);
								if (xuiV_Window == null)
								{
									continue;
								}
								if (xelement3.HasAttribute("anchor"))
								{
									xuiV_Window.Anchor = xelement3.GetAttribute("anchor");
								}
								if (xelement3.HasAttribute("pos"))
								{
									xuiV_Window.Position = StringParsers.ParseVector2i(xelement3.GetAttribute("pos"), ',');
								}
								else if (xelement3.HasAttribute("position"))
								{
									xuiV_Window.Position = StringParsers.ParseVector2i(xelement3.GetAttribute("position"), ',');
								}
							}
							if (!flag)
							{
								if (XUiFromXml.windowData.ContainsKey(text2))
								{
									IDictionary<string, int> dictionary = XUiFromXml.usedWindows;
									string key = text2;
									int num = dictionary[key];
									dictionary[key] = num + 1;
									XUiV_Window xuiV_Window2 = (XUiV_Window)XUiFromXml.parseViewComponents(XUiFromXml.windowData[text2], xuiWindowGroup, xuiWindowGroup.Controller, "", null);
									if (xuiV_Window2 != null)
									{
										if (xelement3.HasAttribute("anchor"))
										{
											xuiV_Window2.Anchor = xelement3.GetAttribute("anchor");
										}
										if (xelement3.HasAttribute("pos"))
										{
											xuiV_Window2.Position = StringParsers.ParseVector2i(xelement3.GetAttribute("pos"), ',');
										}
										else if (xelement3.HasAttribute("position"))
										{
											xuiV_Window2.Position = StringParsers.ParseVector2i(xelement3.GetAttribute("position"), ',');
										}
										_xui.AddWindow(xuiV_Window2);
									}
								}
								else if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
								{
									Log.Warning("[XUi] window name '" + text2 + "' not found!");
								}
							}
						}
						_xui.WindowGroups.Add(xuiWindowGroup);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadWindows(XmlFile _xmlFile)
	{
		foreach (XElement xelement in _xmlFile.XmlDoc.Root.Elements("window"))
		{
			string text = "";
			if (xelement.HasAttribute("name"))
			{
				text = xelement.GetAttribute("name");
			}
			if (!xelement.HasAttribute("platform") || XUi.IsMatchingPlatform(xelement.GetAttribute("platform")))
			{
				if (!XUiFromXml.windowData.ContainsKey(text))
				{
					XUiFromXml.windowData[text] = xelement;
					XUiFromXml.usedWindows[text] = 0;
				}
				else if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning("[XUi] window data already contains '" + text + "'");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadControls(XmlFile _xmlFile)
	{
		foreach (XElement xelement in _xmlFile.XmlDoc.Root.Elements())
		{
			string localName = xelement.Name.LocalName;
			Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
			foreach (XAttribute xattribute in xelement.Attributes())
			{
				dictionary[xattribute.Name.LocalName] = xattribute.Value;
			}
			int num = xelement.Elements().Count<XElement>();
			XElement value = xelement.Elements().First<XElement>();
			if (num > 1)
			{
				if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Out("[XUi] Control '{0}' cannot have more than a single child node!", new object[]
					{
						localName
					});
				}
			}
			else if (num < 1)
			{
				if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Out("[XUi] Control '{0}' must have a single child node!", new object[]
					{
						localName
					});
					continue;
				}
				continue;
			}
			if (XUiFromXml.controlData.ContainsKey(localName) && XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				Log.Warning("[XUi] Control '" + localName + "' already defined, overwriting!");
			}
			XUiFromXml.controlData[localName] = value;
			XUiFromXml.controlDefaults[localName] = dictionary;
			XUiFromXml.usedControls[localName] = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadStyles(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <styles> found!");
		}
		foreach (XElement xelement in root.Elements())
		{
			XUiFromXml.StyleData styleData;
			if (xelement.Name == "global")
			{
				if (!XUiFromXml.styles.TryGetValue("global", out styleData))
				{
					styleData = new XUiFromXml.StyleData("global", string.Empty);
					XUiFromXml.styles.Add(styleData.KeyName, styleData);
				}
			}
			else
			{
				string text = null;
				string text2 = null;
				if (xelement.HasAttribute("name"))
				{
					text = xelement.GetAttribute("name");
				}
				if (xelement.HasAttribute("type"))
				{
					text2 = xelement.GetAttribute("type");
				}
				if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2))
				{
					Log.Warning("[XUi] Style entry with neither 'Type' or 'Name' attribute");
					continue;
				}
				XUiFromXml.StyleData styleData2 = new XUiFromXml.StyleData(text, text2);
				if (XUiFromXml.styles.TryGetValue(styleData2.KeyName, out styleData))
				{
					if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
					{
						Log.Warning("[XUi] Style '" + styleData2.KeyName + "' already defined, merging contents");
					}
				}
				else
				{
					XUiFromXml.styles.Add(styleData2.KeyName, styleData2);
					styleData = styleData2;
				}
			}
			foreach (XElement element in xelement.Elements())
			{
				if (!element.HasAttribute("name"))
				{
					Log.Error("[XUi] Style '" + styleData.KeyName + "' contains a entry that has no 'name' attribute!");
				}
				else if (!element.HasAttribute("value"))
				{
					Log.Error("[XUi] Style '" + styleData.KeyName + "' contains a entry that has no 'value' attribute!");
				}
				else
				{
					string attribute = element.GetAttribute("value");
					string attribute2 = element.GetAttribute("name");
					XUiFromXml.StyleEntryData value = new XUiFromXml.StyleEntryData(attribute2, attribute);
					styleData.StyleEntries[attribute2] = value;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView parseViewComponents(XElement _node, XUiWindowGroup _windowGroup, XUiController _parent = null, string nodeNameOverride = "", Dictionary<string, object> _controlParams = null)
	{
		if (_node.HasAttribute("platform") && !XUi.IsMatchingPlatform(_node.GetAttribute("platform")))
		{
			return null;
		}
		XUi xui = _windowGroup.xui;
		string localName = _node.Name.LocalName;
		string text = localName;
		if (nodeNameOverride == "" && _node.HasAttribute("name"))
		{
			text = _node.GetAttribute("name");
		}
		else if (nodeNameOverride != "")
		{
			text = nodeNameOverride;
		}
		bool flag = true;
		bool flag2 = true;
		if (_controlParams != null)
		{
			XUiFromXml.parseControlParams(_node, _controlParams);
		}
		uint num = <PrivateImplementationDetails>.ComputeStringHash(localName);
		XUiView xuiView;
		if (num <= 2240103498U)
		{
			if (num <= 1179827136U)
			{
				if (num != 1013213428U)
				{
					if (num != 1135768689U)
					{
						if (num == 1179827136U)
						{
							if (localName == "gamepad_icon")
							{
								xuiView = new XUiV_GamepadIcon(text);
								goto IL_310;
							}
						}
					}
					else if (localName == "button")
					{
						xuiView = new XUiV_Button(text);
						goto IL_310;
					}
				}
				else if (localName == "texture")
				{
					xuiView = new XUiV_Texture(text);
					goto IL_310;
				}
			}
			else if (num != 1251777503U)
			{
				if (num != 2179094556U)
				{
					if (num == 2240103498U)
					{
						if (localName == "textlist")
						{
							xuiView = new XUiV_TextList(text);
							goto IL_310;
						}
					}
				}
				else if (localName == "sprite")
				{
					xuiView = new XUiV_Sprite(text);
					goto IL_310;
				}
			}
			else if (localName == "table")
			{
				xuiView = new XUiV_Table(text);
				goto IL_310;
			}
		}
		else if (num <= 2843749381U)
		{
			if (num != 2354395792U)
			{
				if (num != 2708649949U)
				{
					if (num == 2843749381U)
					{
						if (localName == "widget")
						{
							xuiView = new XUiV_Widget(text);
							goto IL_310;
						}
					}
				}
				else if (localName == "window")
				{
					xuiView = new XUiV_Window(text);
					goto IL_310;
				}
			}
			else if (localName == "filledsprite")
			{
				xuiView = new XUiV_FilledSprite(text);
				goto IL_310;
			}
		}
		else if (num <= 3439217733U)
		{
			if (num != 2944866961U)
			{
				if (num == 3439217733U)
				{
					if (localName == "panel")
					{
						xuiView = new XUiV_Panel(text);
						goto IL_310;
					}
				}
			}
			else if (localName == "grid")
			{
				xuiView = new XUiV_Grid(text);
				goto IL_310;
			}
		}
		else if (num != 3940830471U)
		{
			if (num == 4137097213U)
			{
				if (localName == "label")
				{
					xuiView = new XUiV_Label(text);
					goto IL_310;
				}
			}
		}
		else if (localName == "rect")
		{
			xuiView = new XUiV_Rect(text);
			goto IL_310;
		}
		xuiView = XUiFromXml.createFromTemplate(localName, text, _node, _parent, _windowGroup, ref flag, ref flag2);
		IL_310:
		XUiView xuiView2 = xuiView;
		if (flag2)
		{
			xuiView2.xui = xui;
			XUiFromXml.parseController(_node, xuiView2, _parent);
			XUiFromXml.parseAttributes(_node, xuiView2, _parent, _controlParams);
		}
		xuiView2.Controller.WindowGroup = _windowGroup;
		if (xuiView2.RepeatContent)
		{
			if (_node.Elements().Count<XElement>() != 1)
			{
				if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					XUiFromXml.logForNode(LogType.Warning, _node, "[XUi] XUiFromXml::parseByElementName: Invalid repeater child count. Must have one child element.");
				}
			}
			else
			{
				int repeatCount = xuiView2.RepeatCount;
				if (_controlParams == null)
				{
					_controlParams = new CaseInsensitiveStringDictionary<object>();
				}
				_controlParams["repeat_count"] = repeatCount;
				XElement other = _node.Elements().First<XElement>();
				for (int i = 0; i < repeatCount; i++)
				{
					_controlParams["repeat_i"] = i;
					xuiView2.setRepeatContentTemplateParams(_controlParams, i);
					XUiFromXml.parseViewComponents(new XElement(other), _windowGroup, xuiView2.Controller, i.ToString(), _controlParams);
				}
			}
			flag = false;
		}
		if (flag)
		{
			foreach (XElement node in _node.Elements())
			{
				XUiFromXml.parseViewComponents(node, _windowGroup, xuiView2.Controller, "", _controlParams);
			}
		}
		return xuiView2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createFromTemplate(string _templateName, string _viewName, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, ref bool _parseChildren, ref bool _parseControllerAndAttributes)
	{
		XElement other;
		if (!XUiFromXml.controlData.TryGetValue(_templateName, out other))
		{
			if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				XUiFromXml.logForNode(LogType.Warning, _node, "[XUi] View \"" + _templateName + "\" not found!");
			}
			return XUiFromXml.createEmptyView(_viewName, _parent, _windowGroup, ref _parseControllerAndAttributes);
		}
		if (_node.HasElements)
		{
			if (XUiFromXml.debugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				XUiFromXml.logForNode(LogType.Warning, _node, "[XUi] Instantiation of templates may not have any child nodes!");
			}
			_parseChildren = false;
			return XUiFromXml.createEmptyView(_viewName, _parent, _windowGroup, ref _parseControllerAndAttributes);
		}
		Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
		Dictionary<string, object> src;
		if (XUiFromXml.controlDefaults.TryGetValue(_templateName, out src))
		{
			src.CopyTo(dictionary);
		}
		XUiFromXml.parseAttributes(_node, null, null, dictionary);
		XElement xelement = new XElement(other);
		IDictionary<string, int> dictionary2 = XUiFromXml.usedControls;
		int num = dictionary2[_templateName];
		dictionary2[_templateName] = num + 1;
		_node.Add(xelement);
		XUiView xuiView = XUiFromXml.parseViewComponents(xelement, _windowGroup, _parent, _viewName, dictionary);
		if (xuiView == null)
		{
			return null;
		}
		xuiView.xui = _windowGroup.xui;
		xelement.Remove();
		_parseChildren = false;
		_parseControllerAndAttributes = false;
		return xuiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createEmptyView(string _viewName, XUiController _parent, XUiWindowGroup _windowGroup, ref bool _parseControllerAndAttributes)
	{
		XUiView xuiView = new XUiView(_viewName)
		{
			xui = _windowGroup.xui
		};
		xuiView.Controller = new XUiController
		{
			xui = _windowGroup.xui
		};
		if (_parent != null)
		{
			xuiView.Controller.Parent = _parent;
			_parent.AddChild(xuiView.Controller);
		}
		xuiView.SetDefaults(_parent);
		_parseControllerAndAttributes = false;
		return xuiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseControlParams(XElement _node, Dictionary<string, object> _controlParams)
	{
		foreach (XAttribute xattribute in _node.Attributes())
		{
			string text = xattribute.Value;
			bool flag = false;
			int num;
			while ((num = text.IndexOf("${", StringComparison.Ordinal)) >= 0)
			{
				int num2 = text.IndexOf('}', num);
				int count = num2 - num + 1;
				if (num2 < 0)
				{
					LogType level = LogType.Error;
					string str = "[XUi] Expression has unclosed parameter references: ";
					XName name = xattribute.Name;
					XUiFromXml.logForNode(level, _node, str + ((name != null) ? name.ToString() : null) + "=" + text);
					break;
				}
				string text2 = text.Substring(num + 2, num2 - (num + 2));
				Expression expression;
				if (!XUiFromXml.expressionCache.TryGetValue(text2, out expression))
				{
					expression = new Expression(text2, EvaluateOptions.IgnoreCase | EvaluateOptions.UseDoubleForAbsFunction);
					expression.EvaluateFunction += XUiFromXml.NCalcIdentifierDefinedFunction;
					XUiFromXml.expressionCache.Add(text2, expression);
				}
				expression.Parameters = _controlParams;
				string value2;
				try
				{
					object obj = expression.Evaluate();
					if (obj is decimal)
					{
						decimal value = (decimal)obj;
						value2 = value.ToCultureInvariantString("0.########");
					}
					else if (obj is float)
					{
						float value3 = (float)obj;
						value2 = value3.ToCultureInvariantString();
					}
					else if (obj is double)
					{
						double value4 = (double)obj;
						value2 = value4.ToCultureInvariantString();
					}
					else
					{
						value2 = obj.ToString();
					}
				}
				catch (ArgumentException ex)
				{
					LogType level2 = LogType.Error;
					string[] array = new string[7];
					array[0] = "[XUi] Control parameter '";
					array[1] = ex.ParamName;
					array[2] = "' undefined (in: ";
					int num3 = 3;
					XName name2 = xattribute.Name;
					array[num3] = ((name2 != null) ? name2.ToString() : null);
					array[4] = "=\"";
					array[5] = text;
					array[6] = "\")";
					XUiFromXml.logForNode(level2, _node, string.Concat(array));
					value2 = "";
				}
				catch (Exception e)
				{
					XUiFromXml.logForNode(LogType.Exception, _node, "[XUi] Control expression can not be evaluated: " + text2);
					Log.Exception(e);
					value2 = "";
				}
				text = text.Remove(num, count).Insert(num, value2);
				flag = true;
			}
			if (flag)
			{
				xattribute.Value = text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void NCalcIdentifierDefinedFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (_name.EqualsCaseInsensitive("defined") && parameters.Length == 1)
		{
			Identifier identifier = parameters[0].ParsedExpression as Identifier;
			if (identifier != null)
			{
				string name = identifier.Name;
				_args.Result = parameters[0].Parameters.ContainsKey(name);
				_args.HasResult = true;
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttributes(XElement _node, XUiView _viewComponent, XUiController _parent, Dictionary<string, object> _controlParams = null)
	{
		string localName = _node.Name.LocalName;
		XUiFromXml.StyleData styleData;
		if (XUiFromXml.styles.TryGetValue(localName, out styleData))
		{
			foreach (KeyValuePair<string, XUiFromXml.StyleEntryData> keyValuePair in styleData.StyleEntries)
			{
				XUiFromXml.StyleEntryData value = keyValuePair.Value;
				XUiFromXml.parseAttribute(_viewComponent, value.Name, value.Value, _parent, _controlParams);
			}
		}
		if (_node.HasAttribute("style"))
		{
			string[] array = _node.GetAttribute("style").Replace(" ", "").Split(',', StringSplitOptions.None);
			int i = 0;
			while (i < array.Length)
			{
				string text = array[i];
				string text2 = localName + "." + text;
				if (XUiFromXml.styles.TryGetValue(text2, out styleData))
				{
					using (Dictionary<string, XUiFromXml.StyleEntryData>.Enumerator enumerator = styleData.StyleEntries.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, XUiFromXml.StyleEntryData> keyValuePair2 = enumerator.Current;
							XUiFromXml.StyleEntryData value2 = keyValuePair2.Value;
							XUiFromXml.parseAttribute(_viewComponent, value2.Name, value2.Value, _parent, _controlParams);
						}
						goto IL_1A1;
					}
					goto IL_12A;
				}
				goto IL_12A;
				IL_1A1:
				i++;
				continue;
				IL_12A:
				if (XUiFromXml.styles.TryGetValue(text, out styleData))
				{
					using (Dictionary<string, XUiFromXml.StyleEntryData>.Enumerator enumerator = styleData.StyleEntries.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, XUiFromXml.StyleEntryData> keyValuePair3 = enumerator.Current;
							XUiFromXml.StyleEntryData value3 = keyValuePair3.Value;
							XUiFromXml.parseAttribute(_viewComponent, value3.Name, value3.Value, _parent, _controlParams);
						}
						goto IL_1A1;
					}
				}
				XUiFromXml.logForNode(LogType.Error, _node, "[XUi] Style key '" + text2 + "' not found!");
				goto IL_1A1;
			}
		}
		foreach (XAttribute xattribute in _node.Attributes())
		{
			string localName2 = xattribute.Name.LocalName;
			if (!(localName2 == "style"))
			{
				string value4 = xattribute.Value;
				if (XUiFromXml.IsStyleRef(value4))
				{
					string text3 = value4.Substring(1, value4.Length - 2);
					XUiFromXml.StyleEntryData styleEntryData;
					if (!XUiFromXml.styles["global"].StyleEntries.TryGetValue(text3, out styleEntryData))
					{
						XUiFromXml.logForNode(LogType.Error, _node, "[XUi] Global style key '" + text3 + "' not found!");
						continue;
					}
					value4 = styleEntryData.Value;
				}
				string attributeNameLower = localName2.ToLower();
				XUiFromXml.parseAttribute(_viewComponent, attributeNameLower, value4, _parent, _controlParams);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttribute(XUiView _viewComponent, string _attributeNameLower, string _value, XUiController _parent, Dictionary<string, object> _controlParams = null)
	{
		if (_viewComponent == null)
		{
			_controlParams[_attributeNameLower] = _value;
			return;
		}
		_viewComponent.ParseAttributeViewAndController(_attributeNameLower, _value, _parent, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseController(XElement _node, XUiView _viewComponent, XUiController _parent)
	{
		XUi xui = _viewComponent.xui;
		XUiController xuiController = null;
		if (_node.HasAttribute("controller"))
		{
			string attribute = _node.GetAttribute("controller");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", attribute);
			if (typeWithPrefix == null)
			{
				XUiFromXml.logForNode(xui.IgnoreMissingClass ? LogType.Warning : LogType.Error, _node, "[XUi] Controller '" + attribute + "' not found, using base XUiController");
				xuiController = (xui.IgnoreMissingClass ? new XUiControllerMissing() : new XUiController());
			}
			else if (typeWithPrefix.IsAbstract)
			{
				XUiFromXml.logForNode(LogType.Error, _node, "[XUi] Controller '" + attribute + "' not instantiable, class is abstract");
			}
			else
			{
				xuiController = (XUiController)Activator.CreateInstance(typeWithPrefix);
			}
		}
		else
		{
			xuiController = new XUiController();
		}
		_viewComponent.Controller = xuiController;
		xuiController.xui = xui;
		XUiC_DragAndDropWindow xuiC_DragAndDropWindow = _viewComponent.Controller as XUiC_DragAndDropWindow;
		if (xuiC_DragAndDropWindow != null)
		{
			xui.dragAndDrop = xuiC_DragAndDropWindow;
		}
		XUiC_OnScreenIcons xuiC_OnScreenIcons = _viewComponent.Controller as XUiC_OnScreenIcons;
		if (xuiC_OnScreenIcons != null)
		{
			xui.onScreenIcons = xuiC_OnScreenIcons;
		}
		if (_parent != null)
		{
			xuiController.Parent = _parent;
			_parent.AddChild(_viewComponent.Controller);
		}
		_viewComponent.SetDefaults(_parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logForNode(LogType _level, XElement _node, string _message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_message);
		stringBuilder.Append(" --- hierarchy: ");
		XUiFromXml.logTree(stringBuilder, _node);
		string txt = stringBuilder.ToString();
		switch (_level)
		{
		case LogType.Error:
		case LogType.Exception:
			Log.Error(txt);
			return;
		case LogType.Warning:
			Log.Warning(txt);
			return;
		case LogType.Log:
			Log.Out(txt);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logTree(StringBuilder _sb, XElement _node)
	{
		if (_node.Parent != null)
		{
			XUiFromXml.logTree(_sb, _node.Parent);
			_sb.Append(" -> ");
		}
		if (_node != null)
		{
			if (_node.HasAttribute("name"))
			{
				_sb.Append(_node.Name);
				_sb.Append(" (");
				_sb.Append(_node.GetAttribute("name"));
				_sb.Append(")");
				return;
			}
			_sb.Append(_node.Name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsStyleRef(string _value)
	{
		return _value.StartsWith("[", StringComparison.Ordinal) && _value.IndexOf("]", StringComparison.Ordinal) == _value.Length - 1 && _value.IndexOf("[", 1, StringComparison.Ordinal) < 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> windowData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedWindows;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> controlData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Dictionary<string, object>> controlDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XUiFromXml.StyleData> styles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Expression> expressionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XmlFile mainXuiXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiFromXml.DebugLevel debugXuiLoading;

	public enum DebugLevel
	{
		Off,
		Warning,
		Verbose
	}

	public class StyleData
	{
		public StyleData(string _name, string _type)
		{
			this.Type = _type;
			this.Name = _name;
			if (!string.IsNullOrEmpty(this.Type) && !string.IsNullOrEmpty(this.Name))
			{
				this.KeyName = this.Type + "." + this.Name;
				return;
			}
			if (!string.IsNullOrEmpty(this.Type))
			{
				this.KeyName = this.Type;
				return;
			}
			if (!string.IsNullOrEmpty(this.Name))
			{
				this.KeyName = this.Name;
				return;
			}
			Log.Error("[XUi] Style entry with neither 'Type' or 'Name' attribute");
		}

		public readonly string Name;

		public readonly string Type;

		public readonly string KeyName;

		public readonly Dictionary<string, XUiFromXml.StyleEntryData> StyleEntries = new Dictionary<string, XUiFromXml.StyleEntryData>();
	}

	public class StyleEntryData
	{
		public string Value
		{
			get
			{
				string text = this.value;
				if (!XUiFromXml.IsStyleRef(text))
				{
					return text;
				}
				string key = text.Substring(1, text.Length - 2);
				XUiFromXml.StyleEntryData styleEntryData;
				if (!XUiFromXml.styles["global"].StyleEntries.TryGetValue(key, out styleEntryData))
				{
					return text;
				}
				text = styleEntryData.Value;
				this.value = text;
				return text;
			}
		}

		public StyleEntryData(string _name, string _value)
		{
			this.Name = _name;
			this.value = _value;
		}

		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public string value;
	}
}
