//show on page load
$(document).ready(function () {
    showLoginDialog();
});

//right click context menu functions
function showRClickContextMenu(top, left) {
    var menu = document.getElementById('r_click_context_menu');
    menu.style.display = 'block';
    menu.style.top = (top - 10) + 'px';
    menu.style.left = (left - 10) + 'px';
}

$('#r_click_context_menu').mouseleave(function () {
    $(this).css('display', 'none');
});

$('#r_click_context_menu button').click(function () {
    $('#r_click_context_menu').css('display', 'none');
});

$('#ctx_button_copy').click(function () {
    whiteboard_pages[current_page].copyObject();
});

$('#ctx_button_paste').click(function () {
    whiteboard_pages[current_page].pasteObject();
});

$('#ctx_button_bring_forward').click(function () {
    var selected_object = whiteboard_pages[current_page].getSelectedObject();
    if (selected_object != null) {
        draw_client.sendBringObjectForward(selected_object);
    }
});

$('#ctx_button_send_backwards').click(function () {
    var selected_object = whiteboard_pages[current_page].getSelectedObject();
    if (selected_object != null) {
        draw_client.sendSendObjectBackwards(selected_object);
    }
});

$('#ctx_button_remove').click(function () {
    var selected_object = whiteboard_pages[current_page].getSelectedObject();
    if (selected_object != null) {
        whiteboard_pages[current_page].removeSelectedObject();
        draw_client.sendObjectRemove(selected_object);
    }
});

$('#ctx_button_group').click(function () {
    var selected_object = whiteboard_pages[current_page].getSelectedObject();
    if (selected_object != null) {
        draw_client.sendObjectGroup(selected_object);
        whiteboard_pages[current_page].groupObjects(selected_object);
    }
});

//color picker hint box functions
$('#whiteboards_container').mousemove(function (e) {
    if (whiteboard_pages[current_page].drawing_modes.isColorPickerMode) {
        showColorPickerHintBox(e);
    } else {
        $('#color_picker_hint_box').css('display', 'none');
    }
});

$('#whiteboards_container').mouseleave(function () {
    $('#color_picker_hint_box').css('display', 'none');
});

function showColorPickerHintBox(e) {
    var color = whiteboard_pages[current_page].getColorAtPointer();
    var menu = document.getElementById('color_picker_hint_box');
    menu.style.display = 'block';
    menu.style.top = (e.pageY - 30) + 'px';
    menu.style.left = (e.pageX + 30) + 'px';

    var color_rgba = '(' + color.r + ', ' + color.g + ', ' + color.b + ', ' + color.a + ')';

    $('#cpb_color_value').text(color_rgba);
    $('#cpb_color_show').css('background-color', 'rgba' + color_rgba);
}

//detect disconnect
window.addEventListener('offline', () => showReconnectDialog());

function showReconnectDialog() {
    toggleShadowOverlay();
    $('#whiteboard').css('display', 'none');
    $('#dialog_login').css('display', 'none');
    $('#dialog_whiteboards').css('display', 'none');
    $('#dialog_new').css('display', 'none');
    $('#dialog_reconnect').css('display', 'block');
}

$('#btn_reconnect').click(function () {
    window.location.reload();
});



//shadow overlay toggle
function toggleShadowOverlay() {
    var overlay = document.getElementById("shadow_overlay");
    if (overlay.style.display == "block") {
        overlay.style.display = "none";
    } else {
        overlay.style.display = "block";
    }
}

//loader animation toggle
function toggleLoader() {
    var loader = document.getElementById("loader");
    if (loader.style.display == "block") {
        loader.style.display = "none";
    } else {
        loader.style.display = "block";
    }
}

function randomSadFace() {
    var sad_faces = ['(ಥ﹏ಥ)', 'o(TヘTo)', '(｡•́︿•̀｡)', '(っ˘̩╭╮˘̩)っ'];
    var sad_face = sad_faces[Math.floor(Math.random() * sad_faces.length)];
    return sad_face;
}

//diable-enable login/register/whiteboard sections of dialog window
function disableRegisterSection() {
    $('#register_section :button').prop('disabled', true);
    $('#register_section :input').css('opacity', '0.3');
}

function enableRegisterSectionWithError(result) {
    $('#register_section :input').css('opacity', '1');
    $('#registration_result').text(result);
    $('#register_section :button').prop('disabled', false);
}

function enableRegisterSectionWithSuccess() {
    $('#register_section').css('display', 'none');
    $('#register_section :button').prop('disabled', false);
    $('#register_section :input').css('opacity', '1');
    $('#register_section :input').val('');
    $('#registration_result').text('');
    $('#dialog_login > .success_message').css('display', 'block');

    setTimeout(function () {
        $('#dialog_login > .success_message').css('display', 'none');
        $(".dialog_profile_back_button").trigger("click");
    }, 2000);
}

function disableLoginSection() {
    $('#login_section :button').prop('disabled', true);
    $('#login_section :input').css('opacity', '0.3');
}

function enableLoginSectionWithError(result) {
    $('#login_section :button').prop('disabled', false);
    $('#login_section :input').css('opacity', '1');
    $('#login_result').text(result);
}

