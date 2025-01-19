using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DataManagementBar : XUiController
{
	public override void Init()
	{
		base.Init();
		this.background = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		this.bar_used = (XUiV_Sprite)base.GetChildById("bar_used").ViewComponent;
		this.bar_selected_primary_fill = (XUiV_Sprite)base.GetChildById("bar_selected_primary_fill").ViewComponent;
		this.bar_selected_secondary_fill = (XUiV_Sprite)base.GetChildById("bar_selected_secondary_fill").ViewComponent;
		this.bar_selected_tertiary_fill = (XUiV_Sprite)base.GetChildById("bar_selected_tertiary_fill").ViewComponent;
		this.bar_selected_primary_outline = (XUiV_Sprite)base.GetChildById("bar_selected_primary_outline").ViewComponent;
		this.bar_selected_secondary_outline = (XUiV_Sprite)base.GetChildById("bar_selected_secondary_outline").ViewComponent;
		this.bar_selected_tertiary_outline = (XUiV_Sprite)base.GetChildById("bar_selected_tertiary_outline").ViewComponent;
		this.bar_hovered_outline = (XUiV_Sprite)base.GetChildById("bar_hovered_outline").ViewComponent;
		this.bar_required = (XUiV_Sprite)base.GetChildById("bar_required").ViewComponent;
		this.bar_pending = (XUiV_Sprite)base.GetChildById("bar_pending").ViewComponent;
		this.selectionFillColor = this.bar_selected_primary_fill.Color;
		this.fullWidth = this.background.Size.x;
		this.fullHeight = this.background.Size.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public void SetDisplayMode(XUiC_DataManagementBar.DisplayMode displayMode)
	{
		if (this.displayMode == displayMode)
		{
			return;
		}
		this.displayMode = displayMode;
		this.IsDirty = true;
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion primaryRegion)
	{
		this.SetSelectedByteRegion(primaryRegion, XUiC_DataManagementBar.BarRegion.None, XUiC_DataManagementBar.BarRegion.None);
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion primaryRegion, XUiC_DataManagementBar.BarRegion secondaryRegion)
	{
		this.SetSelectedByteRegion(primaryRegion, secondaryRegion, XUiC_DataManagementBar.BarRegion.None);
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion primaryRegion, XUiC_DataManagementBar.BarRegion secondaryRegion, XUiC_DataManagementBar.BarRegion tertiaryRegion)
	{
		if (!this.primaryByteRegion.Equals(primaryRegion))
		{
			this.primaryByteRegion = primaryRegion;
			this.IsDirty = true;
		}
		if (!this.secondaryByteRegion.Equals(secondaryRegion))
		{
			this.secondaryByteRegion = secondaryRegion;
			this.IsDirty = true;
		}
		if (!this.tertiaryByteRegion.Equals(tertiaryRegion))
		{
			this.tertiaryByteRegion = tertiaryRegion;
			this.IsDirty = true;
		}
	}

	public void SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion region)
	{
		if (!this.hoveredByteRegion.Equals(region))
		{
			this.hoveredByteRegion = region;
			this.IsDirty = true;
		}
	}

	public void SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion region)
	{
		if (!this.archivePreviewByteRegion.Equals(region))
		{
			this.archivePreviewByteRegion = region;
			this.IsDirty = true;
		}
	}

	public void SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth selectionDepth)
	{
		if (this.focusedSelectionDepth != selectionDepth)
		{
			this.focusedSelectionDepth = selectionDepth;
			this.IsDirty = true;
		}
	}

	public void SetDeleteHovered(bool hovered)
	{
		if (this.deleteHovered != hovered)
		{
			this.deleteHovered = hovered;
			this.IsDirty = true;
		}
	}

	public void SetDeleteWindowDisplayed(bool displayed)
	{
		if (this.deleteWindowDisplayed != displayed)
		{
			this.deleteWindowDisplayed = displayed;
			this.IsDirty = true;
		}
	}

	public void SetUsedBytes(long usedBytes)
	{
		if (this.usedBytes == usedBytes)
		{
			return;
		}
		this.usedBytes = usedBytes;
		this.IsDirty = true;
	}

	public void SetAllowanceBytes(long allowanceBytes)
	{
		if (this.allowanceBytes == allowanceBytes)
		{
			return;
		}
		this.allowanceBytes = allowanceBytes;
		this.bytesToPixels = (((float)allowanceBytes > 0f) ? ((float)this.fullWidth / (float)allowanceBytes) : 0f);
		this.IsDirty = true;
	}

	public void SetPendingBytes(long pendingBytes)
	{
		if (this.pendingBytes == pendingBytes)
		{
			return;
		}
		this.pendingBytes = pendingBytes;
		this.IsDirty = true;
	}

	public long GetPendingBytes()
	{
		return this.pendingBytes;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		this.bar_used.IsVisible = false;
		this.bar_selected_primary_fill.IsVisible = false;
		this.bar_selected_secondary_fill.IsVisible = false;
		this.bar_selected_tertiary_fill.IsVisible = false;
		this.bar_selected_primary_outline.IsVisible = false;
		this.bar_selected_secondary_outline.IsVisible = false;
		this.bar_selected_tertiary_outline.IsVisible = false;
		this.bar_hovered_outline.IsVisible = false;
		this.bar_required.IsVisible = false;
		this.bar_pending.IsVisible = false;
		XUiC_DataManagementBar.DisplayMode displayMode = this.displayMode;
		if (displayMode != XUiC_DataManagementBar.DisplayMode.Selection && displayMode == XUiC_DataManagementBar.DisplayMode.Preview)
		{
			this.RefreshPreviewMode();
		}
		else
		{
			this.RefreshSelectionMode();
		}
		base.RefreshBindings(true);
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshPreviewMode()
	{
		if (this.allowanceBytes <= 0L || (this.usedBytes <= 0L && this.pendingBytes <= 0L))
		{
			return;
		}
		int num = Mathf.CeilToInt(this.bytesToPixels * (float)this.usedBytes);
		int num2 = 0;
		int num3 = 0;
		int num6;
		if (this.pendingBytes > 0L)
		{
			long num4 = this.allowanceBytes - this.usedBytes;
			if (num4 < this.pendingBytes)
			{
				num3 = this.fullWidth - num;
				long num5 = this.pendingBytes - num4;
				num2 = Math.Clamp(Mathf.CeilToInt(this.bytesToPixels * (float)num5), 3, num);
				num6 = num - num2;
			}
			else
			{
				num3 = Math.Clamp(Mathf.CeilToInt(this.bytesToPixels * (float)this.pendingBytes), 3, this.fullWidth - 3);
				num6 = Math.Min(num, this.fullWidth - num3);
			}
		}
		else
		{
			num6 = num;
		}
		if (num6 > 0)
		{
			this.bar_used.IsVisible = true;
			this.bar_used.Size = new Vector2i(num6, this.fullHeight);
		}
		if (num2 > 0)
		{
			this.bar_required.IsVisible = true;
			this.bar_required.Position = new Vector2i(num6, 0);
			this.bar_required.Size = new Vector2i(num2, this.fullHeight);
		}
		if (num3 > 0)
		{
			this.bar_pending.IsVisible = true;
			this.bar_pending.Position = new Vector2i(num6 + num2, 0);
			this.bar_pending.Size = new Vector2i(num3, this.fullHeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSelectionMode()
	{
		XUiC_DataManagementBar.<>c__DisplayClass52_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		if (this.allowanceBytes <= 0L || this.usedBytes <= 0L)
		{
			return;
		}
		int val = Mathf.CeilToInt(this.bytesToPixels * (float)this.usedBytes);
		this.bar_used.IsVisible = true;
		this.bar_used.Size = new Vector2i(Math.Max(val, 8), this.fullHeight);
		CS$<>8__locals1.maxPosition = this.fullWidth;
		this.<RefreshSelectionMode>g__UpdateRegion|52_1(this.primaryByteRegion, this.bar_selected_primary_fill, this.bar_selected_primary_outline, XUiC_DataManagementBar.SelectionDepth.Primary, ref CS$<>8__locals1);
		this.<RefreshSelectionMode>g__UpdateRegion|52_1(this.secondaryByteRegion, this.bar_selected_secondary_fill, this.bar_selected_secondary_outline, XUiC_DataManagementBar.SelectionDepth.Secondary, ref CS$<>8__locals1);
		this.<RefreshSelectionMode>g__UpdateRegion|52_1(this.tertiaryByteRegion, this.bar_selected_tertiary_fill, this.bar_selected_tertiary_outline, XUiC_DataManagementBar.SelectionDepth.Tertiary, ref CS$<>8__locals1);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "warningtext"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		long num = this.allowanceBytes - this.usedBytes;
		if (this.displayMode == XUiC_DataManagementBar.DisplayMode.Selection || num >= this.pendingBytes)
		{
			_value = string.Empty;
			return true;
		}
		long bytes = this.pendingBytes - num;
		_value = string.Format(Localization.Get("xuiDmBarRequiredSpaceWarning", false), XUiC_DataManagement.FormatMemoryString(bytes));
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RefreshSelectionMode>g__GetPixelValues|52_0(XUiC_DataManagementBar.BarRegion byteRegion, int maxPosition, out int position, out int width, ref XUiC_DataManagementBar.<>c__DisplayClass52_0 A_5)
	{
		position = Mathf.Min(Mathf.FloorToInt(this.bytesToPixels * (float)byteRegion.Start), maxPosition - 8);
		width = Mathf.Max(Mathf.CeilToInt(this.bytesToPixels * (float)byteRegion.Size), 8);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RefreshSelectionMode>g__UpdateRegion|52_1(XUiC_DataManagementBar.BarRegion byteRegion, XUiV_Sprite fillSprite, XUiV_Sprite outlineSprite, XUiC_DataManagementBar.SelectionDepth depth, ref XUiC_DataManagementBar.<>c__DisplayClass52_0 A_5)
	{
		if (this.focusedSelectionDepth == depth && this.hoveredByteRegion.Size > 0L)
		{
			int x;
			int x2;
			this.<RefreshSelectionMode>g__GetPixelValues|52_0(this.hoveredByteRegion, A_5.maxPosition, out x, out x2, ref A_5);
			this.bar_hovered_outline.IsVisible = true;
			this.bar_hovered_outline.Position = new Vector2i(x, 0);
			this.bar_hovered_outline.Size = new Vector2i(x2, this.fullHeight);
		}
		if (byteRegion.Size <= 0L)
		{
			return;
		}
		int num;
		int num2;
		this.<RefreshSelectionMode>g__GetPixelValues|52_0(byteRegion, A_5.maxPosition, out num, out num2, ref A_5);
		fillSprite.IsVisible = true;
		fillSprite.Position = new Vector2i(num, 0);
		fillSprite.Size = new Vector2i(num2, this.fullHeight);
		float num3 = ((this.focusedSelectionDepth == XUiC_DataManagementBar.SelectionDepth.Secondary) ? 1f : 0.5f) * (float)(this.focusedSelectionDepth - depth);
		Color color = this.selectionFillColor;
		color = Color.Lerp(color, Color.white, 0.5f * Mathf.Abs(num3));
		bool flag = this.deleteHovered || this.deleteWindowDisplayed;
		if (num3 > 0f)
		{
			color.a = Mathf.Lerp(color.a, 0.5f, num3);
		}
		else if (flag)
		{
			color = Color.Lerp(color, XUiC_DataManagementBar.selectionOutlineColorDelete, 0.2f);
		}
		fillSprite.Color = color;
		if (!flag || num3 >= 0f)
		{
			outlineSprite.IsVisible = true;
			outlineSprite.Position = fillSprite.Position;
			outlineSprite.Size = fillSprite.Size;
			Color color2;
			if (this.focusedSelectionDepth == depth)
			{
				color2 = (flag ? XUiC_DataManagementBar.selectionOutlineColorDelete : XUiC_DataManagementBar.selectionOutlineColor);
			}
			else
			{
				color2 = XUiC_DataManagementBar.selectionOutlineColorFade;
			}
			outlineSprite.Color = color2;
		}
		if (this.focusedSelectionDepth == depth && this.archivePreviewByteRegion.Size > 0L)
		{
			int x3;
			int x4;
			this.<RefreshSelectionMode>g__GetPixelValues|52_0(this.archivePreviewByteRegion, A_5.maxPosition, out x3, out x4, ref A_5);
			this.bar_pending.IsVisible = true;
			this.bar_pending.Position = new Vector2i(x3, 0);
			this.bar_pending.Size = new Vector2i(x4, this.fullHeight);
		}
		A_5.maxPosition = num + num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_used;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_required;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_pending;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_primary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_secondary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_tertiary_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_primary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_secondary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_tertiary_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_hovered_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public long usedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long allowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion primaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion secondaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion tertiaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion hoveredByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion archivePreviewByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.SelectionDepth focusedSelectionDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteWindowDisplayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bytesToPixels;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColor = new Color32(250, byte.MaxValue, 163, 193);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorFade = new Color32(250, byte.MaxValue, 163, 86);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorDelete = new Color32(234, 67, 53, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectionFillColor;

	public enum SelectionDepth
	{
		Primary,
		Secondary,
		Tertiary
	}

	public struct BarRegion : IEquatable<XUiC_DataManagementBar.BarRegion>
	{
		public BarRegion(long offset, long size)
		{
			this.Start = offset;
			this.Size = size;
			this.End = this.Start + this.Size;
		}

		public bool Equals(XUiC_DataManagementBar.BarRegion other)
		{
			return this.Start == other.Start && this.Size == other.Size;
		}

		public readonly long Start;

		public readonly long Size;

		public readonly long End;

		public static readonly XUiC_DataManagementBar.BarRegion None = new XUiC_DataManagementBar.BarRegion(0L, 0L);
	}

	public enum DisplayMode
	{
		Selection,
		Preview
	}
}
