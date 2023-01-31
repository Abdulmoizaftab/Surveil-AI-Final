// Title: Demo code for jQuery Datatables
// Location: tables.data.html
// Dependency File(s):

// -----------------------------------------------------------------------------

(function(window, document, $, undefined) {
  "use strict";
$(function() {

    $('#bs4-table').DataTable();
    $('#bs-role-table').DataTable();
    $('#bs-job-table').DataTable();
    $('#bs4-table-sort').DataTable({
        columnDefs: [{ type: 'date', 'targets': [1] }],
        order: [[1, 'desc']]
    });

});

})(window, document, window.jQuery);
