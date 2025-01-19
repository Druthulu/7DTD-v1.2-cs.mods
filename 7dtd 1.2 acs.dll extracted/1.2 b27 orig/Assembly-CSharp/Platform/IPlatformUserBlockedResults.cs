using System;

namespace Platform
{
	public interface IPlatformUserBlockedResults
	{
		IPlatformUser User { get; }

		void Block(EBlockType blockType);

		void BlockAll()
		{
			foreach (EBlockType blockType in EnumUtils.Values<EBlockType>())
			{
				this.Block(blockType);
			}
		}

		void Error();
	}
}
