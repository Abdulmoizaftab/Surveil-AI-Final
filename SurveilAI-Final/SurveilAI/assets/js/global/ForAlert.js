function myFunction() {
    var input, filter, table, tr, td, pd, i, txtValue;
    input = document.getElementById("myInput");
    filter = input.value.toUpperCase();
    table = document.getElementById("myTable");
    tr = table.getElementsByTagName("tr");
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[0];
        pd = tr[i].getElementsByTagName("td")[1];
        if (td) {
            txtValue = td.textContent || td.innerText;
            couValue = pd.textContent || pd.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1 || couValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}

function chngstate(source) {
    var checkboxes = document.querySelectorAll('input[name="StateChk"]');
    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i] != source)
            checkboxes[i].checked = source.checked;
    }
}

function chng(source) {
    var checkboxes = document.querySelectorAll('input[name="Alldevices"]');
    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i] != source)
            checkboxes[i].checked = source.checked;
    }
}

//---------Function to make query for Resource ---------
function Query() {
    var val = $("input[name='Radio']:checked").val();
    var sql = [];
    var dt = [];
    var query = "";
    if (val === "hierarchyDiv") {
        $.each($("input[name='hierarchy']:checked"), function () {
            sql.push($(this).val());
        });
        $.each($("input[name='Dtype']:checked"), function () {
            dt.push($(this).val());
        });
        if (sql != "") {
            query += " d.HierLevel IN (" + sql + ") ";
            if (dt != "") {
                query += "AND d.DeviceType IN(" + dt + ")";
            }
        }
        else {
            if (dt != "") {
                query = " d.DeviceType IN(" + dt + ")";
            }
        }
    }
    else if (val === "deviceDiv") {
        $.each($("input[name='Alldevices']:checked"), function () {
            sql.push($(this).val());
        });
        if (sql != "") {
            query = "d.DeviceID IN (" + sql + ")";
        }
    }
    $('#condition').attr("value", query);
}
//---------Function to make query for Resource ---------

//---------Function to Set event trigger ---------------
function Fevents() {
    var total = 0;
    $.each($("input[name='StateChk']:checked"), function () {
        var count = 1;
        var i = $(this).val();
        for (i = 0; i < ($(this).val() - 1); i++) {
            count *= 2;
        }
        total += count;
        $('#statebitmask').attr("value", total);
    });
}
//---------Function to Set event trigger ---------------

//---------Function to Set event trigger ---------------
function SecFevents() {var total = 0;
    $.each($("input[name='StateChk']:checked"), function () {
        var count = 1;
        var i = $(this).val();
        for (i = 0; i < ($(this).val() - 1); i++) {
            count *= 2;
        }
        total += count;
        $('#statebitmask').attr("value", total);
    });
}
//---------Function to Set event trigger ---------------

//---------Finish Button -----------------
function Finish() {
    var myarr = [];
    var c = $('#FRule').val();
    if (c == "Rule") {
        var Rowcount = document.getElementById('ActionBody').getElementsByTagName('tr');
        for (var i = 0; i < ((Rowcount.length) * 2); i++) {
            var x = document.getElementsByTagName('table')[3].getElementsByTagName("td")[i].textContent;
            myarr += x + "^";
        }
        $('#Actdata').attr("value", myarr);
    }
}
//---------Finish Button -----------------
//---------Finish Button -----------------
function SecFin() {
    var myarr = [];
    var Rowcount = document.getElementById('ActionBody').getElementsByTagName('tr');
    for (var i = 0; i < ((Rowcount.length) * 2); i++) {
        var x = document.getElementsByTagName('table')[2].getElementsByTagName("td")[i].textContent;
        myarr += x + "^";
    }
    $('#Actdata').attr("value", myarr);
}
//---------Finish Button -----------------
function CheckEMan() {
    if ($('[name="EMan"]').is(':checked')) {
        $(".ETime").css({ "pointer-events": "", "opacity": "" });
        $('#escalation').attr("value", 1);
    }
    else {
        $(".ETime").css({ "pointer-events": "none", "opacity": "0.4" });
        $('#escalation').attr("value", null);
    }
}

function PItrigg() {
    var val = $("#triggers").val();
    if (val == "CP") {
        $("#classification").css({ "pointer-events": "", "opacity": "" });
        $("#problempmid").css({ "pointer-events": "", "opacity": "" });
        $("#PicaD").css({ "pointer-events": "", "opacity": "" });
        $('#problemevent').attr("value", "4");
        $('#classification').attr("value", 1);
    }
    else if (val == "CI") {
        $("#classification").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#problempmid").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#PicaD").css({ "pointer-events": "none", "opacity": "0.4" });
        $('[name="EMan"]').prop('checked', false);
        $('#problemevent').attr("value", "8");
        $('#classification').attr("value", null);
        CheckEMan();
    }
    else if (val == "NI") {
        $("#classification").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#problempmid").css({ "pointer-events": "", "opacity": "" });
        $("#PicaD").css({ "pointer-events": "", "opacity": "" });
        $('#problemevent').attr("value", "1");
        $('#classification').attr("value", null);
    }
    else if (val == "RI") {
        $("#classification").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#problempmid").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#PicaD").css({ "pointer-events": "none", "opacity": "0.4" });
        $('[name="EMan"]').prop('checked', false);
        $('#problemevent').attr("value", "9");
        $('#classification').attr("value", null);
        CheckEMan();
    }
}

function TriggType(val) {
    var demovalue = val;
    $('.AllDiv2').hide();
    $("#" + demovalue).show();
    $("." + demovalue).show();
    if (demovalue === "eventR") { $('#triggertype').attr("value", "1"); } else if (demovalue === "stateR") { $('#triggertype').attr("value", "2"); } else if (demovalue === "incidentR") { $('#triggertype').attr("value", "3"); }
}

