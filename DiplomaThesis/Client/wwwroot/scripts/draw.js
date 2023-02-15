//"use strict";

var whiteboard = new Whiteboard();
var connection = new signalR.HubConnectionBuilder().withUrl("/drawhub").build();
var user_group;
var username;

function connectDrawClient(user_group_id) {
    user_group = user_group_id;
    connection.start().then(function () {
        addToGroup(user_group_id);
    }).catch(function (err) {
        return console.error(err.toString());
    });

    whiteboard.canvas.on('mouse:wheel', function (opt) {
        whiteboard.zoom(opt);
        opt.e.preventDefault();
        opt.e.stopPropagation();
    });

    whiteboard.canvas.on('object:added', function (e) {
        whiteboard.setObjectId(e.target);
        console.log('xd');
        sendObjectAdd(e.target);

    });

    whiteboard.canvas.on('selection:created', function (e) {
        if (whiteboard.drawing_modes.isEllipseMode || whiteboard.drawing_modes.isRectangleMode || whiteboard.drawing_modes.isTriangleMode || whiteboard.drawing_modes.isLineMode) { whiteboard.deselectObjects(); }
        else {
            if (whiteboard.drawing_modes.isDeleteMode) {
                whiteboard.removeSelectedObject();
                sendObjectRemove(e.target);
            } else {
                if (whiteboard.drawing_modes.isBucketMode) {
                    if (e.target.type == 'activeSelection') {
                        var objects_fills = [];
                        for (let i = 0; i < e.target._objects.length; i++) {
                            objects_fills.push(e.target._objects[i].fill);
                        }
                    }

                    whiteboard.bucketSelectedObject();
                    sendObjectBucket(e.target);
                } else {
                    if (whiteboard.drawing_modes.isRecolorMode) {
                        if (e.target.type == 'activeSelection') {
                            var objects_strokes = [];
                            for (let i = 0; i < e.target._objects.length; i++) {
                                objects_strokes.push(e.target._objects[i].stroke);
                            }
                        }
                        whiteboard.recolorSelectedObject();
                        sendObjectRecolor(e.target);
                    }
                }
            }
        }
    });

    whiteboard.canvas.on('object:moved', function (e) {
        sendObjectMove(e.target);
    });

    whiteboard.canvas.on('object:scaled', function (e) {
        sendObjectScale(e.target);
    });

    whiteboard.canvas.on('object:rotated', function (e) {
        sendObjectRotate(e.target);
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
        sendTextModify(e.target);
    });

    //whiteboard.canvas.on('mouse:move', function (e) {
    //    pointer_move_counter += 1;
    //    if (pointer_move_counter >= POINTER_SEND_INTERVAL) {
    //        pointer_move_counter = 0;
    //        sendPointer();
    //    }
    //    if (whiteboard.drawing_modes.isColorPickerMode) {
    //        whiteboard.setColorAtPointer(e);
    //    }
    //});

    window.addEventListener('resize', function () { whiteboard.resize(); }, false);

    connection.on("LoadMessagesRequest", function (caller) {
        sendMessages(caller);
    });

    connection.on("LoadMessages", function (messages) {
        loadMessages(messages);
    });

    connection.on("LoadMessage", function (connection_id, username, message) {
        loadMessage(connection_id, username, message);
    });

    connection.on("AddPage", function () {
        addPage();
    });

    connection.on("LoadPointer", function (pointer_x, pointer_y, connection_id, username) {
        //console.log(pointer_x, pointer_y);
        drawPointer(pointer_x, pointer_y, connection_id, username);
    });

    connection.on("LoadCanvasRequest", function (caller) {
        sendCanvas(caller);
    });

    connection.on("DrawCanvas", function (json, id_counter) {
        drawCanvas(json, id_counter);
    });

    connection.on("DrawBringObjectForward", function (id) {
        drawBringObjectForward(id);
    });

    connection.on("DrawSendObjectBackwards", function (id) {
        drawSendObjectBackwards(id);
    });

    connection.on("DrawObjectAdd", function (json) {
        console.log('draw.js', json);
        drawObjectAdd(json);
    });

    connection.on("DrawObjectMove", function (x, y, id) {
        drawObjectMove(x, y, id);
    });

    connection.on("DrawObjectScale", function (x, y, scaleX, scaleY, flipX, flipY, id) {
        drawObjectScale(x, y, scaleX, scaleY, flipX, flipY, id);
    });

    connection.on("DrawObjectRotate", function (angle, id) {
        drawObjectRotate(angle, id);
    });

    connection.on("DrawObjectRemove", function (id) {
        drawObjectRemove(id);
    });

    connection.on("DrawObjectBucket", function (fill, id) {
        drawObjectBucket(fill, id);
    });

    connection.on("DrawObjectRecolor", function (stroke, id) {
        drawObjectRecolor(stroke, id);
    });

    connection.on("DrawObjectGroup", function (objects) {
        drawObjectGroup(objects);
    });

    connection.on("DrawImg", function (img_data) {
        drawImg(img_data);
    });

    connection.on("DrawTextModify", function (text, id) {
        drawTextModify(text, id);
    });

//function loadWhiteboard(slot) {
//    toggleLoader();
//    disableWhiteboardSection();

//    $.ajax({
//        context: this,
//        type: 'post',
//        data: {
//            slot: slot
//        },
//        url: '/Whiteboard/LoadWhiteboard',
//        success: function (result) {
//            if (result.answer != "") {
//                toggleLoader();
//                enableWhiteboardSectionWithError(result.answer);
//            }
//            else {
//                this.drawCanvas(result.whiteboard_data, result.id_counter, this.current_page);
//                this.sendCanvasAll(result.whiteboard_data, result.id_counter);
//                enableWhiteboardSectionWithSuccessLoad();
//                toggleLoader();
//            }
//        }
//    });
//}

//function saveWhiteboard(slot, whiteboard_data, whiteboard_name, id_counter) {
//    toggleLoader();
//    disableWhiteboardSection();

//    $.ajax({
//        type: 'post',
//        data: {
//            slot: slot, whiteboard_data: whiteboard_data, whiteboard_name: whiteboard_name, id_counter: id_counter
//        },
//        url: '/Whiteboard/SaveWhiteboard',
//        success: function (result) {
//            if (result != "") {
//                toggleLoader();
//                enableWhiteboardSectionWithError(result);
//            }
//            else {
//                toggleLoader();
//                enableWhiteboardSectionWithSuccessSave(slot, whiteboard_name);
//            }
//        }
//    });
//}
}
function addToGroup(group) {
    connection.invoke("AddToGroup", group).catch(function (err) {
        return console.error(err.toString());
    });
}

