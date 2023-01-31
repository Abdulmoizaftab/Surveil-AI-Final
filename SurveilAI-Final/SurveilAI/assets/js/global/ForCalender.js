$("#lunar-div").hide();
$("#event-name-msg").hide();
$("#event-type-msg").hide();
$("#event-sdate-msg").hide();
$("#event-smonth-msg").hide();
$("#event-ldate-msg").hide();
$("#event-lmonth-msg").hide();


//$("#adjust-msg").hide();
//$("#adjust-suc-msg").hide();

$("#event-uname-msg").hide();
$("#event-utype-msg").hide();
$("#event-usdate-msg").hide();
$("#event-usmonth-msg").hide();
$("#event-uldate-msg").hide();
$("#event-ulmonth-msg").hide();
$("#event-undate-msg").hide();

//Event name regex
$('#NewEventName').on('keypress', function (event) {
    var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('#NewEventName').bind('paste', function () {
    var elmId = $(this).attr('id');
    CheckSpecialChar(elmId);
});

$('#UEventName').on('keypress', function (event) {
    var regex = new RegExp("^[a-zA-Z0-9\\-\\s]+$");
    var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
    if (!regex.test(key)) {
        event.preventDefault();
        return false;
    }
});
$('#UEventName').bind('paste', function () {
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


//Create Event
$("#NewEventType").change(function () {

    var CalenderType = $("#NewEventType").val();
    if (CalenderType != "0") {
        if (CalenderType == "Lunar") {
            $("#solar-div").hide();
            $("#lunar-div").show();
        }
        else {
            $("#solar-div").show();
            $("#lunar-div").hide();
        }
    }
});

$("#Add-Cal-Event").click(function () {
    $('#EventOptModal').modal('show');
});

$("#CreateCalEvent").off().on('click', function (event)
{
    event.preventDefault();
    var url = $(this).attr('data-url');
    $('#loading').show();
    var EvName = $("#NewEventName").val();
    var EvType = $("#NewEventType").val();
    var SolarD = $("#SolarDate").val();
    var LunarD = $("#LunarDate").val();
    var SolarM = $("#SolarMonth").val();
    var LunarM = $("#LunarMonth").val();

    if (EvName == "") {
        $("#event-name-msg").show();
    }
    else {
        $("#event-name-msg").hide();
    }
    if (EvType == "0") {
        $("#event-type-msg").show();
    }
    else {
        $("#event-type-msg").hide();
    }
    if (EvType == "Solar") {
        if (SolarD == "0") {
            $("#event-sdate-msg").show();
        }
        else {
            $("#event-sdate-msg").hide();
        }
        if (SolarM == "0") {
            $("#event-smonth-msg").show();
        }
        else {
            $("#event-smonth-msg").hide();
        }
    }
    if (EvType == "Lunar") {
        if (LunarD == "0") {
            $("#event-ldate-msg").show();
        }
        else {
            $("#event-ldate-msg").hide();
        }
        if (LunarM == "0") {
            $("#event-lmonth-msg").show();
        }
        else {
            $("#event-lmonth-msg").hide();
        }
    }

    if (EvType != "0" && EvType == "Solar" && EvName != "" && SolarD != "0" && SolarM != "0") {
        $("#event-name-msg").hide();
        $("#event-type-msg").hide();
        $("#event-sdate-msg").hide();
        $("#event-smonth-msg").hide();
        var model = { Event_Name: EvName, EventDate: SolarD, EventMonth: SolarM, CalendarType: EvType };
        
        $.ajax({
            type: 'post',
            url: url,
            data: model,
            //contentType: 'application/json; charset=utf-8',
            //dataType: 'json',
            success: function (data) {
                $('#loading').hide();
                $("#event-list").html(data);
                $("#NewEventName").val("");
                $("#SolarDate").val("0");
                $("#SolarMonth").val("0");
                $('#EventOptModal').modal('toggle');
            },
            error: function (error) {
                $('#loading').hide();
                alert('failed');
            }
        })
    }
    if (EvType != "0" && EvType == "Lunar" && EvName != "" && LunarD != "0" && LunarM != "0") {
        $("#event-name-msg").hide();
        $("#event-type-msg").hide();
        $("#event-ldate-msg").hide();
        $("#event-lmonth-msg").hide();
        var model = { Event_Name: EvName, EventDate: LunarD, EventMonth: LunarM, CalendarType: EvType };
        $.ajax({
            type: 'post',
            url: url,
            data: model,
            //contentType: 'application/json; charset=utf-8',
            //dataType: 'json',
            success: function (data) {
                $("#event-list").html(data);
                $("#NewEventName").val("");
                $("#LunarDate").val("0");
                $("#LunarMonth").val("0");
                $("#NewEventType").val("1");
                $('#EventOptModal').modal('toggle');
            },
            error: function (error) {
                alert('failed');
            }
        })
    }
    $('#loading').hide();
});

//Edit Event 
$("#UEventType").change(function () {

    var CalenderType = $("#UEventType").val();
    if (CalenderType != "0") {
        if (CalenderType == "Lunar") {
            $("#solar-div-u").hide();
            $("#lunar-div-u").show();
        }
        else {
            $("#solar-div-u").show();
            $("#lunar-div-u").hide();
        }
    }
});

$(".btn-edit-cal").click(function () {
    var EventName = $(this).closest('tr').children("td:eq(0)").text();
    var EventDate = $(this).closest('tr').children("td:eq(1)").text();
    var EventType = $(this).closest('tr').children("td:eq(2)").find('span:first').children().val();
    var ThisYear = $(this).closest('tr').children("td:eq(2)").children("span").eq(1).text();
    var arr = EventDate.split(' ');
    var date = arr[0];
    var month = arr[1];

    if (EventType == "Lunar") {
        $("#UEventName").val(EventName);
        $("#old-event-name").val(EventName);
        $("#UEventType").val("Lunar");
        $("#ULunarDate").val(date);
        $("#ULunarMonth").val(month);
        $("#solar-div-u").hide();
        $("#lunar-div-u").show();
        var from = ThisYear.split("-")
        var fa = from[2] + "-" + from[0] + "-" + from[1];
        $('#dateselector').val(fa);
        $("#EventEditModal").modal('show');
    }
    else {
        $("#UEventName").val(EventName);
        $("#old-event-name").val(EventName);
        $("#UEventType").val("Solar");
        $("#USolarDate").val(date);
        $("#USolarMonth").val(month);
        $("#lunar-div-u").hide();
        $("#solar-div-u").show();
        var from = ThisYear.split("-")
        var fa = from[2] + "-" + from[0] + "-" + from[1];
        $('#dateselector').val(fa);
        $("#EventEditModal").modal('show');
        $("#EventEditModal").modal('show');
    }
});

$("#UdateCalEvent").off().on('click', function (event) { 
    event.preventDefault();
    var url = $(this).attr('data-url');
    $('#loading').show();
    var EvName = $("#UEventName").val();
    var OldEvName = $("#old-event-name").val();
    var EvType = $("#UEventType").val();
    var SolarD = $("#USolarDate").val();
    var LunarD = $("#ULunarDate").val();
    var SolarM = $("#USolarMonth").val();
    var LunarM = $("#ULunarMonth").val();
    var ThisYear = $("#dateselector").val();
    var str = ThisYear.split('-');
    var a = new Date().getFullYear()
    if (str[0] == a) {
        $("#event-undate-msg").hide();
    }
    else {

        $("#event-undate-msg").text("Please select valid date");
        $("#event-undate-msg").show();
    }
    
    if (ThisYear == "") {
        $("#event-undate-msg").val("required");
        $("#event-undate-msg").show();
    }

    if (EvName == "") {
        $("#event-uname-msg").show();
    }
    else {
        $("#event-uname-msg").hide();
    }
    if (EvType == "0") {
        $("#event-utype-msg").show();
    }
    else {
        $("#event-utype-msg").hide();
    }
    if (EvType == "Solar") {
        if (SolarD == "0") {
            $("#event-usdate-msg").show();
        }
        else {
            $("#event-usdate-msg").hide();
        }
        if (SolarM == "0") {
            $("#event-usmonth-msg").show();
        }
        else {
            $("#event-usmonth-msg").hide();
        }
    }
    if (EvType == "Lunar") {
        if (LunarD == "0") {
            $("#event-uldate-msg").show();
        }
        else {
            $("#event-uldate-msg").hide();
        }
        if (LunarM == "0") {
            $("#event-ulmonth-msg").show();
        }
        else {
            $("#event-ulmonth-msg").hide();
        }
    }

    if (EvType != "0" && EvType == "Solar" && EvName != "" && SolarD != "0" && SolarM != "0" && ThisYear != "" && str[0] == a) {
        $("#event-uname-msg").hide();
        $("#event-utype-msg").hide();
        $("#event-usdate-msg").hide();
        $("#event-usmonth-msg").hide();
        $("#event-undate-msg").hide();
        var model = { Event_Name: EvName, EventDate: SolarD, EventMonth: SolarM, CalendarType: EvType, Event_Date: ThisYear, OldEventName: OldEvName };
        $.ajax({
            type: 'post',
            url: url,
            data: model,
            //contentType: 'application/json; charset=utf-8',
            //dataType: 'json',
            success: function (data) {
                $("#event-list").html(data);
                $("#dateselector").val("");
                $("#NewEventName").val("");
                $("#SolarDate").val("0");
                $("#SolarMonth").val("0");
                $('#EventEditModal').modal('toggle');
            },
            error: function (error) {
                alert('failed');
            }
        })
    }
    if (EvType != "0" && EvType == "Lunar" && EvName != "" && LunarD != "0" && LunarM != "0" && ThisYear != "" && str[0] == a) {
        $("#event-uname-msg").hide();
        $("#event-utype-msg").hide();
        $("#event-uldate-msg").hide();
        $("#event-ulmonth-msg").hide();
        $("#event-undate-msg").hide();
        var model = { Event_Name: EvName, EventDate: LunarD, EventMonth: LunarM, CalendarType: EvType, Event_Date: ThisYear, OldEventName: OldEvName };
        $.ajax({
            type: 'post',
            url: url,
            data: model,
            //contentType: 'application/json; charset=utf-8',
            //dataType: 'json',
            success: function (data) {
                $("#event-list").html(data);
                $("#dateselector").val("");
                $("#NewEventName").val("");
                $("#LunarDate").val("0");
                $("#LunarMonth").val("0");
                $("#NewEventType").val("1");
                $('#EventEditModal').modal('toggle');
            },
            error: function (error) {
                alert('failed');
            }
        })
    }
    $('#loading').hide();
});


//Delete Events
$(".btn-del-cal").click(function () {
    var EventName = $(this).closest('tr').children("td:eq(0)").text();
    var EventDate = $(this).closest('tr').children("td:eq(1)").text();

    $("#d-event-name").text(EventName);
    $("#d-event-date").text(EventDate);
    $("#EventDelModal").modal('show');
    
});

$("#DelCalEvent").off().on('click', function (event) {
    event.preventDefault();
    var url = $(this).attr('data-url');
    var EvName = $("#d-event-name").text();
    var EvDate = $("#d-event-date").text();
   

    var model = { Event_Name: EvName, EventDate: EvDate };
    $.ajax({
        type: 'post',
        url: url,
        data: model,
        //contentType: 'application/json; charset=utf-8',
        //dataType: 'json',
        success: function (data) {
            $("#event-list").html(data);
            $('#EventDelModal').modal('toggle');
        },
        error: function (error) {
            alert('failed');
        }
    })
    


});



$('#btn-events').click(function () {
    $('#btn-events').removeClass('btn-secondary');
    $('#btn-events').addClass('btn-primary');
    $('#btn-calendar').removeClass('btn-primary');
    $('#btn-calendar').addClass('btn-secondary');
    $('#div-events').removeClass('d-none');
    $('#div-events').addClass('d-block');

    $('#div-calendar').removeClass('d-block');
    $('#div-calendar').addClass('d-none');
    $('#adjust-div').removeClass('d-block');
    $('#adjust-div').addClass('d-none');

    $('#add-event-div').removeClass('d-none');
    $('#add-event-div').addClass('d-block');

    $('#events-heading').removeClass('d-none');
    $('#events-heading').addClass('d-block');


    //$('#div-calender').hide();
    //$('#div-events').show();

});

$('#btn-calendar').off().on('click', function () {
    var url = $('#get-cal').attr('data-url');
    $('#btn-calendar').removeClass('btn-secondary');
    $('#btn-calendar').addClass('btn-primary');
    $('#btn-events').removeClass('btn-primary');
    $('#btn-events').addClass('btn-secondary');
    $('#div-calendar').removeClass('d-none');
    $('#div-calendar').addClass('d-block');

    $('#div-events').removeClass('d-block');
    $('#div-events').addClass('d-none');
    $('#adjust-div').removeClass('d-none');
    $('#adjust-div').addClass('d-block');
    $('#add-event-div').removeClass('d-block');
    $('#add-event-div').addClass('d-none');

    $('#events-heading').removeClass('d-block');
    $('#events-heading').addClass('d-none');

    var events = [];
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            $.each(data, function (i, v) {
                events.push({
                    title: v.Subject,
                    description: v.Description,
                    start: moment(v.Start),
                    end: v.End != null ? moment(v.End) : null,
                    color: v.ThemeColor,
                    allDay: v.IsFullDay
                });
            })
            var HijDate = data[0].HijriDates;
            GenerateCalender(events, HijDate);
        },
        error: function (error) {
            alert('failed');
        }
    })
});

$("#adjustment").on("keypress", function (event) {
    var charCode = (event.which) ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 50) ) {
        event.preventDefault();
    }

});

