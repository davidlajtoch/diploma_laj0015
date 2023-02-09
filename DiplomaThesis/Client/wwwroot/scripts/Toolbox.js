

const MAX_FILE_SIZE = 1000000;
var is_fullscreen = false;
//optionsbox functions
//optionsbox new
$('#tb_option_new').click(function () {
    toggleShadowOverlay();
    $('#dialog_new').css('display', 'block');
});
$("#tb_option_new").hover(function () {
    $('#hint_box').text('Create new lobby');
}, function () {
    $('#hint_box').text('');
});
$('#dialog_new_btn_close').click(function () {
    $('#dialog_new').css('display', 'none');
    toggleShadowOverlay();
});
$('#dialog_new_btn_confirm').click(function () {
    draw_client.createNewLobby();
    $('#dialog_new').css('display', 'none');
    toggleShadowOverlay();
    $('#hint_box').text('New lobby created!');
});

//optionsbox share
function copyUrlClipboard() {
    var $temp = $("<input>");
    var $url = window.location.href;
    $("body").append($temp);
    $temp.val($url).select();
    document.execCommand("copy");
    $temp.remove();
}

$('#tb_option_share').click(function () {
    copyUrlClipboard();
    $('#hint_box').text('Lobby link copied to clipboard!');
});
$("#tb_option_share").hover(function () {
    $('#hint_box').text('Copy lobby link to clipboard');
}, function () {
    $('#hint_box').text('');
});

//optionsbox fullscreen
$('#tb_option_fullscreen').click(function () {
    if (!is_fullscreen) {
        var elem = document.documentElement;
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) { /* Safari */
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) { /* IE11 */
            elem.msRequestFullscreen();
        }
        $('#tb_option_fullscreen i').text('fullscreen_exit');
        is_fullscreen = true;
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) { /* Safari */
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) { /* IE11 */
            document.msExitFullscreen();
        }
        $('#tb_option_fullscreen i').text('fullscreen');
        is_fullscreen = false;
    }
    
});

//optionsbox whiteboards
$('#tb_option_whiteboards').click(function () {
    draw_client.loadSavedWhiteboardsList();
    toggleShadowOverlay();
    $('#dialog_whiteboards').css('display', 'block');
});

$("#tb_option_whiteboards").hover(function () {
    $('#hint_box').text('Load / save whiteboard');
}, function () {
    $('#hint_box').text('');
});

$('#dialog_whiteboards .slot').click(function () {
    $(this).addClass('light_grey selected_whiteboard').siblings().removeClass('light_grey selected_whiteboard');

    var selected_whiteboard_name = $('.selected_whiteboard .slot_info .whiteboard_name');
    if (selected_whiteboard_name.text() != '-') {
        $('#whiteboard_new_name').val(selected_whiteboard_name.text());
    }
});

$('#dialog_whiteboards_load').click(function () {
    var slot = $('.selected_whiteboard').attr('id');
    draw_client.loadWhiteboard(slot);
});

$('#dialog_whiteboards_save').click(function () {
    if (!$('#whiteboard_form')[0].reportValidity()) {
        return false;
    }

    var whiteboard_name = $('#whiteboard_new_name').val();

    if (containsSpecialChars(whiteboard_name)) {
        $('#whiteboard_result').text('Whiteboard name can only contain letters and numbers');
        return false;
    }

    if (whiteboard_name == "") {
        $('#whiteboard_result').text('Invalid whiteboard name');
        return false;
    }

    if (whiteboard_name.Length > 16) {
        $('#whiteboard_result').text('Whiteboard name must be between 1 and 16 characters');
        return false;
    }

    var slot = $('.selected_whiteboard').attr('id');
    var whiteboard_data = JSON.stringify(whiteboard_pages[current_page].canvas.toJSON(['id']));
    var id_counter = whiteboard_pages[current_page].id_counter;
    draw_client.saveWhiteboard(slot, whiteboard_data, whiteboard_name, id_counter);
});