function enableLoginSectionWithSuccess() {
    $('#login_section').css('display', 'none');
    $('#login_section :button').prop('disabled', false);
    $('#login_section :input').css('opacity', '1');
    $('#login_section :input').val('');
    $('#login_result').text('');
    $('#dialog_login > .success_message').css('display', 'block');

    setTimeout(function () {
        toggleShadowOverlay();
        $('#dialog_login > .success_message').css('display', 'none');
        $('#dialog_login').css('display', 'none');
    }, 1500);
}

function fillWhiteboardList(result) {
    $('#whiteboard_slot_1 .slot_info .whiteboard_name').text(result.whiteboard1_name);
    $('#whiteboard_slot_2 .slot_info .whiteboard_name').text(result.whiteboard2_name);
    $('#whiteboard_slot_3 .slot_info .whiteboard_name').text(result.whiteboard3_name);
    $('#whiteboard_slot_4 .slot_info .whiteboard_name').text(result.whiteboard4_name);
    $('#whiteboard_slot_5 .slot_info .whiteboard_name').text(result.whiteboard5_name);
    if (result.whiteboard1_date != '0001-01-01T00:00:00') {
        $('#whiteboard_slot_1 .slot_info .whiteboard_date').text(moment(result.whiteboard1_date).format('DD.MM.yyyy HH:mm:ss'));
    } else {
        $('#whiteboard_slot_1 .slot_info .whiteboard_date').text('-');
    }
    if (result.whiteboard2_date != '0001-01-01T00:00:00') {
        $('#whiteboard_slot_2 .slot_info .whiteboard_date').text(moment(result.whiteboard2_date).format('DD.MM.yyyy HH:mm:ss'));
    } else {
        $('#whiteboard_slot_2 .slot_info .whiteboard_date').text('-');
    }
    if (result.whiteboard3_date != '0001-01-01T00:00:00') {
        $('#whiteboard_slot_3 .slot_info .whiteboard_date').text(moment(result.whiteboard3_date).format('DD.MM.yyyy HH:mm:ss'));
    } else {
        $('#whiteboard_slot_3 .slot_info .whiteboard_date').text('-');
    }
    if (result.whiteboard3_date != '0001-01-01T00:00:00') {
        $('#whiteboard_slot_4 .slot_info .whiteboard_date').text(moment(result.whiteboard3_date).format('DD.MM.yyyy HH:mm:ss'));
    } else {
        $('#whiteboard_slot_4 .slot_info .whiteboard_date').text('-');
    }
    if (result.whiteboard3_date != '0001-01-01T00:00:00') {
        $('#whiteboard_slot_5 .slot_info .whiteboard_date').text(moment(result.whiteboard3_date).format('DD.MM.yyyy HH:mm:ss'));
    } else {
        $('#whiteboard_slot_5 .slot_info .whiteboard_date').text('-');
    }
}

function blockWhiteboardSection() {
    $('#blocked_section').css('display', 'block');
    $('#sadface').text(randomSadFace());
}

function unblockWhiteboardSection() {
    $('#blocked_section').css('display', 'none');
}

function disableWhiteboardSection() {
    $('#whiteboard_section :button').prop('disabled', true);
    $('#whiteboard_section :input').prop('disabled', true);
    $('#whiteboard_section > div').css('opacity', '0.3');
}

function enableWhiteboardSection() {
    $('#whiteboard_section :button').prop('disabled', false);
    $('#whiteboard_section :input').prop('disabled', false);
    $('#whiteboard_section > div').css('opacity', '1');
}

function disableWhiteboardSectionWithError(result) {
    disableWhiteboardSection();
    $('#whiteboard_result').text(result);
}

function enableWhiteboardSectionWithError(result) {
    enableWhiteboardSection();
    $('#whiteboard_result').text(result);
}

function enableWhiteboardSectionWithSuccessSave(slot, whiteboard_name) {
    var whiteboard_name_element = '#' + slot + ' .slot_info .whiteboard_name';
    var whiteboard_date_element = '#' + slot + ' .slot_info .whiteboard_date';
    var datetime = moment().format('DD.MM.yyyy HH:mm:ss');

    $(whiteboard_name_element).text(whiteboard_name);
    $(whiteboard_date_element).text(datetime);
    enableWhiteboardSection();
    $('#whiteboard_result').text('');
}

function enableWhiteboardSectionWithSuccessLoad() {
    $('#blocked_section').css('display', 'none');
    $('#whiteboard_result').text('');
    $('#whiteboard_new_name').val('');
    enableWhiteboardSection();
}

function showLoginDialog() {
    toggleShadowOverlay();
    $('#register_section').css('display', 'none');
    $('#login_section').css('display', 'none');
    $('#dialog_login > .success_message').css('display', 'none');
    $('#dialog_login').css('display', 'block');
    $('#login_method_section').css('display', 'block');
}

$('.dialog_close_button').click(function () {
    toggleShadowOverlay();
    $('#dialog_login').css('display', 'none');
    $('#dialog_whiteboards').css('display', 'none');
    $('#whiteboard_result').text('');
});