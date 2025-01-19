using System;

public class ParentControllerState
{
	public ParentControllerState(XUiController parentController)
	{
		this.m_parentController = parentController;
		if (this.m_parentController == null)
		{
			return;
		}
		this.m_isVisible = this.m_parentController.ViewComponent.IsVisible;
		this.m_isEscClosable = this.m_parentController.WindowGroup.isEscClosable;
	}

	public void Hide()
	{
		if (this.m_parentController == null)
		{
			return;
		}
		this.m_parentController.ViewComponent.IsVisible = false;
		this.m_parentController.WindowGroup.isEscClosable = false;
	}

	public void Restore()
	{
		if (this.m_parentController == null)
		{
			return;
		}
		this.m_parentController.ViewComponent.IsVisible = this.m_isVisible;
		this.m_parentController.WindowGroup.isEscClosable = this.m_isEscClosable;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController m_parentController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_isVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_isEscClosable;
}
