using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Integrations
{
	internal interface IModIntegration
	{
		string Label { get; }

		bool IsLoaded { get; }
	}
}