//$("#adjustment").on("keypress", function (event) {
//    var charCode = (event.which) ? event.which : event.keyCode;
//    if (charCode > 31 && (charCode < 48 || charCode > 57 || charCode > 50 || charCode > 51 || charCode > 52 || charCode > 53 || charCode > 54 || charCode > 55 || charCode > 56 || charCode > 57)) {
//        event.preventDefault();
//    }
   
//});
function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}
$('#btn-adjust').off().on('click', function () {
    var url = $('#adjust-cal').attr('data-url');
    var adj = $('#adjustment').val();
    if (adj >= -2 && adj <= 2 && adj != "") {
        //$("#adjust-msg").hide();
        $("#adjust-msg").removeClass('d-block');
        $("#adjust-msg").addClass('d-none');
        var events = [];
        $.ajax({
            type: "GET",
            url: url,
            data: { adjustment: adj },
            success: function (data) {
                //$("#adjust-suc-msg").show();
                $("#adjust-suc-msg").removeClass('d-none');
                $("#adjust-suc-msg").addClass('d-block');
                $.each(data, function (i, v) {
                    events.push({
                        title: v.Subject,
                        description: v.Description,
                        start: moment(v.Start),
                        end: v.End != null ? moment(v.End) : null,
                        color: v.ThemeColor,
                        allDay: v.IsFullDay
                    });
                })
                var HijDate = data[0].HijriDates;
                GenerateCalender(events, HijDate);
                
            },
            error: function (error) {
                //alert('failed');
            }
        })
    }
    else {
        //$("#adjust-suc-msg").hide();
        //$("#adjust-msg").show();
        $("#adjust-suc-msg").removeClass('d-block');
        $("#adjust-suc-msg").addClass('d-none');
        $("#adjust-msg").removeClass('d-none');
        $("#adjust-msg").addClass('d-block');
    }
});

