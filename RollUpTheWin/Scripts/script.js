// templating jQuery extension
$.fn.parseTemplate = function (data) {
	var str = (this).html();
	var _tmplCache = {}
	var err = "";
	try {
		var func = _tmplCache[str];
		if (!func) {
			var strFunc =
				"var p=[],print=function(){p.push.apply(p,arguments);};" +
								"with(obj){p.push('" +
				str.replace(/[\r\t\n]/g, " ")
					.replace(/'(?=[^#]*#>)/g, "\t")
					.split("'").join("\\'")
					.split("\t").join("'")
					.replace(/<#=(.+?)#>/g, "',$1,'")
					.split("<#").join("');")
					.split("#>").join("p.push('")
					+ "');}return p.join('');";

			//alert(strFunc);
			func = new Function("obj", strFunc);
			_tmplCache[str] = func;
		}
		return func(data);
	} catch (e) { err = e.message; }
	return "< # ERROR: " + err.toString() + " # >";
}

function getGrid(id) {
	$.ajax({
		url: $('a#cups_url').attr('href'),
		success: function (data) {
			$('#cups tbody').empty().append($('#cup_rows').parseTemplate({ d: JSON.parse(data) }));
			$('tr#' + id + ' td').effect("highlight");
			bindButtons();
		}
	})
}

function getTotals() {
	$.ajax({
		url: $('a#total_url').attr('href'),
		success: function (data) {
			$('#cups tfoot').empty().append($('#cup_footer').parseTemplate(data));
		}
	})
}

function FillTable(id) {
	getGrid(id);
	getTotals();
}

function bindButtons() {
	$('td.wins, td.total').hover(function () {
		$(this).find('button').show();
		},
		function () {
			$(this).find('button').hide();
		}
	);

		$('button.add_win').click(function () {
			var id = $(this).parent().parent().attr('id');

			$.ajax({
				url: $('a#add_url').attr('href'),
				data: 'id=' + id,
				success: function () {
					FillTable(id);
				}
			});
		});

	$('button.add_total').click(function () {
		var id = $(this).parent().parent().attr('id');

		$.ajax({
			url: $('a#add_total_url').attr('href'),
			data: 'id=' + id,
			success: function () {
				FillTable(id);
			}
		});
	});


}

$(function () {

	FillTable();

});