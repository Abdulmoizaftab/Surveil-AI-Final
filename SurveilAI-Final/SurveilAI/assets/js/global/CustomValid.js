function emailIsValid(email) {
    var pattern = /^(([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/i
    return pattern.test(email);
}
//Email validation and show msg
$('.valid-email').keyup(function () {
    var email = $(this).val();

    var vaild_elm_id = $(this).data("emid");
    vaild_elm_id = "#" + vaild_elm_id;
    $(vaild_elm_id).hide();
    $(vaild_elm_id).text("Invalid Email");
    var IsValid = true;
    var lst = email.split(";");
    for (var i = 0; i < lst.length; i++) {
        if (lst[i] != "") {
            var rslt = emailIsValid(lst[i]);
            if (rslt == false) {
                IsValid = false;
            }
        }
    }
    if (!IsValid) {
        $(vaild_elm_id).show();
    }
    else {
        $(vaild_elm_id).hide();
    }
});

function ValidateMobile(mobileNum) {
    var validateMobNum = /^\d*(?:\.\d{1,2})?$/;
    if (validateMobNum.test(mobileNum) && mobileNum.length == 11) {
        return true;
    }
    else {
        return false;
    }
}
//Mobile validation and show msg
$('.valid-mobile').keyup(function () {
    var phone = $(this).val();
    var vaild_elm_id = $(this).data("emid");
    vaild_elm_id = "#" + vaild_elm_id;
    var IsValid = true;
    var lst = phone.split(";");
    for (var i = 0; i < lst.length; i++) {
        if (lst[i] != "") {
            var rslt = ValidateMobile(lst[i]);
            if (rslt == false) {
                IsValid = false;
            }
        }
    }
    if (!IsValid) {
        $(vaild_elm_id).show();
    }
    else {
        $(vaild_elm_id).hide();
    }
});

function ValidateMobileNums(phone) {
    var IsValid = true;
    var lst = phone.split(";");
    for (var i = 0; i < lst.length; i++) {
        if (lst[i] != "") {
            var rslt = ValidateMobile(lst[i]);
            if (rslt == false) {
                IsValid = false;
            }
        }
    }
    return IsValid;
}
function ValidateEmails(email) {
    var IsValid = true;
    var lst = email.split(";");
    for (var i = 0; i < lst.length; i++) {
        if (lst[i] != "") {
            var rslt = emailIsValid(lst[i]);
            if (rslt == false) {
                IsValid = false;
            }
        }
    }
    return IsValid;
}


//Restrict special characters enter and paste

$('.BlockSpecial').on('keypress', function (event) {
    $('#war-msg').hide();
    var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        $('#war-msg').show();
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecial').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialChars(elmId);
});

function CheckSpecialChars(id) {
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


//Gernal restrict hml tags
$('.BlockSpecialU').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    var regex = new RegExp("^[,\\\-:\\\\._a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialU').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsU(elmId);
});
function CheckSpecialCharsU(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        var dataFull = data.replace(/[^\w\s]/gi, '');

        dataFull = dataFull.trim();
        //set the new value of the input text without special characters
        elmId.val(dataFull);
    });
}


//block chars for email

$('.BlockForEmail').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    var regex = new RegExp("^[\\\-@;._a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});

function CheckSpecialEml(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        var dataFull = data.replace(new RegExp('<', 'g'), "");
        var dataFull = dataFull.replace(new RegExp('>', 'g'), "");
        var dataFull = dataFull.replace(new RegExp('/', 'g'), "");

        dataFull = dataFull.trim();
        //set the new value of the input text without special characters
        elmId.val(dataFull);
    });
}


//Remove special chara and space
function CheckSpecialCharsWithSpace(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters
        elmId.val(dataFull);
    });
}






//Validation of UserName and Password (Block < and >)
$('.BlockSpecialP').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    var regex = new RegExp("^[@#$%^&*!,:\\\\._a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialP').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsPass(elmId);
});
function CheckSpecialCharsPass(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        var dataFull = data.replace(/[^\w\s]/gi, '');

        dataFull = dataFull.trim();
        dataFull = dataFull.replaceAll(/\s/g, '')
        //set the new value of the input text without special characters
        elmId.val(dataFull);
    });
}










function activaTab(tab) {
    $('.nav-tabs a[href="#' + tab + '"]').tab('show');
};

function GetDateTimeUs() {
    var dt = new Date();
    var hr = dt.getHours();
    var min = dt.getMinutes();
    var day = dt.getUTCDate()
    var month = dt.getMonth() + 1;
    var year = dt.getUTCFullYear();
}

//function checkWhitespace(event)
//{
//	var data = event.clipboardData.getData("text/plain");
//    var isNullOrContainsWhitespace = (!data || data.length === 0 || /\s/g.test(data));

//    if(isNullOrContainsWhitespace)
//    {
//  	  event.preventDefault(); 
//    }

//}




//For Time

$('.BlockSpecialTime').on('keypress', function (event) {
    $('#war-msg').hide();
    var regex = new RegExp("^[0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        $('#war-msg').show();
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialTime').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialChars(elmId);
});









//Block html tags and spaces-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialL').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[@#$%^&*!,\\\-:\\\\._a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialL').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsL(elmId);
});

$('.BlockSpecialL').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsL(elmId);
});

//Remove special chara and space
function CheckSpecialCharsL(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^@#$%^&*!\\\-,:\\\\._a-zA-Z0-9]/g, '');
        var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Block html tags and spaces-------------End-------------------











//Block html tags special and spaces-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialAll').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[,\\\-:\\\\._a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialAll').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsAll(elmId);
});

