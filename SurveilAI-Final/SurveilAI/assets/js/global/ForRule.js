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

function myFunctionEvent() {
    var input, filter, table, tr, td, pd, i, txtValue;
    input = document.getElementById("myInputTwo");
    filter = input.value.toUpperCase();
    table = document.getElementById("myTableTwo");
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

function chng(source) {
    var checkboxes = document.querySelectorAll('input[name="Alldevices"]');
    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i] != source)
            checkboxes[i].checked = source.checked;
    }
}
//---------Function to calculate No of Days --------
function CalcDays() {
    var day = 0;
    if ($('#Mon').is(':checked')) {
        day += 1;
    }
    if ($('#Tue').is(':checked')) {
        day += 2;
    }
    if ($('#Wed').is(':checked')) {
        day += 4;
    }
    if ($('#Thr').is(':checked')) {
        day += 8;
    }
    if ($('#Fri').is(':checked')) {
        day += 16;
    }
    if ($('#Sat').is(':checked')) {
        day += 32;
    }
    if ($('#Sun').is(':checked')) {
        day += 64;
    }
    if ($('#ResNeed').is(':checked')) {
        $('#resultneeded').attr("value", "1");
    }
    else {
        $('#resultneeded').attr("value", "0");
    }
    if ($('#JourNeed').is(':checked')) {
        $('#journalneeded').attr("value", "1");
    }
    else {
        $('#journalneeded').attr("value", "0");
    }
    $('#dayofweek').attr("value", day);

}
//---------Function to calculate No of Days --------

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
            sql.push("''" + $(this).val() + "''");
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
    var myarr = [];
    var Rowcount = document.getElementById('EventTab').getElementsByTagName('tr');
    $('#noevents').attr("value", Rowcount.length);
    for (var i = 0; i < ((Rowcount.length) * 2); i++) {
        var x = document.getElementsByTagName('table')[1].getElementsByTagName("td")[i].textContent;
        x = x.trim()
        if (i % 2 === 0) { } else { x = x + "|";}
        myarr.push(x);
    }
    
    $('#eventno').attr("value", myarr);
    
    if ($('#leveltri').is(':checked')) { $('#leveltriggered').attr("value", 1); } else { $('#leveltriggered').attr("value", 0); }

    if ($('#Set').is(':checked')) { $('#setreset').attr("value", "0"); } else if ($('#Reset').is(':checked')) { $('#setreset').attr("value", "1"); } else { $('#setreset').attr("value", null); }
    var total = 0;
    $.each($("input[name='StateChk']:checked"), function () {
        var count = 1;
        var i = $(this).val();
        for (i = 0; i < ($(this).val() - 1); i++) {
            count *= 2;
        }
        total += count;
        $('#statebitmask').attr("value", total);
        //alert(total);
    });
    var hour = ($('#MHrs').val() * 60);
    var min = $('#MMin').val();
    var time = parseInt(hour) + parseInt(min);
    $('#timetoescalate').attr("value", time);
}
//---------Function to Set event trigger ---------------

//---------Function to Set event trigger ---------------
function SecFevents() {
    var myarr = [];
    var Rowcount = document.getElementById('EventTab').getElementsByTagName('tr');
    $('#noevents').attr("value", Rowcount.length);
    //for (var i = 0; i < ((Rowcount.length) * 2); i++) {
    //    var x = document.getElementsByTagName('table')[0].getElementsByTagName("td")[i].textContent;
    //    myarr.push(x.trim());
    //    console.log(x);


    //}
    for (var i = 0; i < ((Rowcount.length) * 2); i++) {
        var x = document.getElementsByTagName('table')[0].getElementsByTagName("td")[i].textContent;
        x = x.trim()
        if (i % 2 === 0) { } else { x = x + "|"; }
        myarr.push(x);
    }
    $('#eventno').attr("value", myarr);
    

    if ($('#leveltri').is(':checked')) { $('#leveltriggered').attr("value", 1); } else { $('#leveltriggered').attr("value", 0); }

    if ($('#Set').is(':checked')) { $('#setreset').attr("value", "0"); } else if ($('#Reset').is(':checked')) { $('#setreset').attr("value", "1"); } else { $('#setreset').attr("value", null); }
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
    var hour = ($('#MHrs').val() * 60);
    var min = $('#MMin').val();
    var time = parseInt(hour) + parseInt(min);
    $('#timetoescalate').attr("value", time);
}
//---------Function to Set event trigger ---------------

