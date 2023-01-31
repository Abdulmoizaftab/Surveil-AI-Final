
$(document).ready(function () {
    $('.block-special').on('keypress', function (event) {
        var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
        var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
        if (!regex.test(key)) {
            event.preventDefault();
            return false;
        }
    });

    //function ChkVal() {
    //    alert("asd");
    //    var pmid = $('#pmid').val();
    //    if (pmid == "") {
    //        $('#pmid-valid').removeClass('d-none');
    //        $('#pmid-valid').addClass('d-block');
    //    }
    //    else {
    //        $('#pmid-valid').removeClass('d-block');
    //        $('#pmid-valid').addClass('d-none');
    //    }
    //}


});