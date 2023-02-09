"use strict";


//console.log(whiteboard_pages[0].top_left);

////register whiteboard events

//pageselect box functions
const MAX_PAGES = 5;

var page_counter = 1;
var current_page = 0;

function addPage() {
    if (page_counter < MAX_PAGES) {

        var page_button = '<a id="psb_button_page' + (page_counter + 1) + '" class="psb_button_page">' + (page_counter + 1) + '</a> ';

        $('#psb_page_buttons').append(page_button);

        page_counter++;

        if (page_counter >= MAX_PAGES) {
            $('#psb_button_page_add').css('display', 'none');
        }
    }
}

//page button click functions
$('#psb_page_buttons').on('click', '.psb_button_page', function () {
    $('#psb_page_buttons .psb_button_page').each(function () {
        $(this).removeClass("psb_selected");
    });
    $(this).addClass("psb_selected");

    current_page = parseInt($(this).text()) - 1;

    $('#whiteboards_container').children().css('display', 'none');
    $('#whiteboard' + (current_page + 1)).parent().css('display', 'block');

});

//add button functions
$('#psb_button_page_add').click(function () {
    addPage();
    draw_client.sendAddPage();
    $('#psb_page_buttons').find('.psb_button_page').last().trigger('click');
});


var draw_client;
var whiteboard_pages;

