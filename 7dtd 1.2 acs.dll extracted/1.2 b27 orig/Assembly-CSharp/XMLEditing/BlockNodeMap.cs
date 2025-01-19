using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing
{
	public class BlockNodeMap : IEnumerable<KeyValuePair<string, BlockNode>>, IEnumerable
	{
		public IEnumerator<KeyValuePair<string, BlockNode>> GetEnumerator()
		{
			return this.blockNodes.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GetEnumerator()
		{
			return this.blockNodes.GetEnumerator();
		}

		public bool TryGetValue(string targetName, out BlockNode blockNode)
		{
			return this.blockNodes.TryGetValue(targetName, out blockNode);
		}

		public int Count
		{
			get
			{
				return this.blockNodes.Count;
			}
		}

		public void PopulateFromFile(string blocksFilePath)
		{
			XDocument xdocument = XMLUtils.LoadXDocument(blocksFilePath);
			this.root = xdocument.Root;
			this.Refresh();
		}

		public void PopulateFromRoot(XElement root)
		{
			this.root = root;
			this.Refresh();
		}

		public void Refresh()
		{
			if (this.root == null)
			{
				Debug.LogError("Refresh failed: root element is null. This may occur if you have not called one of the PopulateFrom[...] methods prior to calling Refresh. Otherwise there may be an error in the source xml.");
				return;
			}
			if (!this.root.HasElements)
			{
				Debug.LogError("Refresh failed: root element has no child elements.");
				return;
			}
			this.blockNodes.Clear();
			foreach (XElement element in this.root.Elements(XNames.block))
			{
				string attribute = element.GetAttribute(XNames.name);
				BlockNode value = new BlockNode
				{
					Name = attribute,
					Element = element
				};
				this.blockNodes[attribute] = value;
			}
			foreach (BlockNode blockNode in this.blockNodes.Values)
			{
				foreach (XElement element2 in blockNode.Element.Elements(XNames.property))
				{
					string text = element2.GetAttribute(XNames.name);
					if (text == "Extends")
					{
						string attribute2 = element2.GetAttribute(XNames.value);
						BlockNode blockNode2;
						if (this.blockNodes.TryGetValue(attribute2, out blockNode2))
						{
							blockNode2.AddChild(blockNode);
							foreach (string key in element2.GetAttribute(XNames.param1).Split(new char[]
							{
								','
							}, StringSplitOptions.RemoveEmptyEntries))
							{
								BlockNode.ElementInfo elementInfo;
								if (!blockNode.ElementInfos.TryGetValue(key, out elementInfo))
								{
									elementInfo = new BlockNode.ElementInfo();
									blockNode.ElementInfos[key] = elementInfo;
								}
								elementInfo.CanInherit = false;
							}
							BlockNode.ElementInfo elementInfo2 = new BlockNode.ElementInfo();
							elementInfo2.CanInherit = false;
							elementInfo2.Element = element2;
							elementInfo2.IsClass = false;
							blockNode.ElementInfos["Extends"] = elementInfo2;
						}
						else
						{
							Debug.LogError(string.Concat(new string[]
							{
								"Failed to find parent BlockNode \"",
								attribute2,
								"\" for block \"",
								blockNode.Name,
								"\""
							}));
						}
					}
					else
					{
						bool isClass = false;
						if (string.IsNullOrWhiteSpace(text))
						{
							string attribute3 = element2.GetAttribute(XNames.class_);
							if (string.IsNullOrWhiteSpace(attribute3))
							{
								continue;
							}
							isClass = true;
							text = attribute3;
						}
						BlockNode.ElementInfo elementInfo3;
						if (!blockNode.ElementInfos.TryGetValue(text, out elementInfo3))
						{
							elementInfo3 = new BlockNode.ElementInfo();
							elementInfo3.CanInherit = (text != "CreativeMode");
							blockNode.ElementInfos[text] = elementInfo3;
						}
						elementInfo3.Element = element2;
						elementInfo3.IsClass = isClass;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, BlockNode> blockNodes = new Dictionary<string, BlockNode>(StringComparer.OrdinalIgnoreCase);

		[PublicizedFrom(EAccessModifier.Private)]
		public XElement root;
	}
}
