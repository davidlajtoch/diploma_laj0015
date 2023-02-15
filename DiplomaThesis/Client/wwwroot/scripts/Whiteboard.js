//used by whiteboard with copy / paste
var clipboard;

fabric.Object.prototype.set({
    borderColor: 'rgba(0, 0, 0, 0.5)',
    borderDashArray: [3, 3],
    cornerColor: 'rgba(0, 0, 0, 0.5)',
    cornerStyle: 'circle',
    cornerSize: 11,
    transparentCorners: false,
    perPixelTargetFind: true,
    lockScalingFlip: true
});

class Whiteboard {
    constructor() {

        this.canvas = new fabric.Canvas('whiteboard', {
            isDrawingMode: true,
            imageSmoothingEnabled: false,
            backgroundColor: '#fff',
            selection: true,
            perPixelTargetFind: true,
            selectionFullyContained: true,
            allowTouchScrolling: true,
            targetFindTolerance: 8,
        });
        console.log('SHITEBOARD');

        this.top_left = { x: 0, y: 0 };

        this.canvas.selectionColor = 'rgba(0,0,0,0.1)';

        this.free_drawing_brush = new fabric.PencilBrush(this.canvas);
        this.eraser_brush = new fabric.EraserBrush(this.canvas);

        this.free_drawing_brush.color = 'rgba(0,0,0,1)';
        this.canvas.freeDrawingBrush = this.free_drawing_brush;

        this.line_width = 1;

        this.drawing_modes = {
            isDeleteMode: false,
            isTextMode: false,
            isBucketMode: false,
            isRecolorMode: false,
            isLineMode: false,
            isRectangleMode: false,
            isEllipseMode: false,
            isTriangleMode: false,
            isColorPickerMode: false
        };

        this.id_counter = 0;
        this.current_line = 0;

        this.color_at_pointer = {
            r: 0,
            g: 0,
            b: 0,
            a: 0
        },

            this.object_p1 = [];
        this.object_p2 = [];

        this.defaultCursors = [this.canvas.defaultCursor, this.canvas.hoverCursor, this.canvas.moveCursor, this.canvas.rotateCursor];

        this.disabled_events = {};

        this.resize();
    }

    activateDrawingMode(mode) {
        Object.entries(this.drawing_modes).forEach(([k, v]) => {
            this.drawing_modes[k] = false;
        });

        if (mode != '') {
            this.drawing_modes[mode] = true;
        }
    }

    resetCursors() {
        this.canvas.defaultCursor = this.defaultCursors[0];
        this.canvas.hoverCursor = this.defaultCursors[1];
        this.canvas.moveCursor = this.defaultCursors[2];
        this.canvas.rotateCursor = this.defaultCursors[3];
    }

    getPointer() {
        return this.canvas.getPointer();
    }

    setTopLeft() {
        this.top_left = {
            x: fabric.util.invertTransform(this.canvas.viewportTransform)[4],
            y: fabric.util.invertTransform(this.canvas.viewportTransform)[5]
        }
    }

    getZoom() {
        return this.canvas.getZoom();
    }

    loadCanvas(json, id_counter) {
        this.disableEvent("object:added");
        var tmp_this = this;
        this.canvas.loadFromJSON(json, function () {
            tmp_this.enableEvent("object:added");
        });
        this.id_counter = id_counter;
        this.history[0] = json;
    }

    resize() {
        this.canvas.setHeight(window.innerHeight);
        this.canvas.setWidth(window.innerWidth);
        this.canvas.renderAll();
    }

    zoom(opt) {
        var delta = opt.e.deltaY;
        var zoom = this.canvas.getZoom();
        zoom *= 0.999 ** delta;
        if (zoom > 20) zoom = 20;
        if (zoom < 0.05) zoom = 0.05;
        this.canvas.zoomToPoint({
            x: opt.e.offsetX,
            y: opt.e.offsetY
        }, zoom);

        this.setTopLeft();
    }

    setObjectId(object) {
        object['id'] = this.id_counter;
        this.id_counter += 1;
        return this.id_counter - 1;
    }

    getObjectById(id) {
        var object = this.canvas.getObjects().filter(
            obj => obj.id === id
        );
        return object[0];
    }

    getSelectedObject() {
        return this.canvas.getActiveObject();
    }

    clearObjects() {
        this.canvas.remove(...this.canvas.getObjects());
        this.id_counter = 0;
        this.canvas.renderAll();
    }

