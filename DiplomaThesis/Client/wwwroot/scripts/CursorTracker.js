var pointer_move_counter = 0;
const POINTER_SEND_INTERVAL = 25;

//cursor tracking
function getRandomColor() {
    return {
        r: Math.round(Math.random() * 256),
        g: Math.round(Math.random() * 256),
        b: Math.round(Math.random() * 256),
    };
}

function getRgb(rgb) {
    return "rgb(" + rgb.r + "," + rgb.g + "," + rgb.b + ")";
}

function createElementIfNotExists(id, username) {
    var element = $("#" + id);
    if (element.length == 0) {
        var color = getRandomColor();
        element = $("<div class='pointer' id='" + id + "'></div>");
        element.html("<span class='pointer-circle' style='background-color: " + getRgb(color) + "'></span><span class='pointer-username'>" + username + "</span>");
        $("body").append(element).show();
    }
    return element;
}

function drawPointer(pointer_x, pointer_y, connection_id, username, page) {
    var elem = createElementIfNotExists(connection_id, username);
    if (page === current_page) {
        var top_left = whiteboard_pages[page].top_left;
        var pointer_x_actual = (pointer_x - top_left.x) * whiteboard_pages[page].getZoom();
        var pointer_y_actual = (pointer_y - top_left.y) * whiteboard_pages[page].getZoom();
        $(elem).css({ left: pointer_x_actual, top: pointer_y_actual });
        $(elem).css('display', 'flex');
    } else {
        $(elem).css('display', 'none');
    }
}