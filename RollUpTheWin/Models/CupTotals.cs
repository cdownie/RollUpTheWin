using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace RollUpTheWin
{
	public class CupTotals : DynamicModel
	{
		public CupTotals() : base("RollUp", "CupTotals") { }
	}
}