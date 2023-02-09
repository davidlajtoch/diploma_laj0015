$(document).ready(function () {
    //disable default right click menu
    document.oncontextmenu = function () { return false; };

    $('.whiteboard').mousedown(function (e) {
        if (e.button == 2) {
            $('#tb_tool_move').trigger('click');
            $('#color_picker_hint_box').css('display', 'none');
            showRClickContextMenu(e.pageY, e.pageX);
            return false;
        }
        return true;
    });

    //keyboard shortcuts
    $(document).keydown(function (event) {
        if (!chb_open) {
            //Ctrl+c - copy selection
            if (event.ctrlKey && event.which === 67) {
                whiteboard_pages[current_page].copyObject();
                event.preventDefault();
            }
            //Ctrl+v - paste selection
            if (event.ctrlKey && event.which === 86) {
                whiteboard_pages[current_page].pasteObject();
                event.preventDefault();
            }
            //Ctrl+z - undo
            if (event.ctrlKey && event.which === 90) {
                $("#cb_undo_button").trigger('click');
                event.preventDefault();
            }
            //Ctrl+y - redo
            if (event.ctrlKey && event.which === 89) {
                $("#cb_redo_button").trigger('click');
                event.preventDefault();
            }
            //Alt+d - delete tool
            if (event.altKey && event.which === 68) {
                event.preventDefault();
                $('#tb_tool_remover').trigger('click');
            }
            //Alt+w - text tool
            if (event.altKey && event.which === 87) {
                event.preventDefault();
                $('#tb_tool_text').trigger('click');
            }
            //Alt+p - color picker tool
            if (event.altKey && event.which === 80) {
                event.preventDefault();
                $('#tb_tool_color_picker').trigger('click');
            }
            //Alt+b - bucket tool
            if (event.altKey && event.which === 66) {
                event.preventDefault();
                $('#tb_tool_bucket').trigger('click');
            }
            //Alt+c - recolor tool
            if (event.altKey && event.which === 67) {
                event.preventDefault();
                $('#tb_tool_recolor').trigger('click');
            }
            //Alt+g - eraser tool
            if (event.altKey && event.which === 71) {
                event.preventDefault();
                $('#tb_tool_eraser').trigger('click');
            }
            //Alt+e - ellipse tool
            if (event.altKey && event.which === 69) {
                event.preventDefault();
                $('#tb_tool_ellipse').trigger('click');
            }
            //Alt+e - recatngle tool
            if (event.altKey && event.which === 82) {
                event.preventDefault();
                $('#tb_tool_rectangle').trigger('click');
            }
            //Alt+t - triangle tool
            if (event.altKey && event.which === 84) {
                event.preventDefault();
                $('#tb_tool_triangle').trigger('click');
            }
            //Alt+l - line tool
            if (event.altKey && event.which === 76) {
                event.preventDefault();
                $('#tb_tool_line').trigger('click');
            }
            //Alt+f - pen tool
            if (event.altKey && event.which === 70) {
                event.preventDefault();
                $('#tb_tool_pen').trigger('click');
            }
            //Alt+s - selection tool
            if (event.altKey && event.which === 83) {
                event.preventDefault();
                $('#tb_tool_move').trigger('click');
            }
            //delete
            if (event.which === 46) {
                var selected_object = whiteboard_pages[current_page].getSelectedObject();
                if (selected_object != null) {
                    whiteboard_pages[current_page].removeSelectedObject();
                    draw_client.sendObjectRemove(selected_object);
                }

            }
        }
    });
});