export function connectDrawClient(user_group_id) {
    whiteboard_pages = [new Whiteboard(1), new Whiteboard(2), new Whiteboard(3), new Whiteboard(4), new Whiteboard(5)];
    draw_client = new DrawClient(user_group_id, whiteboard_pages);
    

    $.each(whiteboard_pages, function (i, whiteboard) {

        whiteboard.canvas.on('mouse:wheel', function (opt) {
            whiteboard.zoom(opt);
            opt.e.preventDefault();
            opt.e.stopPropagation();
        });

        whiteboard.canvas.on('object:added', function (e) {
            whiteboard.setObjectId(e.target);
            draw_client.sendObjectAdd(e.target);

        });

        whiteboard.canvas.on('selection:created', function (e) {
            if (whiteboard.drawing_modes.isEllipseMode || whiteboard.drawing_modes.isRectangleMode || whiteboard.drawing_modes.isTriangleMode || whiteboard.drawing_modes.isLineMode) { whiteboard.deselectObjects(); }
            else {
                if (whiteboard.drawing_modes.isDeleteMode) {
                    whiteboard.removeSelectedObject();
                    draw_client.sendObjectRemove(e.target);
                } else {
                    if (whiteboard.drawing_modes.isBucketMode) {
                        if (e.target.type == 'activeSelection') {
                            var objects_fills = [];
                            for (let i = 0; i < e.target._objects.length; i++) {
                                objects_fills.push(e.target._objects[i].fill);
                            }
                        }

                        whiteboard.bucketSelectedObject();
                        draw_client.sendObjectBucket(e.target);
                    } else {
                        if (whiteboard.drawing_modes.isRecolorMode) {
                            if (e.target.type == 'activeSelection') {
                                var objects_strokes = [];
                                for (let i = 0; i < e.target._objects.length; i++) {
                                    objects_strokes.push(e.target._objects[i].stroke);
                                }
                            }
                            whiteboard.recolorSelectedObject();
                            draw_client.sendObjectRecolor(e.target);
                        }
                    }
                }
            }
        });

        whiteboard.canvas.on('object:moved', function (e) {
            draw_client.sendObjectMove(e.target);
        });

        whiteboard.canvas.on('object:scaled', function (e) {
            draw_client.sendObjectScale(e.target);
        });

        whiteboard.canvas.on('object:rotated', function (e) {
            draw_client.sendObjectRotate(e.target);
        });

        whiteboard.canvas.on('mouse:down', function () {
            if (whiteboard.drawing_modes.isTextMode) {
                whiteboard.addText();
            } else {
                if (whiteboard.drawing_modes.isEllipseMode || whiteboard.drawing_modes.isRectangleMode || whiteboard.drawing_modes.isTriangleMode || whiteboard.drawing_modes.isLineMode) {
                    whiteboard.addObjectP1();
                } else {
                    if (whiteboard.drawing_modes.isColorPickerMode) {
                        copyColorPickerValueToClipboard();
                        whiteboard.canvas.discardActiveObject().renderAll();
                    }
                }
            }
        });

        whiteboard.canvas.on('mouse:up', function () {
            if (whiteboard.drawing_modes.isEllipseMode || whiteboard.drawing_modes.isRectangleMode || whiteboard.drawing_modes.isTriangleMode || whiteboard.drawing_modes.isLineMode) {
                whiteboard.addObjectP2();
            }
            if (whiteboard.drawing_modes.isLineMode) {
                whiteboard.addLine();
            } else {
                if (whiteboard.drawing_modes.isRectangleMode) {
                    whiteboard.addRectangle();
                } else {
                    if (whiteboard.drawing_modes.isEllipseMode) {
                        whiteboard.addEllipse();
                    } else {
                        if (whiteboard.drawing_modes.isTriangleMode) {
                            whiteboard.addTriangle();
                        }
                    }
                }
            }
        });

        whiteboard.canvas.on('text:editing:exited', function (e) {
            draw_client.sendTextModify(e.target);
        });

        //whiteboard.canvas.on('mouse:move', function (e) {
        //    pointer_move_counter += 1;
        //    if (pointer_move_counter >= POINTER_SEND_INTERVAL) {
        //        pointer_move_counter = 0;
        //        draw_client.sendPointer();
        //    }
        //    if (whiteboard.drawing_modes.isColorPickerMode) {
        //        whiteboard.setColorAtPointer(e);
        //    }
        //});

        window.addEventListener('resize', function () { whiteboard.resize(); }, false);
    });

    draw_client.connection.on("LoadMessagesRequest", function (caller) {
        draw_client.sendMessages(caller);
    });

    draw_client.connection.on("LoadMessages", function (messages) {
        draw_client.loadMessages(messages);
    });

    draw_client.connection.on("LoadMessage", function (connection_id, username, message) {
        draw_client.loadMessage(connection_id, username, message);
    });

    draw_client.connection.on("AddPage", function () {
        addPage();
    });

    draw_client.connection.on("LoadPointer", function (pointer_x, pointer_y, connection_id, username, page) {
        //console.log(pointer_x, pointer_y);
        drawPointer(pointer_x, pointer_y, connection_id, username, page);
    });

    draw_client.connection.on("LoadCanvasRequest", function (caller) {
        draw_client.sendCanvas(caller);
    });

    draw_client.connection.on("DrawCanvas", function (json, id_counter, page) {
        if (page > (page_counter - 1)) {
            addPage();
        }
        draw_client.drawCanvas(json, id_counter, page);
    });

    draw_client.connection.on("DrawBringObjectForward", function (id, page) {
        draw_client.drawBringObjectForward(id, page);
    });

    draw_client.connection.on("DrawSendObjectBackwards", function (id, page) {
        draw_client.drawSendObjectBackwards(id, page);
    });

    draw_client.connection.on("DrawObjectAdd", function (json, page) {
        console.log('draw.js', json);
        draw_client.drawObjectAdd(json, page);
    });

    draw_client.connection.on("DrawObjectMove", function (x, y, id, page) {
        draw_client.drawObjectMove(x, y, id, page);
    });

    draw_client.connection.on("DrawObjectScale", function (x, y, scaleX, scaleY, flipX, flipY, id, page) {
        draw_client.drawObjectScale(x, y, scaleX, scaleY, flipX, flipY, id, page);
    });

    draw_client.connection.on("DrawObjectRotate", function (angle, id, page) {
        draw_client.drawObjectRotate(angle, id, page);
    });

    draw_client.connection.on("DrawObjectRemove", function (id, page) {
        draw_client.drawObjectRemove(id, page);
    });

    draw_client.connection.on("DrawObjectBucket", function (fill, id, page) {
        draw_client.drawObjectBucket(fill, id, page);
    });

    draw_client.connection.on("DrawObjectRecolor", function (stroke, id, page) {
        draw_client.drawObjectRecolor(stroke, id, page);
    });

    draw_client.connection.on("DrawObjectGroup", function (objects, page) {
        draw_client.drawObjectGroup(objects, page);
    });

    draw_client.connection.on("DrawImg", function (img_data, page) {
        draw_client.drawImg(img_data, page);
    });

    draw_client.connection.on("DrawTextModify", function (text, id, page) {
        draw_client.drawTextModify(text, id, page);
    });
}