    deselectObjects() {
        this.canvas.discardActiveObject().renderAll();
    }

    setColorAtPointer(e) {
        var canvas_context = this.canvas.getContext('2d');
        var pointer = this.canvas.getPointer(e.e);
        var x = parseInt(pointer.x);
        var y = parseInt(pointer.y);
        var pixel = canvas_context.getImageData(x, y, 1, 1).data;
        var color = {
            r: pixel[0],
            g: pixel[1],
            b: pixel[2],
            a: pixel[3],
        };
        this.color_at_pointer = color;
    }

    getColorAtPointer() {
        return this.color_at_pointer;
    }

    addObjectP1() {
        var pointer = this.getPointer();
        this.object_p1[0] = pointer.x;
        this.object_p1[1] = pointer.y;
    }
    addObjectP2() {
        var pointer = this.getPointer();
        this.object_p2[0] = pointer.x;
        this.object_p2[1] = pointer.y;
    }

    addLine() {
        var line = new fabric.Line(
            [
                this.object_p1[0],
                this.object_p1[1],
                this.object_p2[0],
                this.object_p2[1]
            ],
            {
                id: this.id_counter,
                stroke: this.free_drawing_brush.color,
                strokeWidth: this.free_drawing_brush.width,
                selectable: true,
                originX: "center",
                originY: "center"
            }
        );
        this.canvas.add(line);
        this.canvas.renderAll();
    }

    addRectangle() {
        var left = this.object_p1[0];
        var top = this.object_p1[1];
        if (this.object_p1[0] > this.object_p2[0]) {
            left = Math.abs(this.object_p2[0]);
        }
        if (this.object_p1[1] > this.object_p2[1]) {
            top = Math.abs(this.object_p2[1]);
        }

        var width = Math.abs(this.object_p1[0] - this.object_p2[0]);
        var height = Math.abs(this.object_p1[1] - this.object_p2[1]);

        var rect = new fabric.Rect({
            left: left,
            top: top,
            width: width,
            height: height,
            stroke: this.free_drawing_brush.color,
            strokeWidth: this.free_drawing_brush.width,
            fill: 'rgba(0,0,0,0)'
        });

        this.canvas.add(rect);
        this.canvas.renderAll();
        console.log(rect);
    }

    addTriangle() {
        var left = this.object_p1[0];
        var top = this.object_p1[1];
        if (this.object_p1[0] > this.object_p2[0]) {
            left = Math.abs(this.object_p2[0]);
        }
        if (this.object_p1[1] > this.object_p2[1]) {
            top = Math.abs(this.object_p2[1]);
        }

        var width = Math.abs(this.object_p1[0] - this.object_p2[0]);
        var height = Math.abs(this.object_p1[1] - this.object_p2[1]);

        var triangle = new fabric.Triangle({
            left: left,
            top: top,
            width: width,
            height: height,
            stroke: this.free_drawing_brush.color,
            strokeWidth: this.free_drawing_brush.width,
            fill: 'rgba(0,0,0,0)'
        });

        this.canvas.add(triangle);
        this.canvas.renderAll();
    }

    addEllipse() {

        var rx = Math.abs(this.object_p1[0] - this.object_p2[0]) / 2;
        var ry = Math.abs(this.object_p1[1] - this.object_p2[1]) / 2;

        var origin_x = 'left';
        var origin_y = 'top';

        if (this.object_p1[0] > this.object_p2[0]) {
            origin_x = 'right';
        }
        if (this.object_p1[1] > this.object_p2[1]) {
            origin_y = 'bottom';
        }

        var ellipse = new fabric.Ellipse({
            id: this.id_counter,
            left: this.object_p1[0],
            top: this.object_p1[1],
            originX: origin_x,
            originY: origin_y,
            rx: rx,
            ry: ry,
            angle: 0,
            fill: '',
            stroke: this.free_drawing_brush.color,
            strokeWidth: this.free_drawing_brush.width
        });

        this.canvas.add(ellipse);
        this.canvas.renderAll();
    }

    addImg(img_data) {
        var c = this.canvas;
        var id = this.id_counter;
        var this_tmp = this;
        fabric.Image.fromURL(img_data, function (img) {
            var oImg = img.set({
                id: id,
                left: 10,
                top: 10,
                angle: 0,
            }).scale(0.9);
            this_tmp.disableEvent("object:added");
            c.add(oImg);
            this_tmp.enableEvent("object:added");
        });
        this.canvas.renderAll();
        this.id_counter += 1;
    }

