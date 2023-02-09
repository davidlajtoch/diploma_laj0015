"use strict";

class DrawClient {
    constructor(user_group_id, whiteboard_pages) {
        this.connection = new signalR.HubConnectionBuilder().withUrl("/drawhub").build();
        this.group = user_group_id;
        var this_tmp = this;
        this.connection.start().then(function () {
            this_tmp.addToGroup(user_group_id);
        }).catch(function (err) {
            return console.error(err.toString());
        });
        
        this.username = "";
        this.messages = [];
        this.whiteboard_pages = this.whiteboard_pages
        this.current_page = 0;
        console.log(user_group_id);
    }

    loadWhiteboard(slot) {
        toggleLoader();
        disableWhiteboardSection();

        $.ajax({
            context: this,
            type: 'post',
            data: {
                slot: slot
            },
            url: '/Whiteboard/LoadWhiteboard',
            success: function (result) {
                if (result.answer != "") {
                    toggleLoader();
                    enableWhiteboardSectionWithError(result.answer);
                }
                else {
                    this.drawCanvas(result.whiteboard_data, result.id_counter, this.current_page);
                    this.sendCanvasAll(result.whiteboard_data, result.id_counter);
                    enableWhiteboardSectionWithSuccessLoad();
                    toggleLoader();
                }
            }
        });
    }

    saveWhiteboard(slot, whiteboard_data, whiteboard_name, id_counter) {
        toggleLoader();
        disableWhiteboardSection();

        $.ajax({
            type: 'post',
            data: {
                slot: slot, whiteboard_data: whiteboard_data, whiteboard_name: whiteboard_name, id_counter: id_counter
            },
            url: '/Whiteboard/SaveWhiteboard',
            success: function (result) {
                if (result != "") {
                    toggleLoader();
                    enableWhiteboardSectionWithError(result);
                }
                else {
                    toggleLoader();
                    enableWhiteboardSectionWithSuccessSave(slot, whiteboard_name);
                }
            }
        });
    }