//optionsbox profile
//register
function containsSpecialChars(str) {
    const specialChars = /[`!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~]/;
    return specialChars.test(str);
}

function hasWhiteSpace(str) {
    return str.indexOf(' ') >= 0;
}

$('#register_button').click(function () {
    if (!$('#register_form')[0].reportValidity()) {
        return false;
    }

    var username = $('#register_username').val();
    var email = $('#register_email').val();
    var password = $('#register_password').val();
    var password_repeat = $('#register_password_repeat').val();

    if (containsSpecialChars(username)) {
        $('#registration_result').text('Username can only contain letters and numbers');
        return false;
    }

    if (username.length < 4 || username.length > 16) {
        $('#registration_result').text('Username length must be between 4 and 16 characters');
        return false;
    }

    if (password.length < 8) {
        $('#registration_result').text('Minimum password length is 8 characters');
        return false;
    }
    if (password != password_repeat) {
        $('#registration_result').text('Passwords do not match');
        return false;
    }

    if (hasWhiteSpace(password) || hasWhiteSpace(password_repeat)) {
        $('#registration_result').text('Passwords must not contain a whitespace');
    }

    draw_client.registerUser(username, email, password, password_repeat);
});

//login
$('#login_button').click(function () {
    if (!$('#login_form')[0].reportValidity()) {
        return false;
    }

    var username = $('#login_username').val();
    var password = $('#login_password').val();

    if (username.length < 4 || username.length > 16) {
        $('#login_result').text('Invalid username');
        return false;
    }

    if (password.length < 8) {
        $('#login_result').text('Invalid password');
        return false;
    }

    draw_client.loginUser(username, password);
});

$('#tb_option_profile').click(function () {
    showLoginDialog();
});

$("#tb_option_profile").hover(function () {
    $('#hint_box').text('Log out');
}, function () {
    $('#hint_box').text('');
});

$('#login_method_button_registered').click(function () {
    $('#login_method_section').css('display', 'none');
    $('#login_section').css('display', 'block');
});

$('#login_method_button_guest').click(function () {
    draw_client.loginGuest();
    toggleShadowOverlay();
    $('#dialog_login').css('display', 'none');
});

$('#login_method_register').click(function () {
    $('#login_method_section').css('display', 'none');
    $('#register_section').css('display', 'block');
});

$("#login_method_register").hover(function () {
    $(this).css('color', $('#logo').css('color'));
}, function () {
    $(this).css('color', $('#login_method_section').css('color'));
});

$('.dialog_profile_back_button').click(function () {
    $('#login_section').css('display', 'none');
    $('#register_section').css('display', 'none');
    $('#login_method_section').css('display', 'block');
});

//toolbox functions
//toolbox color picker
$('.tb_color').click(function () {
    var color = $(this).css('color');
    $('#tb_color_selected').css('color', color);
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.setBrushColor(color);
    });
});
$(".tb_color").hover(function () {
    $('#hint_box').text('Color picker');
}, function () {
    $('#hint_box').text('');
});

//toolbox move tool
$('#tb_tool_move').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateMoveTool();
    });
    $('#tb_tool_selected > i').text('near_me');
});
$("#tb_tool_move").hover(function () {
    $('#hint_box').text('Selection tool (Alt + S)');
}, function () {
    $('#hint_box').text('');
});

//toolbox text tool
$('#tb_tool_text').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateTextTool();
    });
    $('#tb_tool_selected > i').text('text_fields');
});
$("#tb_tool_text").hover(function () {
    $('#hint_box').text('Text tool (Alt + W)');
}, function () {
    $('#hint_box').text('');
});

//toolbox erase tool
$('#tb_tool_eraser').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateEraseTool();
    });
    $('#tb_tool_selected > i').text('gradient');
});
$("#tb_tool_eraser").hover(function () {
    $('#hint_box').text('Eraser tool (Alt + G)');
}, function () {
    $('#hint_box').text('');
});

//toolbox remove tool
$('#tb_tool_remover').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateRemoveTool();
    });
    $('#tb_tool_selected > i').text('delete');
});
$("#tb_tool_remover").hover(function () {
    $('#hint_box').text('Remove tool (Alt + D)');
}, function () {
    $('#hint_box').text('');
});

//toolbox pen tool
$('#tb_tool_pen').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateDrawTool();
    });
    $('#tb_tool_selected > i').text('gesture');
});
$("#tb_tool_pen").hover(function () {
    $('#hint_box').text('Free drawing tool (Alt + F)');
}, function () {
    $('#hint_box').text('');
});

//toolbox line tool
$('#tb_tool_line').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateLineTool();
    });
    $('#tb_tool_selected > i').text('remove');
});
$("#tb_tool_line").hover(function () {
    $('#hint_box').text('Line tool (Alt + L)');
}, function () {
    $('#hint_box').text('');
});

//toolbox rectangle tool
$('#tb_tool_rectangle').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateRectangleTool();
    });
    $('#tb_tool_selected > i').text('crop_square');
});
$("#tb_tool_rectangle").hover(function () {
    $('#hint_box').text('Rectangle tool (Alt + R)');
}, function () {
    $('#hint_box').text('');
});

//toolbox triangle tool
$('#tb_tool_triangle').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateTriangleTool();
    });
    $('#tb_tool_selected > i').text('change_history');
});
$("#tb_tool_triangle").hover(function () {
    $('#hint_box').text('Triangle tool (Alt + T)');
}, function () {
    $('#hint_box').text('');
});

//toolbox ellipse tool
$('#tb_tool_ellipse').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateEllipseTool();
    });
    $('#tb_tool_selected > i').text('panorama_fish_eye');
});
$("#tb_tool_ellipse").hover(function () {
    $('#hint_box').text('Ellipse tool (Alt + E)');
}, function () {
    $('#hint_box').text('');
});

//toolbox line width tool
$('#line_width_range').on('input', function () {
    var width = $(this).val();
    var width_val = parseInt(width, 10) || 1;
    $('#tb_line_width_selected > span').text(width_val);
    $.each(whiteboard_pages, function (i, whiteboard) {
        var actual_width;
        switch (width_val) {
            case 1:
                actual_width = 1;
                break;
            case 2:
                actual_width = 2;
                break;
            case 3:
                actual_width = 5;
                break;
            case 4:
                actual_width = 10;
                break;
            case 5:
                actual_width = 20;
                break;
            case 6:
                actual_width = 40;
                break;
            case 7:
                actual_width = 60;
                break;
            case 8:
                actual_width = 80;
                break;
            case 9:
                actual_width = 120;
                break;
        }
        whiteboard.setBrushWidth(actual_width);
    });
});
$("#line_width_range").hover(function () {
    $('#hint_box').text('Line width tool');
}, function () {
    $('#hint_box').text('');
});

//toolbox pdf tool
$("#tb_pdf_upload").click(function (e) {
    e.preventDefault();
    $("#pdf_input:hidden").trigger('click');
});
$("#tb_pdf_upload").hover(function () {
    $('#hint_box').text('Upload PDF (maximum PDF size is 1MB)');
}, function () {
    $('#hint_box').text('');
});

document.querySelector('#pdf_input').addEventListener('change', async (e) => {
    /*if (e.target.files[0].size > MAX_FILE_SIZE) {
        toggleShadowOverlay();
        $('#dialog_image_size').css('display', 'block');
        return;
    }*/
    var fileType = e.target.files[0]["type"];
    var validImageTypes = ["application/pdf"];
    if ($.inArray(fileType, validImageTypes) < 0) {
        toggleShadowOverlay();
        $('#dialog_image_upload > span').text('Only .pdf files are supported!');
        $('#dialog_image_upload').css('display', 'block');
        $('#hint_box').text('PDF upload failed: only .pdf files are supported!');
        return;
    }
    await Promise.all(pdfToImage(e.target.files[0], whiteboard_pages[current_page].canvas));

});

//toolbox image tool
$("#tb_image_upload").click(function (e) {
    e.preventDefault();
    $("#image_input:hidden").trigger('click');
});

$("#tb_image_upload").hover(function () {
    $('#hint_box').text('Upload image (maximum image size is 1MB)');
}, function () {
    $('#hint_box').text('');
});

$('#dialog_image_upload_btn_confirm').click(function () {
    $('#dialog_image_upload').css('display', 'none');
    toggleShadowOverlay();
});

$('#btn_image_size_confirm').click(function () {
    toggleShadowOverlay();
    $('#dialog_image_size').css('display', 'none');
});

$('#image_input').change(function (e) {
    if (e.target.files[0].size > MAX_FILE_SIZE) {
        toggleShadowOverlay();
        $('#dialog_image_size').css('display', 'block');
        return;
    }

    var fileType = e.target.files[0]["type"];
    var validImageTypes = ["image/jpeg", "image/png"];
    if ($.inArray(fileType, validImageTypes) < 0) {
        toggleShadowOverlay();
        $('#dialog_image_upload > span').text('Only .jpg and .png images are supported!');
        $('#dialog_image_upload').css('display', 'block');
        $('#hint_box').text('Image upload failed: only .jpg and .png images are supported!');
        return;
    }

    var options = {
        maxSizeMB: 0.15,
        maxWidthOrHeight: 1920,
        useWebWorker: true
    }
    imageCompression(event.target.files[0], options)
        .then(function (compressed_img) {
            let reader = new FileReader();
            reader.readAsDataURL(compressed_img);
            reader.onload = function () {
                draw_client.drawImg(reader.result, current_page);
                draw_client.sendImg(reader.result, current_page);
            };
        })
        .catch(function (error) {
            console.log(error);
        });

    $("#tb_tool_move").trigger('click');
});

$("#tb_image_download").click(function () {
    whiteboard_pages[current_page].deselectObjects();
    console.log('#whiteboard' + (current_page+1));
    $('#whiteboard' + (current_page+1)).get(0).toBlob(function (blob) {
        saveAs(blob, "canvas.png");
    });
});
$("#tb_image_download").hover(function () {
    $('#hint_box').text('Download current view as jpg image');
}, function () {
    $('#hint_box').text('');
});

//toolbox bucket tool
$('#tb_tool_bucket').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateBucketTool();
        whiteboard.deselectObjects();
    });
    $('#tb_tool_selected > i').text('format_color_fill');
});
$("#tb_tool_bucket").hover(function () {
    $('#hint_box').text('Bucket tool (Alt + B)');
}, function () {
    $('#hint_box').text('');
});

//toolbox recolor tool
$('#tb_tool_recolor').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateRecolorTool();
        whiteboard.deselectObjects();
    });
    $('#tb_tool_selected > i').text('invert_colors');
});
$("#tb_tool_recolor").hover(function () {
    $('#hint_box').text('Recolor tool (Alt + C)');
}, function () {
    $('#hint_box').text('');
});

//toolbox color picker tool
$('#tb_tool_color_picker').click(function () {
    $.each(whiteboard_pages, function (i, whiteboard) {
        whiteboard.activateColorPickerTool();
    });
    $('#tb_tool_selected > i').text('colorize');
});
$("#tb_tool_color_picker").hover(function () {
    $('#hint_box').text('Color picker tool (Alt + P)');
}, function () {
    $('#hint_box').text('');
});

function copyColorPickerValueToClipboard() {
    console.log('xd');
    var $temp = $("<input>");
    var $color = whiteboard_pages[current_page].getColorAtPointer();
    var $color_rgb = 'rgba(' + $color.r + ', ' + $color.g + ', ' + $color.b + ', ' + $color.a + ')';
    $("body").append($temp);
    $temp.val($color_rgb).select();
    document.execCommand("copy");
    $temp.remove();
    $('#hint_box').text('Color value copied to clipboard!');
}

//toolbox animation tool
$("#animate_button").click(function () {
    whiteboard_pages[current_page].animateObject();
});