    addObject(json) {
        console.log(json);
        if (!json.hasOwnProperty('id')) {
            this.setObjectId(json);
        }

        var tmp = this.canvas;
        var ths = this;

        fabric.util.enlivenObjects([json], function (enlivenedObjects) {
            ths.disableEvent('object:added');
            tmp.add(enlivenedObjects[0]);
            ths.enableEvent('object:added');
            //tmp.renderAll();

        });
        this.canvas = tmp;
        this.canvas.renderAll();
    }

    groupObjects(objects) {
        objects.toGroup();
        this.canvas.requestRenderAll();
    }

    copyObject() {
        // clone what are you copying since you
        // may want copy and paste on different moment.
        // and you do not want the changes happened
        // later to reflect on the copy.
        if (this.canvas.getActiveObject() != null) {
            this.canvas.getActiveObject().clone(function (cloned) {
                clipboard = cloned;
            });
        }
    }

    doPasteOne(object) {
        var cnv = this.canvas;
        object.clone(function (cloned_object) {
            cnv.discardActiveObject();
            cloned_object.set({
                left: cloned_object.left + 30,
                top: cloned_object.top + 30,
                evented: true,
            });
            cnv.add(cloned_object);
            cnv.setActiveObject(cloned_object);
            cnv.requestRenderAll();
        });
    }

    doPasteMany(object) {
        var cnv = this.canvas;
        object.clone(function (cloned_object) {
            cnv.discardActiveObject();
            cloned_object.set({
                left: clipboard.left + (clipboard.width / 2.0) + object.left + 30,
                top: clipboard.top + (clipboard.height / 2.0) + object.top + 30,
                evented: true,
            });
            cnv.add(cloned_object);
            cnv.requestRenderAll();
        });
    }

    pasteObject() {
        if (clipboard._objects != null) {
            for (let i = 0; i < clipboard._objects.length; i++) {
                this.doPasteMany(clipboard._objects[i]);
            }
        } else {
            this.doPasteOne(clipboard);
        }

    }

    bringObjectForward(object_id) {
        this.deselectObjects();
        this.getObjectById(object_id).bringForward(true);
    }
    sendObjectBackwards(object_id) {
        this.deselectObjects();
        this.getObjectById(object_id).sendBackwards(true);
    }

    moveObject(x, y, id) {
        var object = this.getObjectById(id);
        object.set({
            left: x,
            top: y,
        });
        object.setCoords();
        this.canvas.renderAll();
    }

    scaleObject(x, y, scaleX, scaleY, id) {
        var object = this.getObjectById(id);
        object.set({
            left: x,
            top: y,
            scaleX: scaleX,
            scaleY: scaleY,
        });
        object.setCoords();
        this.canvas.renderAll();
    }

    rotateObject(angle, id) {
        var object = this.getObjectById(id);
        object.rotate(angle);
        object.setCoords();
        this.canvas.renderAll();
    }

    addText() {
        var new_text = new fabric.IText("", {
            fontFamily: "nunito_bold"
        });
        new_text.left = this.canvas.getPointer().x;
        new_text.top = this.canvas.getPointer().y;
        new_text.fontSize = this.free_drawing_brush.width;
        new_text.fill = this.canvas.freeDrawingBrush.color;

        this.canvas.add(new_text);
        this.canvas.setActiveObject(new_text);
        new_text.enterEditing();
        new_text.selectAll();

        $("#tb_tool_move").trigger('click');
    }

    modifyText(text, id) {
        var object = this.getObjectById(id);
        object.set({
            text: text
        });
        this.canvas.renderAll();
    }

    removeSelectedObject() {
        this.canvas.getActiveObjects().forEach((obj) => {
            this.canvas.remove(obj)
        });
        this.canvas.discardActiveObject().renderAll();
    }
    removeObjectById(id) {
        var object = this.getObjectById(id);
        this.canvas.remove(object);
    }

    bucketSelectedObject() {
        this.canvas.getActiveObjects().forEach((obj) => {
            if (obj.type == 'group') {
                for (let i = 0; i < obj._objects.length; i++) {
                    console.log(obj._objects[i]);
                    obj._objects[i].set('fill', this.free_drawing_brush.color);
                }
            } else {
                obj.set('fill', this.free_drawing_brush.color);
            }
            //console.log(obj);
        });
        this.canvas.discardActiveObject().renderAll();
    }

