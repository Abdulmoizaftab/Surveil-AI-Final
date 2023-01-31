// -----------------------------------------------------------------------------
// Title: Recent Transactions
// ID: #recent-transaction-table
// Location: index.html
// Dependency File(s):
// -----------------------------------------------------------------------------
(function(window, document, $, undefined) {
	  "use strict";
	$(function() {
		$('#recent-transaction-table').DataTable({
			"columnDefs": [{
				"targets": 'no-sort',
				"orderable": false,
			}],
			"columns": [
				null,
				null,
				null,
				null,
				{
					"width": "10%"
				}]
		});
	});

})(window, document, window.jQuery);
