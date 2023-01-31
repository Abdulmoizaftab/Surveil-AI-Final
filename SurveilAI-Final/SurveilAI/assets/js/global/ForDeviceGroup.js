$(document).ready(function () {
    $('#gptest').click(function () {
        alert("yes");
    });
    $("#EmCon1").css("pointer-events", "none");
    $("#EmCon2").css("pointer-events", "none");
    $("#EmCon3").css("pointer-events", "none");
    $("#AnCon1").css("pointer-events", "none");
    $("#AnCon2").css("pointer-events", "none");
    $("#AnCon3").css("pointer-events", "none");
    $('#groupName').on('keypress', function (event) {
        var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
        var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
        if (!regex.test(key)) {
            event.preventDefault();
            return false;
        }
    });
    $('#description').on('keypress', function (event) {
        var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
        var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
        if (!regex.test(key)) {
            event.preventDefault();
            return false;
        }
    });

    
    $(".contacts").on('change', function () {
        
        var userinput = $(this).val();
        var result = emailIsValid(userinput);
        if (result) {
            $(this).next().css("display", "none");
            $(this).next().text("");
        }
        else {
            $(this).next().css("display", "block");
            var text = userinput;
            if (text == "") {
                $(this).next().text("");
                $(this).next().css("display", "none");
            }
            else {
                $(this).next().text("Incorrect Syntax");
            }
        }
        
    });
    function emailIsValid(email) {

        //var pattern = /^\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b$/i
        var pattern = /^(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/i
        return pattern.test(email);
    }
    function charLimitValid(email) {
        var CharsRegEx = /^.{1,18}$/;
        return CharsRegEx.test(email);
    }
    
    //if ($('#Profile1').is(":checked")) {
    //    alert("asd");
    //    $(this).siblings("checkbox").prop('checked', true);
    //}
    $('#groupName').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#description').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#contactid1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#contactid2').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#contactid3').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Profile1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Location1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Street1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#City1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Country1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Longitude1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Latitude1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Vendor1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#DevType1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Orgnization1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Contact1').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Contact2').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    $('#Contact3').bind('paste', function () {
        var elmId = $(this).attr('id');
        CheckSpecialChar(elmId);
    });
    function CheckSpecialChar(id) {
        setTimeout(function () {
            var elmId = $('#' + id);
            //get the value of the input text
            var data = elmId.val();
            //replace the special characters to '' 
            var dataFull = data.replace(/[^\w\s]/gi, '');
            //set the new value of the input text without special characters
            elmId.val(dataFull);
        });
    }
      
    //$(".criterias").bind('paste', function () {
    //    setTimeout(function () {
    //        alert("asdasd");
    //        //get the value of the input text
    //        var data = $('.criterias').val();
    //        //replace the special characters to '' 
    //        var dataFull = data.replace(/[^\w\s]/gi, '');
    //        //set the new value of the input text without special characters
    //        $('.criterias').val(dataFull);
    //    });

    //});
    $('.criterias').on('keypress', function (e) {
        var blockSpecialRegex = /[~`!#$%^&()={}[\]<>+\/?]/;
        var key = String.fromCharCode(!e.charCode ? e.which : e.charCode);
        console.log(key)
        if (blockSpecialRegex.test(key)) {
            e.preventDefault();
            return false;
        }
    });
    //$('.criterias').on('keypress', function (event) {
    //    //var regex = new RegExp("[^<>]*");
    //    var regex = /^[^<>,<|>]+$/i;
    //    var key = $(this).val();
    //    //alert(key);
    //    if (!regex.test(key)) {
    //        event.preventDefault();
    //        return false;
    //    }
    //});

    function validateNumber(number) {
        var pattern = /[0-9]{10}/i
        return pattern.test(number);
    }
    function validateEmail(email) {
        var pattern = /^\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b$/i
        return pattern.test(email);
    }
    $(".emails").change(function () {
        
        var userInput = $(this).val();
        if (validateEmail(userInput)) {
            $(this).next().css("display", "none");
            $(this).next().text("");
        }
        else {
            $(this).next().css("display", "block");
            if (userInput == "") {
                $(this).next().text("");
                $(this).next().css("display", "none");
            }
            else {
                $(this).next().text("Incorrect Syntax");
            }
        }
    });
    $("#target").submit(function (event) {
        var groupName = $("#groupName").val();
        var str = groupName.replace(/\s*$/, "");
        var c1 = $('#contactid1').val();
        var c2 = $('#contactid2').val();
        var c3 = $('#contactid3').val();
        var Con1 = $('#Contact1').val();
        var Con2 = $('#Contact2').val();
        var Con3 = $('#Contact3').val();
        if (c1 != "") {
            var reslt = emailIsValid(c1);
            if (!reslt) {
                //alert("C1");
                event.preventDefault();
                $("#RD").click();
            }
        }
        if (c2 != "") {
            var reslt = emailIsValid(c2);
            if (!reslt) {
                //alert("C2");
                event.preventDefault();
                $("#RD").click();
            }
        } 
        if (c3 != "") {
            var reslt = emailIsValid(c3);
            if (!reslt) {
                //alert("C3");
                event.preventDefault();
                $("#RD").click();
            }
        }
        //if (Con1 != "") {
        //    var reslt = validateEmail(Con1);
        //    if (!reslt) {
        //        alert("Con1");
        //        event.preventDefault();
        //    }
        //} 
        //if (Con2 != "") {
        //    var reslt = validateEmail(Con2);
        //    if (!reslt) {
        //        alert("Con2");
        //        event.preventDefault();
        //    }
        //} 
        //if (Con3 != "") {
        //    var reslt = validateEmail(Con3);
        //    if (!reslt) {
        //        alert("Con3");
        //        event.preventDefault();
        //    }
        //} 
        if (str == "") {
            //alert("str");
            event.preventDefault();
            $("#gname").css("display", "block");
            $("#RD").click();
        } 
        else {
            $("#gname").css("display", "none");
        } 
    });

    $("#target1").submit(function (event) {
        var groupName = $("#groupName").val();
        var str = groupName.replace(/\s*$/, "");
        var c1 = $('#contactid1').val();
        var c2 = $('#contactid2').val();
        var c3 = $('#contactid3').val();
        var Con1 = $('#Contact1 option:selected').text();
        var Con2 = $('#Contact2 option:selected').text();
        var Con3 = $('#Contact3 option:selected').text();
        if (c1 != "") {
            var reslt = emailIsValid(c1);
            if (!reslt) {
                event.preventDefault();
                $("#RD").click();
            }
        }
        if (c2 != "") {
            var reslt = emailIsValid(c2);
            if (!reslt) {
                event.preventDefault();
                $("#RD").click();
            }
        }
        if (c3 != "") {
            var reslt = emailIsValid(c3);
            if (!reslt) {
                event.preventDefault();
                $("#RD").click();
            }
        }
        if (Con1 != "") {
            var reslt = validateEmail(Con1);
            if (!reslt) {
                event.preventDefault();
            }
        }
        if (Con2 != "") {
            var reslt = validateEmail(Con2);
            if (!reslt) {
                event.preventDefault();
            }
        }
        if (Con3 != "") {
            var reslt = validateEmail(Con3);
            if (!reslt) {
                event.preventDefault();
            }
        }
        if (str == "") {
            event.preventDefault();
            $("#gname").css("display", "block");
            $("#RD").click();
        }
        else {
            $("#gname").css("display", "none");
        }
    });
});
function SwitchInput(inputId) {
    if (inputId == "EmCon3") {
        $("#AnCon3").css("pointer-events", "none");
        $("#AnCon3").css("opacity", "0.4");
        $("#AnCon3").children().eq(1).prop('selectedIndex', 0);
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
    else if (inputId == "AnCon3") {
        $("#EmCon3").children().eq(1).val("");
        $("#EmCon3").css("pointer-events", "none");
        $("#EmCon3").css("opacity", "0.4");
        $("#EmCon3 :nth-child(3)").css("display", "none");
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
    else if (inputId == "EmCon2") {
        $("#AnCon2").css("pointer-events", "none");
        $("#AnCon2").css("opacity", "0.4");
        $("#AnCon2").children().eq(1).prop('selectedIndex', 0);
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
    else if (inputId == "AnCon2") {
        $("#EmCon2").children().eq(1).val("");
        $("#EmCon2").css("pointer-events", "none");
        $("#EmCon2").css("opacity", "0.4");
        $("#EmCon2 :nth-child(3)").css("display", "none");
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
    else if (inputId == "EmCon1") {
        $("#AnCon1").css("pointer-events", "none");
        $("#AnCon1").css("opacity", "0.4");
        $("#AnCon1").children().eq(1).prop('selectedIndex', 0);
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
    else if (inputId == "AnCon1") {
        $("#EmCon1").children().eq(1).val("");
        $("#EmCon1").css("pointer-events", "none");
        $("#EmCon1").css("opacity", "0.4");
        $("#EmCon1 :nth-child(3)").css("display", "none");
        $('#' + inputId).css("pointer-events", "");
        $('#' + inputId).css("opacity", "");
    }
}
