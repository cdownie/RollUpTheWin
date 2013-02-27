using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RollUpTheWin.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/
		public ActionResult Index()
		{
			return View();
		}

		public JsonResult GetCups()
		{
			dynamic cupsTable = new CupTotals();
			var cups = cupsTable.All(OrderBy: "name");

			var jsonWriter = new JsonFx.Json.JsonWriter();
			string jsonFX = jsonWriter.Write(cups);

			return Json(jsonFX, JsonRequestBehavior.AllowGet);

		}

		public JsonResult GetTotals()
		 {
			 dynamic cupsTable = new CupTotals();
			 var wins = cupsTable.Sum(columns: "wins");
			 var total = cupsTable.Sum(columns: "total");

			 var result = new
			 {
				 wins = wins,
				 total = total
			 };

			 return Json(result, JsonRequestBehavior.AllowGet);

		 }

		public EmptyResult AddWin(string id)
		{
			dynamic totals = new CupTotals();
			var person = totals.Single(where: "id=@0", args: id);

			person.wins ++;
			person.total ++;

			totals.Update(person, person.id);

			return new EmptyResult();
		}

		public EmptyResult AddTotal(string id)
		{
			dynamic totals = new CupTotals();
			var person = totals.Single(where: "id=@0", args: id);

			person.total ++;

			totals.Update(person, person.id);

			return new EmptyResult();
		}
	}
}
