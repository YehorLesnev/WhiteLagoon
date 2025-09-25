document.addEventListener("DOMContentLoaded", function () {
    loadTotalBookingsRadialChart();
});

function loadTotalBookingsRadialChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetBookingPieChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {

            loadPieChart("customerBookingPieChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadPieChart(id, data) {
    const chartColors = getChartColorsArray(id);

    const options = {
        series: data.series,
        labels: data.labels,
        colors: chartColors,
        chart: {
            type: 'pie',
            height: 380
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: '#fff',
                useSeriesColors: true
            },
        },
    };

    const chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}