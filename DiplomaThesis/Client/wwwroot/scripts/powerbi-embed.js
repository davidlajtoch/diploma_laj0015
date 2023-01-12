export function embedReport(reportContainer, reportId, embedUrl, token) {
    // Embed report using the Power BI JavaScript API.
    var models = window['powerbi-client'].models

    var config = {
        type: 'report',
        id: reportId,
        embedUrl: embedUrl,
        accessToken: token,
        permissions: models.Permissions.All,
        tokenType: models.TokenType.Embed,
        viewMode: models.ViewMode.View,
        settings: {
            navContentPaneEnabled: false,
            background: models.BackgroundType.Transparent,
            panes: {
                filters: {expanded: false, visible: true},
                pageNavagation: {visible: false}
            }
        }
    }
    // Embed the report and display it within the div container
    powerbi.embed(reportContainer, config)  
}

export function removeReportBorder() {
    var iframes = document.getElementsByTagName("iframe");
    for (var i = 0; i < iframes.length; i++) {
        iframes[i].setAttribute("frameborder", "0");
    }
}