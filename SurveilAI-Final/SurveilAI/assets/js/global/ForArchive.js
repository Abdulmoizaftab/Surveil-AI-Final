
$(document).ready(function () {
    
    $("#PurgePolicy").change(function () {
        if (this.checked) {
            $('#ArchivePolicy').prop('checked', false);
        }
    });
    $("#ArchivePolicy").change(function () {
        if (this.checked) {
            $('#PurgePolicy').prop('checked', false);
        }
    });
    $("#BySize").change(function () {
        if (this.checked) {
            $('#ByDays').prop('checked', false);
            $('#on-days').hide();
            $('#on-size').show();
        }
    });
    $("#ByDays").change(function () {
        if (this.checked) {
            $('#BySize').prop('checked', false);
            $('#on-days').show();
            $('#on-size').hide();
        }
    });


    $('#test-btn').click(function () {
        $('#ArchiveModal').modal('show');
    });
    $('#bs4-table1').DataTable();
    //$('#bs4-table1').DataTable();
    $('#btn-status').click(function () {
        $('#btn-status').removeClass('btn-secondary');
        $('#btn-status').addClass('btn-primary');
        $('#btn-archive').removeClass('btn-primary');
        $('#btn-archive').addClass('btn-secondary');

        $('#archive-status-tbl').removeClass('d-none');
        $('#archive-status-tbl').addClass('d-block');

        $('#archive-tbl').removeClass('d-block');
        $('#archive-tbl').addClass('d-none');
    });

    $('#btn-archive').click(function () {
        $('#btn-archive').removeClass('btn-secondary');
        $('#btn-archive').addClass('btn-primary');
        $('#btn-status').removeClass('btn-primary');
        $('#btn-status').addClass('btn-secondary');

        $('#archive-tbl').removeClass('d-none');
        $('#archive-tbl').addClass('d-block');

        $('#archive-status-tbl').removeClass('d-block');
        $('#archive-status-tbl').addClass('d-none');
    });

    //debugger;
    $("#days").css("display", "none");
    $("#size").css("display", "none");
    $("#method").css("display", "none");

    $('input:radio[name="ArchivePolicy"]').change(function () {
        //debugger;
        var val = $('input[name="ArchivePolicy"]:checked').val();
        if (val == "True") {
            $("#days").css("display", "");
            $("#size").css("display", "none");
        }
        else {
            $("#size").css("display", "");
            $("#days").css("display", "none");
        }
    });

    $('input:radio[name="PurgePolicy"]').change(function () {
        $("#method").css("display", "");
    });




    $(".btn-edit-arc").click(function () {
        var Archive1 = $(this).closest('tr').children("td:eq(0)").text();
        var NoOfDays = $(this).closest('tr').children("td:eq(1)").text();
        var Size = $(this).closest('tr').children("td:eq(2)").text();
        var ArchivePolicy = $(this).closest('tr').children("td:eq(3)").children("input[type='checkbox']").is(":checked");
        var PurgePolicy = $(this).closest('tr').children("td:eq(4)").children("input[type='checkbox']").is(":checked");
        var ByDays = $(this).closest('tr').children("td:eq(5)").children("input[type='checkbox']").is(":checked");
        var BySize = $(this).closest('tr').children("td:eq(6)").children("input[type='checkbox']").is(":checked");
        var IsActive = $(this).closest('tr').children("td:eq(7)").children("input[type='checkbox']").is(":checked");
        $('#Archive1').val(Archive1);
        $('#NoOfDays').val(NoOfDays);
        $('#Size').val(Size);
        $('#PurgePolicy').prop('checked', PurgePolicy);
        $('#ArchivePolicy').prop('checked', ArchivePolicy);
        $('#ByDays').prop('checked', ByDays);
        $('#BySize').prop('checked', BySize);
        $('#IsActive').prop('checked', IsActive);
        if (ByDays) {
            $('#on-days').show();
            $('#on-size').hide();
        }
        else {
            $('#on-days').hide();
            $('#on-size').show();
        }

        $('#ArchiveModal').modal('show');
    });

    $('#update-archive').off('click').on('click', function () {
        var Archive1 = $('#Archive1').val();
        var NoOfDays = $('#NoOfDays').val();
        var Size = $('#Size').val();
        var PurgePolicy = $('#PurgePolicy').is(":checked");
        var ArchivePolicy = $('#ArchivePolicy').is(":checked");
        var ByDays = $('#ByDays').is(":checked");
        var BySize = $('#BySize').is(":checked");
        var IsActive = $('#IsActive').is(":checked");
        var submit = false;
        if (ByDays == true) {
            Size = "0";
            if (ByDays == true && NoOfDays < 1) {
                $('#no-of-days').text("Invalid input");
                $('#no-of-days').removeClass('d-none');
                $('#no-of-days').addClass('d-block');
                submit = false;
            }
            else if (NoOfDays > 9999) {
                $('#no-of-days').text("Up to 4 digit numbers allowed");
                $('#no-of-days').removeClass('d-none');
                $('#no-of-days').addClass('d-block');
                submit = false;
            }
            else {
                submit = true;
                $('#no-of-days').removeClass('d-block');
                $('#no-of-days').addClass('d-none');
            }
        }
        
        if (ByDays == false) {
            NoOfDays = "0";
            if (BySize == true && Size < 1) {
                $('#policy-size').text("Invalid input");
                $('#policy-size').removeClass('d-none');
                $('#policy-size').addClass('d-block');
                submit = false;

            }
            else if (Size > 9999) {
                $('#policy-size').text("Up to 4 digit numbers allowed");
                $('#policy-size').removeClass('d-none');
                $('#policy-size').addClass('d-block');
                submit = false;
            }
            else {
                submit = true;
                $('#policy-size').removeClass('d-block');
                $('#policy-size').addClass('d-none');
            }
        }
        if (submit) {
            var model = { Archive1: Archive1, NoOfDays: NoOfDays, Size: Size, PurgePolicy: PurgePolicy, ArchivePolicy: ArchivePolicy, IsActive: IsActive, ByDays: ByDays, BySize: BySize };
            var url = $(this).attr('data-url');
            $.ajax({
                type: 'post',
                //url: '/Customize/UpdateArchive',
                url: url,
                data: model,
                //contentType: 'application/json; charset=utf-8',
                //dataType: 'json',
                success: function (data) {
                    $("#archive-status-div").html(data);
                    $('#ArchiveModal').modal('hide');
                },
                error: function (error) {
                    alert('failed');
                }
            });
        }
    });

});
