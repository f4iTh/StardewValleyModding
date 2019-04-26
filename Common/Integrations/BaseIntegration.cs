using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Integrations
{
	internal abstract class BaseIntegration : IModIntegration
	{
		protected string ModID { get; }
	}
}
