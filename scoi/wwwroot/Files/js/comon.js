var timer;
var bTimer = false;
var id = -1;
var operation_url;

var send_data;
var data_sended;
var operation_complete;

$(document).ready(function () {
    $("#btn_send").click(SendImage);
});



function showpreview(input) {

    if (input.files && input.files[0]) {

        let reader = new FileReader();
        reader.onload = function (e) {
            $('#imgpreview').css('visibility', 'visible');
            $('#imgpreview').attr('src', e.target.result);
            let img = new Image();
            img.onload = () => {
                $('#img_h')[0].innerHTML = img.height;
                $('#img_w')[0].innerHTML = img.width;
            }
            img.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

function SendImage() {
    clearTimeout(timer);
    var bTimer = false;
    id = -1;

    $.ajax({
        url: operation_url,
        type: "post",
        //cache: false,
        data: send_data(),
        success: SucsessUpload,
        processData: false,
        contentType: false
    });
}


function SucsessUpload(data) {

    //alert(data);
    
    let string_arr = data.split(":");
    id = string_arr[0];
    let ns = data.slice(id.length + 1);
    data_sended(ns);

    bTimer = true;
    timer = setTimeout(function run() {
        onTimerProgress();
        if (bTimer)
            timer = setTimeout(run, 300);
    }, 1000);
}

function onTimerProgress() {
    console.log("send ajax, " + id);
    $.ajax({
        url: "/Main/getOpStatus",
        type: "post",
        data: "id=" + id,
        success: progressMonitor,
        async: false
    });
}

function progressMonitor(data) {
    $("#progress").text(data);
    if (data >= 100) {
        clearTimeout(timer);
        bTimer = false;
        $.ajax({
            url: "/Main/getOpTime",
            type: "post",
            data: "id=" + id,
            success: progressTime,
            //async: false
        });
        operation_complete();
    } else if (data == -1) {
        clearTimeout(timer);
        bTimer = false;
        $("#opTime").text("Ошибка обработки");
    }
    
}

function progressTime(data) {
    $("#opTime").text("Completion time " + data + "ms.");
}