var events = [];
var urlg = $('#get-cal').attr('data-url');
$.ajax({
    type: "GET",
    url: urlg,
    success: function (data) {
        $.each(data, function (i, v) {
            events.push({
                title: v.Subject,
                description: v.Description,
                start: moment(v.Start),
                end: v.End != null ? moment(v.End) : null,
                color: v.ThemeColor,
                allDay: v.IsFullDay
            });
        })
        var HijDate = data[0].HijriDates;
        GenerateCalender(events, HijDate);
    },
    error: function (error) {
        alert('failed');
    }
})

function GenerateCalender(events, HijDate) {
    $('#calender').fullCalendar('destroy');
    $('#calender').fullCalendar({
        contentHeight: 600,
        displayEventTime: false,
        defaultDate: new Date(),
        timeFormat: 'h(:mm)a',
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,basicWeek,basicDay'
        },
        eventLimit: true,
        dayRender: function (date, element) {
            var ddate = moment(date).format('yyyy-MM-DD');
            $.each(HijDate, function (i, v) {
                var dddate = moment(v.SolarDate).format('yyyy-MM-DD');
                if (ddate == dddate) {
                    element.append('<br/><span style="color:#4CA1C3;">' + v.LunarDate + '</span>');
                }
            })


            //var ddate = date.toDate().toDateString();
            //alert(HijDate[0].LunarDate);
            //var ddate = moment(date).format('yyyy-MM-DD');
            ////2021-04-28
            //if (ddate == '2021-04-28') {
            //    var a = moment('1410/8/28', 'iYYYY/iM/iD');
            //    //var a = moment('2014-11-28 16:40:00', 'YYYY-M-D HH:mm:ss').endOf('iMonth').format('iYYYY/iM/iD HH:mm:ss');
            //    element.append(a);
            //}

        },
        eventColor: '#ffffff',
        events: events,
        eventClick: function (calEvent, jsEvent, view) {
            var EventDate = calEvent.start.toString();
            EventDate = EventDate.split('00:00:00')[0];
            $('#EventModalLabel').text(calEvent.title);
            $('#event-date').text(EventDate);
            $('#event-desc').text(calEvent.description);
            $('#EventModal').modal('show');

        }
    })
}