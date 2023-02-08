AOS.init();

export function scrollToElementById(element_id) {
    let element = document.getElementById(element_id);
    setTimeout(function () {
        element.scrollIntoView({ behavior: "smooth" });
    }, 500);
}