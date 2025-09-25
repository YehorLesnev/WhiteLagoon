document.addEventListener("DOMContentLoaded", function () { 
    loadTotalBookingsRadialChart();
});

function loadTotalBookingsRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookingRadialChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalBookingCount").innerHTML = data.totalCount;

            const sectionnCurrentCount = document.createElement("span");

            if (data.hasRatioIncreased) {
                sectionnCurrentCount.className = "text-success me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-up-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            } else {
                sectionnCurrentCount.className = "text-danger me-1";
                sectionnCurrentCount.innerHTML = `<i class="bi bi-arrow-down-right-circle"></i> <span>${data.currentMonthCount}</span>`;
            }

            document.querySelector("#sectionBookingCount").append(sectionnCurrentCount);
            document.querySelector("#sectionBookingCount").append("since last month");

            $(".chart-spinner").hide();
        }});
}