$('.BlockSpecialAll').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsAll(elmId);
});

//Remove special chara and space
function CheckSpecialCharsAll(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._a-zA-Z0-9]/g, '');
        dataFull = dataFull.replace(/\s/g, '');
        if (dataFull == "." || dataFull == "_" || dataFull == "-") { dataFull = "";}

        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Block html tags special and spaces-------------End-------------------




//Only numeric-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.OnlyNumeric').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[:0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.OnlyNumeric').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialOnlyNumeric(elmId);
});

$('.OnlyNumeric').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialOnlyNumeric(elmId);
});

//Remove special chara and space
function CheckSpecialOnlyNumeric(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^:0-9]/g, '');
        var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Only numeric-------------End-------------------





//Block html tags and spaces, special char-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialAl').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[\\\-,:\\\\._a-zA-Z0-9\/\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialAl').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsA(elmId);
});

$('.BlockSpecialAl').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsA(elmId);
});

//Remove special chara and space
function CheckSpecialCharsA(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._a-zA-Z0-9\/\s]/g, '');
        //var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Block html tags and spaces-------------End-------------------








//Block html tags and spaces, special char-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialIP').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[.0-9]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialIP').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsA(elmId);
});

$('.BlockSpecialIP').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsIP(elmId);
});

//Remove special chara and space
function CheckSpecialCharsIP(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._0-9]/g, '');
        var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Block html tags and spaces-------------End-------------------



//Block html tags and space on start-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialSS').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[\\\-:\\\\._a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
    else if ($(this).val().length === 0 && event.which === 32) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialSS').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsSS(elmId);
});

$('.BlockSpecialSS').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsSS(elmId);
});

//Remove special chara and space
function CheckSpecialCharsSS(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._a-zA-Z0-9\-\s]/g, '');
        //var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        if (dataFull.trim() == "") { dataFull = dataFull.trim(); }
        //if (dataFull == "." || dataFull == "_" || dataFull == "-" || dataFull == "\\") { dataFull = ""; }
        elmId.val(dataFull);
    });
}
//Block html tags and space at start-------------End-------------------




//Block html tags and space on start-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialMs').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[\\\-:,\\\\._a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
    else if ($(this).val().length === 0 && event.which === 32) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialMs').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsMs(elmId);
});

$('.BlockSpecialMs').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsMs(elmId);
});

//Remove special chara and space
function CheckSpecialCharsMs(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._a-zA-Z0-9\-\s]/g, '');
        //var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        if (dataFull.trim() == "") { dataFull = dataFull.trim(); }
        elmId.val(dataFull);
    });
}
//Block html tags and space at start-------------End-------------------






//Block html tags special and spaces-------------Start-------BIOS Password------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialBios').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[a-zA-Z0-9s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialBios').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsBios(elmId);
});

$('.BlockSpecialBios').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsBios(elmId);
});

//Remove special chara and space
function CheckSpecialCharsBios(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^a-zA-Z0-9]/g, '');
        var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        elmId.val(dataFull);
    });
}
//Block html tags special and spaces-------------End-------------------


$('.valsss').click(function () {
    alert("ss");
});





//Block html tags and space on start alllow $-------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialSSD').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[\\\-:$\\\\._a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
    else if ($(this).val().length === 0 && event.which === 32) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialSSD').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsSSD(elmId);
});

$('.BlockSpecialSSD').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsSSD(elmId);
});

//Remove special chara and space
function CheckSpecialCharsSSD(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:$\\\\._a-zA-Z0-9\-\s]/g, '');
        //var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        if (dataFull.trim() == "") { dataFull = dataFull.trim(); }
        elmId.val(dataFull);
    });
}
//Block html tags and space at start-------------End-------------------




//Block html tags and space on start  .-\ at start -------------Start-------------------

//Validation of UserName and Password (Block < and >)
$('.BlockSpecialSSD').on('keypress', function (event) {

    //var regex = new RegExp("^[a-zA-Z0-9!@#$%^&*.,_-;:-s]+$");
    //var regex = new RegExp("^[a-zA-Z0-9,.;:_\\-\\s]+$");
    var regex = new RegExp("^[\\\-:\\\\._a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
    else if ($(this).val().length === 0 && event.which === 32) {
        event.preventDefault();
        return false;
    }
});
$('.BlockSpecialSSD').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialCharsSSD(elmId);
});

$('.BlockSpecialSSD').on('blur input', function () {
    var elmId = $(this).attr('id');

    CheckSpecialCharsSSD(elmId);
});

//Remove special chara and space
function CheckSpecialCharsSSD(id) {
    setTimeout(function () {
        var elmId = $('#' + id);
        //get the value of the input text
        var data = elmId.val();
        //replace the special characters to '' 
        //var dataFull = data.replace(/[^\w\s]/gi, '');
        var dataFull = data.replace(/[^\\\-,:\\\\._a-zA-Z0-9\-\s]/g, '');
        //var dataFull = dataFull.replace(/\s/g, '');
        //set the new value of the input text without special characters    ^[a-zA-Z0-9,.;:_\\-\\s]+$
        if (dataFull.trim() == "") { dataFull = dataFull.trim(); }
        if (dataFull.charAt(0) == "." || dataFull.charAt(0) == "_" || dataFull.charAt(0) == "-" || dataFull.charAt(0) == "\\") { dataFull = ""; }
        elmId.val(dataFull);
    });
}
//Block html tags and space at start-  .-\ at start------------End-------------------