let  timer;
let bTimer = false;
let id = -1;
let operation_url;

let send_data;
let data_sended;
let operation_complete;
let image_opened;

let imageWidth = 0;
let imageHeight = 0;



$(document).ready(function () {
    $("#btn_send").click(SendImage);
});


function showpreview(input) {

    if (input.files && input.files[0]) {

        let reader = new FileReader();

        let promise = new Promise((res, rej) => {
            reader.onload = function (e) {
                $('#imgpreview').css('visibility', 'visible');
                $('#imgpreview').attr('src', e.target.result);
                let img = new Image();

                let promise = new Promise((resolve, reject) => {
                    img.onload = () => {
                        $('#img_h')[0].innerHTML = img.height;
                        $('#img_w')[0].innerHTML = img.width;
                        imageWidth = img.width;
                        imageHeight = img.height;
                        resolve();
                    }
                    img.onerror = reject;
                    img.src = e.target.result;
                });

                promise.then( () => res() );

                
            };
            reader.onerror = rej;
            reader.readAsDataURL(input.files[0]);

        });

        promise.then(() => { image_opened() });


    }
}

function SendImage() {
    clearTimeout(timer);
    var bTimer = false;
    id = -1;

    document.getElementById("btn_send").disabled = true;

    $("#progress").text("Загрузка...");
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
        document.getElementById("btn_send").disabled = false;
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
        document.getElementById("btn_send").disabled = false;
        clearTimeout(timer);
        bTimer = false;
        $("#opTime").text("Ошибка обработки");
    }
    
}

function progressTime(data) {
    $("#opTime").text("Completion time " + data + "ms.");
}