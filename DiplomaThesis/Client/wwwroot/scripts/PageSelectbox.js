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