    bucketObjectById(fill, id) {
        let object = this.getObjectById(id);
        console.log(object);
        if (object.type == "group") {
            for (let i = 0; i < object._objects.length; i++) {
                object._objects[i].set('fill', fill);
            }
        } else {
            object.set('fill', fill);
        }
        this.canvas.renderAll();
    }

    recolorSelectedObject() {
        this.canvas.getActiveObjects().forEach((obj) => {
            if (obj.type == 'group') {
                for (let i = 0; i < obj._objects.length; i++) {
                    obj._objects[i].set('stroke', this.free_drawing_brush.color);
                }
            } else {
                obj.set('stroke', this.free_drawing_brush.color);
            }
        });
        this.canvas.discardActiveObject().renderAll();
    }

    recolorObjectById(stroke, id) {
        let object = this.getObjectById(id);
        console.log(object);
        if (object.type == "group") {
            for (let i = 0; i < object._objects.length; i++) {
                object._objects[i].set('stroke', stroke);
            }
        } else {
            object.set('stroke', stroke);
        }
        this.canvas.renderAll();
    }

    //----------animate----------------------------------------------------

    animateObject() {
        console.log('x');
        this.canvas.getActiveObjects().forEach((obj) => {
            console.log(obj);
            obj.animate('angle', 45, {
                onChange: this.canvas.renderAll.bind(this.canvas)
            });
        });
        this.canvas.discardActiveObject().renderAll();

    }

    //----------setters----------------------------------------------------
    setBrushColor(color) {
        this.free_drawing_brush.color = color;
        this.eraser_brush.color = color;
    }

    setBrushWidth(width) {
        this.free_drawing_brush.width = width;
        this.eraser_brush.width = width;
    }

    activateMoveTool() {
        this.resetCursors();

        this.canvas.hoverCursor = 'grab';
        this.canvas.moveCursor = 'grabbing';
        this.canvas.rotateCursor = 'grab';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('');
    }

    activateLineTool() {
        this.deselectObjects();

        this.resetCursors();
        this.canvas.defaultCursor = 'crosshair';
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isLineMode');
    }

    activateRectangleTool() {
        this.deselectObjects();

        this.resetCursors();
        this.canvas.defaultCursor = 'crosshair';
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isRectangleMode');
    }

    activateTriangleTool() {
        this.deselectObjects();

        this.resetCursors();
        this.canvas.defaultCursor = 'crosshair';
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isTriangleMode');
    }

    activateEllipseTool() {
        this.deselectObjects();

        this.resetCursors();
        this.canvas.defaultCursor = 'crosshair';
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isEllipseMode');
    }

    activateTextTool() {
        this.resetCursors();
        this.canvas.defaultCursor = 'text';
        this.canvas.hoverCursor = 'text';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isTextMode');
    }

    activateEraseTool() {
        this.resetCursors();
        this.canvas.isDrawingMode = true;
        this.canvas.freeDrawingBrush = this.eraser_brush;

        this.activateDrawingMode('');
    }

    activateRemoveTool() {
        this.deselectObjects();

        this.resetCursors();
        this.canvas.hoverCursor = 'not-allowed';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isDeleteMode');
    }

    activateBucketTool() {
        this.resetCursors();
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isBucketMode');
    }

    activateRecolorTool() {
        this.resetCursors();
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isRecolorMode');
    }

    activateDrawTool() {
        this.resetCursors();
        this.canvas.freeDrawingBrush = this.free_drawing_brush;
        this.canvas.isDrawingMode = true;

        this.activateDrawingMode('');
    }

    activateColorPickerTool() {
        this.resetCursors();
        this.canvas.defaultCursor = 'crosshair';
        this.canvas.hoverCursor = 'crosshair';
        this.canvas.isDrawingMode = false;

        this.activateDrawingMode('isColorPickerMode');
    }

    disableEvent(event) {
        this.disabled_events[event] = this.canvas.__eventListeners[event];
        this.canvas.__eventListeners[event] = [];
    }

    enableEvent(event) {
        if (event in this.disabled_events) {
            this.canvas.__eventListeners[event] = this.disabled_events[event];
            delete this.disabled_events[event];
        }
    }
}