using System;

public class XUi_Thickness
{
	public XUi_Thickness(int newLeft, int newTop, int newRight, int newBottom)
	{
		this.left = newLeft;
		this.top = newTop;
		this.right = newRight;
		this.bottom = newBottom;
	}

	public XUi_Thickness(int newLeftRight, int newTopBottom) : this(newLeftRight, newTopBottom, newLeftRight, newTopBottom)
	{
	}

	public XUi_Thickness(int newSides) : this(newSides, newSides, newSides, newSides)
	{
	}

	public static XUi_Thickness Parse(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		switch (array.Length)
		{
		case 1:
			return new XUi_Thickness(int.Parse(array[0]));
		case 2:
			return new XUi_Thickness(int.Parse(array[0]), int.Parse(array[1]));
		case 4:
			return new XUi_Thickness(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3]));
		}
		return new XUi_Thickness(0);
	}

	public int left;

	public int top;

	public int right;

	public int bottom;
}