    addToGroup(group) {
        this.connection.invoke("AddToGroup", group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    removeFromGroup(group) {
        this.connection.invoke("RemoveFromGroup", group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    loadMessagesRequest() {
        this.connection.invoke("LoadMessagesRequest", this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    loadMessages(messages) {
        this.messages = messages.slice();

        for (i = 0; i < this.messages.length;) {
            addMessage(this.messages[i], this.messages[i + 1]);
            i += 2
        }
    }

    loadMessage(connection_id, username, message) {
        this.messages.push([username]);
        this.messages.push([message]);
        addMessage(connection_id, username, message);
    }

    sendMessages(caller) {
        this.connection.invoke("SendMessages", this.messages, caller).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendMessage(message) {
        this.messages.push(this.username);
        this.messages.push(message);

        addMessage("", this.username, message);

        this.connection.invoke("SendMessage", this.username, message, this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendAddPage() {
        this.connection.invoke("SendAddPage", this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendPointer() {
        var pointer = this.whiteboard_pages[this.current_page].getPointer();
        this.connection.invoke("SendPointer", pointer.x, pointer.y, this.username, this.current_page, this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    loadCanvasRequest() {
        this.connection.invoke("LoadCanvasRequest", this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendCanvas(caller) {
        for (let i = 0; i < page_counter; i++) {
            this.connection.invoke("SendCanvas", JSON.stringify(whiteboard_pages[i].canvas.toJSON(['id'])), whiteboard_pages[i].id_counter, i, caller).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendCanvasAll(whiteboard_data, id_counter) {
        this.connection.invoke("SendCanvasAll", whiteboard_data, id_counter, this.current_page, this.group).catch(function (err) {
            return console.error(err.toString());
        });

    }

    sendBringObjectForward(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                whiteboard_pages[current_page].bringObjectForward(object._objects[i].id);
                this.connection.invoke("SendBringObjectForward", object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            whiteboard_pages[current_page].bringObjectForward(object.id);
            this.connection.invoke("SendBringObjectForward", object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendSendObjectBackwards(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                whiteboard_pages[current_page].sendObjectBackwards(object._objects[i].id);
                this.connection.invoke("SendSendObjectBackwards", object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            whiteboard_pages[current_page].sendObjectBackwards(object.id);
            this.connection.invoke("SendSendObjectBackwards", object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendObjectAdd(object) {
        console.log('sendobjectadd', object);
        this.connection.invoke("SendObjectAdd", object, this.current_page, this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendObjectMove(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {

                var left = object.left + (object.width / 2.0) + object._objects[i].left;
                var top = object.top + (object.height / 2.0) + object._objects[i].top;

                this.connection.invoke("SendObjectMove", left, top, object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectMove", object.left, object.top, object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    
    sendObjectScale(object) {
        if (object.type == 'activeSelection') {
            whiteboard_pages[this.current_page].deselectObjects();
            for (let i = 0; i < object._objects.length; i++) {

                var actual_object = whiteboard_pages[this.current_page].getObjectById(object._objects[i].id);

                this.connection.invoke("SendObjectScale", actual_object.left, actual_object.top, actual_object.scaleX, actual_object.scaleY, actual_object.id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectScale", object.left, object.top, object.scaleX, object.scaleY, object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendObjectRotate(object) {
        if (object.type == 'activeSelection') {
            whiteboard_pages[this.current_page].deselectObjects();
            for (let i = 0; i < object._objects.length; i++) {

                console.log(object._objects[i].id);

                var actual_object = whiteboard_pages[this.current_page].getObjectById(object._objects[i].id);

                console.log(actual_object);

                this.connection.invoke("SendObjectRotate", actual_object.angle, actual_object.id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
                this.connection.invoke("SendObjectMove", actual_object.left, actual_object.top, actual_object.id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectRotate", object.angle, object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
        
    }

    sendObjectRemove(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                this.connection.invoke("SendObjectRemove", object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectRemove", object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        } 
    }

    sendObjectBucket(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                this.connection.invoke("SendObjectBucket", object._objects[i].fill, object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectBucket", whiteboard_pages[this.current_page].free_drawing_brush.color, object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendObjectRecolor(object) {
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                this.connection.invoke("SendObjectRecolor", object._objects[i].stroke, object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        } else {
            this.connection.invoke("SendObjectRecolor", whiteboard_pages[this.current_page].free_drawing_brush.color, object.id, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendObjectGroup(object) {
        
        if (object.type == 'activeSelection') {
            for (let i = 0; i < object._objects.length; i++) {
                this.connection.invoke("SendObjectRemove", object._objects[i].id, this.current_page, this.group).catch(function (err) {
                    return console.error(err.toString());
                });
            }
            this.connection.invoke("SendObjectGroup", object, this.current_page, this.group).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }

    sendImg(img_data) {
        this.connection.invoke("SendImg", img_data, this.current_page, this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }

    sendTextModify(text_object) {
        this.connection.invoke("SendTextModify", text_object.text, text_object.id, this.current_page, this.group).catch(function (err) {
            return console.error(err.toString());
        });
    }
    
    drawCanvas(json, id_counter, page) {
        whiteboard_pages[page].loadCanvas(json, id_counter);
    }

    drawBringObjectForward(id, page) {
        whiteboard_pages[page].bringObjectForward(id);
    }

    drawSendObjectBackwards(id, page) {
        whiteboard_pages[page].sendObjectBackwards(id);
    }

    drawObjectAdd(json, page) {
        
        whiteboard_pages[page].disableEvent("object:added");
        whiteboard_pages[page].addObject(json);
        whiteboard_pages[page].enableEvent('object:added');
    }

    drawObjectMove(x, y, id, page) {
        whiteboard_pages[page].disableEvent('object:moved');
        whiteboard_pages[page].moveObject(x, y, id);
        whiteboard_pages[page].enableEvent('object:moved');
    }

    drawObjectScale(x, y, scaleX, scaleY, id, page) {
        whiteboard_pages[page].disableEvent('object:scaled');
        whiteboard_pages[page].scaleObject(x, y, scaleX, scaleY, id);
        whiteboard_pages[page].enableEvent('object:scaled');
    }

    drawObjectRotate(angle, id, page) {
        whiteboard_pages[page].rotateObject(angle, id);
        whiteboard_pages[page].enableEvent('object:moved');
    }

    drawObjectRemove(id, page) {
        whiteboard_pages[page].removeObjectById(id);
    }

    drawObjectBucket(fill, id, page) {
        whiteboard_pages[page].bucketObjectById(fill, id);
    }

    drawObjectRecolor(stroke, id, page) {
        whiteboard_pages[page].recolorObjectById(stroke, id);
    }

    drawObjectGroup(objects, page) {
        whiteboard_pages[page].groupObjects(objects);
    }

    drawImg(img_data, page) {
        whiteboard_pages[page].addImg(img_data);
    }

    drawTextModify(text, id, page) {
        whiteboard_pages[page].modifyText(text, id);
    }
}