function DevType(val) {
    var demovalue = val;
    $('.AllDiv').hide();
    $("#" + demovalue).show();
    if (demovalue === "deviceDiv") {
        $("#type").css({ "pointer-events": "none", "opacity": "0.4" });
    } else {
        $("#type").css({ "pointer-events": "", "opacity": "" });
    }
}

function ForEsc() {
    if ($("#Pica").is(':checked')) {
        $(".ETime").css({ "pointer-events": "", "opacity": "" });
        $('#escalation').attr("value", 1);
    }
    else {
        $(".ETime").css({ "pointer-events": "none", "opacity": "0.4" });
        $('#escalation').attr("value", null);
    }
}

function hello() {

    var viewname = $('#viewname').val();
    $('#pmid').val(viewname);
    //conditionForDevices
    var qry = $('#condition').val();
    if (qry.includes("AND")) {
        var a = qry.split("AND");
        var hie = a[0].match(/\((.*)\)/);
        var dtp = a[1].match(/\((.*)\)/);
        hie = hie[1].split(',');
        dtp = dtp[1].split(',');
        for (var i = 0; i < hie.length; i++) {
            $('#' + hie[i]).prop('checked', true);
        }
        for (var i = 0; i < dtp.length; i++) {
            if (dtp[i] == "2") { $('#Wincor').prop('checked', true); }
            if (dtp[i] == "1000") { $('#NCR').prop('checked', true); }
            if (dtp[i] == "1001") { $('#Diebold').prop('checked', true); }
        }
        $('#radio3').click();
    }
    else if (qry.includes("HierLevel")) {
        var hie = qry.match(/\((.*)\)/);
        hie = hie[1].split(',');
        for (var i = 0; i < hie.length; i++) {
            $('#' + hie[i]).prop('checked', true);
        }
        $('#radio3').click();
    }
    else if (qry.includes("DeviceType")) {
        var dtp = qry.match(/\((.*)\)/);
        dtp = dtp[1].split(',');
        for (var i = 0; i < dtp.length; i++) {
            if (dtp[i] == "2") { $('#Wincor').prop('checked', true); }
            if (dtp[i] == "1000") { $('#NCR').prop('checked', true); }
            if (dtp[i] == "1001") { $('#Diebold').prop('checked', true); }
        }
        $('#radio3').click();
    }
    else if (qry.includes("DeviceID")) {
        var dev = qry.match(/\('(.*)'\)/);
        dev = dev[1].split(',');
        for (var i = 0; i < dev.length; i++) {
            $('#' + dev[i]).prop('checked', true);
        }
        $('#radio4').click();
        DevType("deviceDiv");
    }
    //conditionForDevices
    
    //State
    //if ($('#triggertype').val() == 2) {
        //$('#leveltri').prop('checked', true);

        var bin = $('#statebitmask').val();
        var bit = [];
        var f = 0;
        while (bin > 0) {
            bit[f] = bin % 2;
            bin = Math.trunc(bin / 2);
            f++;
        }
        for (var g = 0; g < bit.length; g++) {
            if (bit[g] == 1) {
                var s = g + 1;
                $("#S-" + s).prop('checked', true);
            }
        }
        //TriggType("stateR");
    //}
    //State
    
}

$(document).ready(function () {
    var navListItems = $('div.setup-panel div a'),
        allWells = $('.setup-content'),
        allNextBtn = $('.nextBtn');
    
    allWells.hide();

    navListItems.click(function (e) {
        e.preventDefault();
        var $target = $($(this).attr('href')),
            $item = $(this);

        if (!$item.hasClass('disabled')) {
            navListItems.removeClass('btn-primary').addClass('btn-default');
            $item.addClass('btn-primary');
            allWells.hide();
            $target.show();
            $target.find('input:eq(0)').focus();
        }
    });

    allNextBtn.click(function () {
        var curStep = $(this).closest(".setup-content"),
            curStepBtn = curStep.attr("id"),
            nextStepWizard = $('div.setup-panel div a[href="#' + curStepBtn + '"]').parent().next().children("a"),
            curInputs = curStep.find("input[type='text'],input[type='url']"),
            isValid = true;

        $(".form-group").removeClass("has-error");
        for (var i = 0; i < curInputs.length; i++) {
            if (!curInputs[i].validity.valid) {
                isValid = false;
                $(curInputs[i]).closest(".form-group").addClass("has-error");
            }
        }

        if (isValid) nextStepWizard.removeAttr('disabled').trigger('click');
    });

    $('div.setup-panel div a.btn-primary').trigger('click');

    $(function () {
        $('.AllDiv').hide();
        $('#hierarchyDiv').show();
        $(".ETime").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#CDOne").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#CDTwo").css({ "pointer-events": "none", "opacity": "0.4" });
        $('.AllDiv2').hide();
        $('#eventR').show();
        $('.eventR').show();
        var chck = $('#triggertype').val();
        if (chck == "check") {
            $('#triggertype').attr("value", "1");
        }
        chck = $('#problemevent').val();
        if (chck == "check") {
            $('#problemevent').attr("value", "4");
        }
        chck = $('#leveltriggered').val();
        if (chck == "check") {
            $('#leveltriggered').attr("value", 0);
        }
        hello();
    });

    $(".source").on("click", "#move-row", function () {
        var tr = $(this).closest("tr").remove();
        $(".dest tbody").append(tr);
    });
    $(".dest").on("click", "#move-row", function () {
        var tr = $(this).closest("tr").remove();
        $(".source tbody").append(tr);
    });



});