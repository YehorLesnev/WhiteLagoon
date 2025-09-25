document.addEventListener("DOMContentLoaded", function () {
    loadTotalRevenueRadialChart();
});

function loadTotalRevenueRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetRevenueChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalRevenueCount").innerHTML = data.totalCount;

            const sectionnCurrentCount = document.createElement("span");

            if (data.hasRatioIncreased) {
                sectionnCurrentCount.className = "text-success me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-up-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            } else {
                sectionnCurrentCount.className = "text-danger me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-down-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            }

            document.querySelector("#sectionRevenueCount").append(sectionnCurrentCount);
            document.querySelector("#sectionRevenueCount").append("since last month");

            loadRadialBarChart("totalRevenueRadialChart", data);

            $(".chart-spinner").hide();
        }
    });
}