function removeFromGroup(group) {
    connection.invoke("RemoveFromGroup", group).catch(function (err) {
        return console.error(err.toString());
    });
}

function loadMessagesRequest() {
    connection.invoke("LoadMessagesRequest", user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function loadMessages(messages) {
    this.messages = messages.slice();

    for (i = 0; i < this.messages.length;) {
        addMessage(this.messages[i], this.messages[i + 1]);
        i += 2
    }
}

function loadMessage(connection_id, username, message) {
    this.messages.push([username]);
    this.messages.push([message]);
    addMessage(connection_id, username, message);
}

function sendMessages(caller) {
    connection.invoke("SendMessages", this.messages, caller).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendMessage(message) {
    this.messages.push(this.username);
    this.messages.push(message);

    addMessage("", this.username, message);

    connection.invoke("SendMessage", this.username, message, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendAddPage() {
    connection.invoke("SendAddPage", user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendPointer() {
    var pointer = this.whiteboard.getPointer();
    connection.invoke("SendPointer", pointer.x, pointer.y, this.username, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function loadCanvasRequest() {
    connection.invoke("LoadCanvasRequest", user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendCanvas(caller) {
    connection.invoke("SendCanvas", JSON.stringify(whiteboard.canvas.toJSON(['id'])), whiteboard.id_counter, caller).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendCanvasAll(whiteboard_data, id_counter) {
    connection.invoke("SendCanvasAll", whiteboard_data, id_counter, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendBringObjectForward(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            whiteboard.bringObjectForward(object._objects[i].id);
            connection.invoke("SendBringObjectForward", object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        whiteboard.bringObjectForward(object.id);
        connection.invoke("SendBringObjectForward", object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendSendObjectBackwards(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            whiteboard.sendObjectBackwards(object._objects[i].id);
            connection.invoke("SendSendObjectBackwards", object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        whiteboard.sendObjectBackwards(object.id);
        connection.invoke("SendSendObjectBackwards", object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendObjectAdd(object) {
    console.log('sendobjectadd', object);
    connection.invoke("SendObjectAdd", object, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendObjectMove(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {

            var left = object.left + (object.width / 2.0) + object._objects[i].left;
            var top = object.top + (object.height / 2.0) + object._objects[i].top;

            connection.invoke("SendObjectMove", left, top, object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectMove", object.left, object.top, object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}


function sendObjectScale(object) {
    if (object.type == 'activeSelection') {
        whiteboard.deselectObjects();
        for (let i = 0; i < object._objects.length; i++) {

            var actual_object = whiteboard.getObjectById(object._objects[i].id);

            connection.invoke("SendObjectScale", actual_object.left, actual_object.top, actual_object.scaleX, actual_object.scaleY, actual_object.id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectScale", object.left, object.top, object.scaleX, object.scaleY, object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendObjectRotate(object) {
    if (object.type == 'activeSelection') {
        whiteboard.deselectObjects();
        for (let i = 0; i < object._objects.length; i++) {

            console.log(object._objects[i].id);

            var actual_object = whiteboard.getObjectById(object._objects[i].id);

            console.log(actual_object);

            connection.invoke("SendObjectRotate", actual_object.angle, actual_object.id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
            connection.invoke("SendObjectMove", actual_object.left, actual_object.top, actual_object.id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectRotate", object.angle, object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }

}

function sendObjectRemove(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            connection.invoke("SendObjectRemove", object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectRemove", object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendObjectBucket(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            connection.invoke("SendObjectBucket", object._objects[i].fill, object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectBucket", whiteboard.free_drawing_brush.color, object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendObjectRecolor(object) {
    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            connection.invoke("SendObjectRecolor", object._objects[i].stroke, object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    } else {
        connection.invoke("SendObjectRecolor", whiteboard.free_drawing_brush.color, object.id, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendObjectGroup(object) {

    if (object.type == 'activeSelection') {
        for (let i = 0; i < object._objects.length; i++) {
            connection.invoke("SendObjectRemove", object._objects[i].id, user_group).catch(function (err) {
                return console.error(err.toString());
            });
        }
        connection.invoke("SendObjectGroup", object, user_group).catch(function (err) {
            return console.error(err.toString());
        });
    }
}

function sendImg(img_data) {
    connection.invoke("SendImg", img_data, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function sendTextModify(text_object) {
    connection.invoke("SendTextModify", text_object.text, text_object.id, user_group).catch(function (err) {
        return console.error(err.toString());
    });
}

function drawCanvas(json, id_counter) {
    whiteboard.loadCanvas(json, id_counter);
}

function drawBringObjectForward(id) {
    whiteboard.bringObjectForward(id);
}

function drawSendObjectBackwards(id) {
    whiteboard.sendObjectBackwards(id);
}

function drawObjectAdd(json) {

    whiteboard.disableEvent("object:added");
    whiteboard.addObject(json);
    whiteboard.enableEvent('object:added');
}

function drawObjectMove(x, y, id) {
    whiteboard.disableEvent('object:moved');
    whiteboard.moveObject(x, y, id);
    whiteboard.enableEvent('object:moved');
}

function drawObjectScale(x, y, scaleX, scaleY, id) {
    whiteboard.disableEvent('object:scaled');
    whiteboard.scaleObject(x, y, scaleX, scaleY, id);
    whiteboard.enableEvent('object:scaled');
}

function drawObjectRotate(angle, id) {
    whiteboard.rotateObject(angle, id);
    whiteboard.enableEvent('object:moved');
}

function drawObjectRemove(id) {
    whiteboard.removeObjectById(id);
}

function drawObjectBucket(fill, id) {
    whiteboard.bucketObjectById(fill, id);
}

function drawObjectRecolor(stroke, id) {
    whiteboard.recolorObjectById(stroke, id);
}

function drawObjectGroup(objects) {
    whiteboard.groupObjects(objects);
}

function drawImg(img_data) {
    whiteboard.addImg(img_data);
}

function drawTextModify(text, id) {
    whiteboard.modifyText(text, id);
}