//---------Send E-mail ---------------
function Email() {
    //Contact Validation
    $('#contact-vali').hide();
    var radioVal = $('input[name=gg]:checked').val();
    if (radioVal == 'one') {
        var selectedRadio = "none";
        if ($('#C1').prop("checked") == true) { selectedRadio = "C1"; }
        if ($('#C2').prop("checked") == true) { selectedRadio = "C2"; }
        if ($('#C3').prop("checked") == true) { selectedRadio = "C3"; }
        if (selectedRadio == "none") { $('#contact-vali').show(); return false; } else { $('#contact-vali').hide(); }
    }
    else if (radioVal == 'two') {
        if ($('#emails').val() == "") {
            $('#contact-vali').show();
            return false;
        }
        else {
            $('#contact-vali').hide();
        }
    }
    else if (radioVal == 'three') {
        
        if ($('#AnyCont').val() == "") {
            $('#contact-vali').show();
            return false;
        }
        else {
            $('#contact-vali').hide();
        }
    }
    $('#Semail').modal('hide');
    var arr = [];
    var emails = $('#emails').val();

    if ($('#RC').is(':checked')) { emails = "$CONT$"; for (var i = 1; i < 4; i++) { if ($("#C" + i).is(':checked')) { emails += "1" } else { emails += "0" } } }
    else if ($('#EM').is(':checked')) { emails = "$emails$"; emails += $('#emails').val(); }
    else if ($('#AC').is(':checked')) { emails = $('#AnyCont').val(); }

    var subject = $('#eSubject').val();
    var message = $('#emessage').val();
    var act, onlyerror;
    if ($('#actDeactive').is(':checked')) { act = "1" } else { act = "0" }
    if ($('#onlyonerror').is(':checked')) { onlyerror = "1" } else { onlyerror = "0" }
    arr.push(emails + "~" + subject + "~" + message + "~" + act + "~" + onlyerror);

    var row = $('<tr>' +
        '<td >Send e-mail<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span><span id="EditAct" style="float:right" class="icon dripicons-document-edit"></span></td>' +
        '<td hidden>' + arr + '</td>' +        
        '</tr>');
    $('#AddAction tbody').append(row);

}
//---------Send E-mail ---------------

//---------Create incident ---------------
function Create() {
    var arr = [];
    var priority = $('#actClassification').val();
    var topic = $('#Topic').val();
    var act, onlyerror;
    if ($('#CIactDeactive').is(':checked')) { act = "1" } else { act = "0" }
    if ($('#CIonlyonerror').is(':checked')) { onlyerror = "1" } else { onlyerror = "0" }
    arr.push(priority + "~" + topic + "~" + act + "~" + onlyerror);

    var row = $('<tr>' +
        '<td>Create Incident<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span><span id="EditAct" style="float:right" class="icon dripicons-document-edit"></span></td>' +
        '<td hidden>' + arr + '</td>' +
        '</tr>');
    $('#AddAction tbody').append(row);
}
//---------Create incident ---------------

