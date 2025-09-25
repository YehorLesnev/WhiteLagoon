document.addEventListener("DOMContentLoaded", function () {
    loadUserRadialChart();
});

function loadUserRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetRegisteredUserChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalUserCount").innerHTML = data.totalCount;

            const sectionnCurrentCount = document.createElement("span");

            if (data.hasRatioIncreased) {
                sectionnCurrentCount.className = "text-success me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-up-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            } else {
                sectionnCurrentCount.className = "text-danger me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-down-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            }

            document.querySelector("#sectionUserCount").append(sectionnCurrentCount);
            document.querySelector("#sectionUserCount").append("since last month");

            loadRadialBarChart("totalUserRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}