//----------BIOS Password Start-----------------
function SetPassword() {
    var arr = [];
    var setPass;
    var passType = $('input[name="RadioPassOpt"]:checked').val();
    if (passType == "StaticPassword") {
        var newPass = $("#BioPassword").val();
        if (newPass == "") {
            $('#password-valid').show();
        }
        else {
            $('#password-valid').hide();
            setPass = "-"+newPass;
        }
    }
    else {
        var dynamicPass = $('#div2').text();
        dynamicPass = dynamicPass.trim();
        dynamicPass = dynamicPass.replace(/\s+/g, '');
        if (dynamicPass.indexOf("ADDDATE") != -1 || dynamicPass.indexOf("ADDDEVICE") != -1) {
            $('#variable-valid').hide();
        }
        else {
            $('#variable-valid').show();
            return false;
        }
        var dynamicPassInput = $('#BioPasswordDynamic').val();
        var AllowedLength = 8;
        var TotalPassLength = getVariablesLenght(dynamicPass);
        var AvailableLength = AllowedLength - TotalPassLength;

        if (AvailableLength < 0) {
            $('#password-valid-dynamic').text("Max character limit reached");
            $('#password-valid-dynamic').show();
            return false;
        }
        else if (AvailableLength == 20) {
            $('#password-valid-dynamic').text("Password required");
            $('#password-valid-dynamic').show();
            return false;
        }
        else {
            $('#password-valid-dynamic').hide();
            dynamicPass = dynamicPassInput + dynamicPass;
        }
        setPass =  dynamicPass;
    }
    //if ($('#SetBioPassword').is(':checked')) { setPass = "1" } else { setPass = "0" }
    arr.push(setPass);

    var row = $('<tr>' +
        '<td>Set Password<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span><span id="EditAct" style="float:right" class="icon dripicons-document-edit"></span></td>' +
        '<td hidden>' + arr + '</td>' +
        '</tr>');
    $('#AddAction tbody').append(row);
}
//----------BIOS Password End-----------------



//----------Reboot Device Start--------------------/
function RebootDev() {
    var row = $('<tr>' +
        '<td>Reboot Device<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span></td>' +
        '<td hidden>' + "RebootATM,135" + '</td>' +
        '</tr>');
    $('#AddAction tbody').append(row);
}
//----------Reboot Device End--------------------/


//---------Close Incident ---------------
function Close() {
    var inc = $('#jobid').val();
    if (inc == "") {
        $('#inc-val').show();
        return false;
    }
    else {
        $('#inc-val').hide();
        var arr = [];
        var jobid = $('#jobid').val();
        var act, onlyerror;
        if ($('#ClactDeactive').is(':checked')) { act = "1" } else { act = "0" }
        if ($('#Clonlyonerror').is(':checked')) { onlyerror = "1" } else { onlyerror = "0" }
        arr.push(jobid + "~" + act + "~" + onlyerror);

        var row = $('<tr>' +
            '<td>Close Incident<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span><span id="EditAct" style="float:right" class="icon dripicons-document-edit"></span></td>' +
            '<td hidden>' + arr + '</td>' +
            '</tr>');
        $('#AddAction tbody').append(row);
        $('#ClIn').modal('hide');
    }
}
//---------Close Incident ---------------

//---------Finish Button -----------------
function Finish() {
    var myarr = [];
    var Rowcount = document.getElementById('ActionBody').getElementsByTagName('tr');
    for (var i = 0; i < ((Rowcount.length) * 2); i++) {
        var x = document.getElementsByTagName('table')[3].getElementsByTagName("td")[i].textContent;
        myarr += x + "^";
    }
    $('#Actdata').attr("value", myarr);
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

function SendEmail(val) {
    var demovalue = val;
    $("#" + demovalue).show();
    if (demovalue === "ReCon") {
        $("#ReCon").css({ "pointer-events": "", "opacity": "" });
        $("#EmCon").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#AnCon").css({ "pointer-events": "none", "opacity": "0.4" });
    } else if (demovalue === "EmCon") {
        $("#EmCon").css({ "pointer-events": "", "opacity": "" });
        $("#ReCon").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#AnCon").css({ "pointer-events": "none", "opacity": "0.4" });
    } else {
        $("#AnCon").css({ "pointer-events": "", "opacity": "" });
        $("#ReCon").css({ "pointer-events": "none", "opacity": "0.4" });
        $("#EmCon").css({ "pointer-events": "none", "opacity": "0.4" });
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
    //dayofweek
    var num = $('#dayofweek').val();
    if (num >= 64) { $('#Sun').prop('checked', true); num -= 64; }
    if (num >= 32) { $('#Sat').prop('checked', true); num -= 32; }
    if (num >= 16) { $('#Fri').prop('checked', true); num -= 16; }
    if (num >= 8) { $('#Thr').prop('checked', true); num -= 8; }
    if (num >= 4) { $('#Wed').prop('checked', true); num -= 4; }
    if (num >= 2) { $('#Tue').prop('checked', true); num -= 2; }
    if (num >= 1) { $('#Mon').prop('checked', true); num -= 1; }
    //dayofweek

    //conditionForDevices
    var qry = $('#condition').val();
    if (qry.includes("AND")) {
        var a = qry.split("AND");
        var hie = a[0].match(/\((.*)\)/);
        var dtp = a[1].match(/\((.*)\)/);
        hie = hie[1];
        hie = hie.replace(/\'/g, '');
        hie = hie.split(',');
        dtp = dtp[1];
        dtp = dtp.replace(/\'/g, '');
        dtp = dtp.split(',');
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
        hie = hie[1];
        hie = hie.replace(/\'/g, '');
        hie = hie.split(',');
        for (var i = 0; i < hie.length; i++) {
            $('#' + hie[i]).prop('checked', true);
        }
        $('#radio3').click();
    }
    else if (qry.includes("DeviceType")) {
        var dtp = qry.match(/\((.*)\)/);
        dtp = dtp[1];
        dtp = dtp.replace(/\'/g, '');
        dtp = dtp.split(',');
        for (var i = 0; i < dtp.length; i++) {
            if (dtp[i] == "2") { $('#Wincor').prop('checked', true); }
            if (dtp[i] == "1000") { $('#NCR').prop('checked', true); }
            if (dtp[i] == "1001") { $('#Diebold').prop('checked', true); }
        }
        $('#radio3').click();
    }
    else if (qry.includes("DeviceID")) {
        var dev = qry.match(/\((.*)\)/);
        dev = dev[1];
        dev = dev.replace(/\'/g, '');
        dev = dev.split(',');
        for (var i = 0; i < dev.length; i++) {
            $('#' + dev[i]).prop('checked', true);
        }
        $('#radio4').click();
        DevType("deviceDiv");
    }
    //conditionForDevices

    //Events
    if ($('#triggertype').val() == 1) {
        
        var evnts = $('#eventno').val();
        //evnts = evnts.split(',');
        evnts = evnts.split('|');        
        var noevnts = $('#noevents').val();
        for (var m = 0; m < (evnts.length - 1); m += 2) {
            if (evnts[m + 1] == "not assigned") {

            }
            else {
                
                //console.log(evnts[m]);
                //console.log(evnts[m + 1]);
                var row = $('<tr id="move-row">' +
                    '<td>' + evnts[m] + '</td>' +
                    '<td>' + evnts[m + 1] + '</td>' +
                    '</tr>');
                $('.dest tbody').append(row);
            }
        }
        TriggType("eventR");
    }
    //Events

    //State
    if ($('#triggertype').val() == 2) {
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
        TriggType("stateR");
    }
    //State

    //Incident
    if ($('#triggertype').val() == 3) {
        var probevent = $('#problemevent').val();
        if (probevent == 4) {
            $('#triggers').val('CP').change();
        } else if (probevent == 8) {
            $('#triggers').val('CI').change();
        } else if (probevent == 1) {
            $('#triggers').val('NI').change();
        }
        var time = $('#timetoescalate').val();
        if (time > 0) {
            var hours = Math.floor(time / 60);
            var minutes = time % 60;
            $('#MHrs').attr("value", hours);
            $('#MMin').attr("value", minutes);
            var clasf = $('#classificate').val();
            var escl = $('#escal').val();
            $('#Pica').click();
            var timetype = $('#ttype').val();
            $('#timetype').val(timetype).change();
            $('#classification').attr("value", clasf);
            $('#escalation').attr("value", escl);
        }

        TriggType("incidentR");
    }
    //Incident

    
    //Actions
    var actdata = $('#Actdata').val();
    var adata = actdata.split('^');
    for (var i = 0; i < adata.length - 1; i += 2) {
        if (adata[i] == "Reboot Device")
        {
            var row = $('<tr>' +
                '<td>' + adata[i] + '<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span></td>' +
                '<td hidden>' + adata[i + 1] + '</td>' +
                '</tr>');
            $('#AddAction tbody').append(row);
        }
        else
        {
            var row = $('<tr>' +
                '<td>' + adata[i] + '<span style="float:right" id="RemoveAction"  class="icon dripicons-cross" ></span><span id="EditAct" style="float:right" class="icon dripicons-document-edit"></span></td>' +
                '<td hidden>' + adata[i + 1] + '</td>' +
                '</tr>');
            $('#AddAction tbody').append(row);
        }
    }
    //Actions
}

$(document).ready(function () {

    var navListItems = $('div.setup-panel div a'),
        allWells = $('.setup-content'),
        allNextBtn = $('.nextBtn');

    allWells.hide();
    //send email
    $("#EmCon").css({ "pointer-events": "none", "opacity": "0.4" });
    $("#AnCon").css({ "pointer-events": "none", "opacity": "0.4" });
    //send email
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
    //Add Event
    $(".source").on("click", "#move-row", function () {
        var tr = $(this).closest("tr").remove();
        $(".dest tbody").append(tr);
    });
    //remove Event
    $(".dest").on("click", "#move-row", function () {
        var tr = $(this).closest("tr").remove();
        var f = tr.find('td').eq(0).text();
        var s = tr.find('td').eq(1).text();
        //alert(f+s);
        var eventNo = parseInt(f);
        var i = 0;
        var addindex;
        $("#myTableTwo tbody > tr").each(function () {
            var ci = parseInt($(this).find('td').eq(0).text());
            if (ci > eventNo) {
                addindex = i;
                return false;
            }
            i++;
        });

        $("#TableSearch tbody > tr").each(function () {
            var ci = parseInt($(this).find('td').eq(0).text());
            if (ci > eventNo) {
                addindex = i;
                return false;
            }
            i++;
        });
        //var row = $('<tr id="move-row">' +
        //    '<td>395 </td>' +
        //    '<td>newTest</td>' +
        //    '</tr>');
        $('#myTableTwo > tbody > tr').eq(addindex).before(tr);
        $('#TableSearch > tbody > tr').eq(addindex).before(tr);

        //$(".source tbody").append(tr);
    });
    $(".actionDelete").on("click", "#move-row", function () {
        var tr = $(this).closest("tr").remove();
        $(".source tbody").append(tr);
        
    });


});
//Tohide and display item by ID
function HideDivs(ToHide, ToShow) {
    $("#" + ToHide).css("display", "none");
    $("#" + ToShow).css("display", "block");

}

function Validate() {
    
    val = $("input[name='Radio1']:checked").val();
    if (val === "eventR") {
        if ($('#noevents').val() < 1) {
            document.getElementById('Ttype').innerHTML = "Atleast one Event Required";
            event.preventDefault();
            $('#Trig').click();
        }
    }
    else if (val === "stateR") {

    }
    else if (val === "incidentR") {

    }

    var val = $("input[name='Radio']:checked").val();
    
    if (val === "hierarchyDiv") {
        if ($("input[name='hierarchy']:checked").length < 1) {
            document.getElementById('Resources').innerHTML = "Resource Required";
            event.preventDefault();
            $('#Res').click(); 
            
        }

        else if ( $("input[name='Dtype']:checked").length < 1) {
            document.getElementById('Resources').innerHTML = "Select atleast one device profile";
            event.preventDefault();
            $('#Res').click();            

        }

        
    }
    else if (val === "deviceDiv") {
        if ($("input[name='Alldevices']:checked").length < 1) {
            document.getElementById('Resources').innerHTML = "Resource Required";
            event.preventDefault();
            $('#Res').click();
        }
    }

    if ($("input[name='Day']:checked").length < 1) {
        document.getElementById('Daysweek').innerHTML = "Atleast one Day Required";
        event.preventDefault();
        $('#RD').click();
        
    }
    var tableRow = document.getElementById("ActionBody").rows.length;
    console.log("count hai");
    console.log(tableRow);
    if (tableRow == 0) {
        document.getElementById('ActionRequired').innerHTML = "Atleast one rule Required";
        event.preventDefault();
        $('#Act').click();
    }
}
