document.addEventListener("DOMContentLoaded", function () {
    loadTotalCustomersAndBookingsLineChart();
});

function loadTotalCustomersAndBookingsLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/GetMemberAndBookingLineChartData",
        type: "GET",
        dataType: "json",
        success: function (data) {

            loadLineChart("newCustomersAndBookingsLineChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadLineChart(id, data) {
    const chartColors = getChartColorsArray(id);

    const options = {
        series: data.series,
        colors: chartColors,
        chart: {
            type: 'line',
            height: 380
        },
        stroke: {
            curve: 'smooth'
        },
        markers: {
            size: 3,
            strokeWidth: 0,
            hover: {
                size: 7
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors: '#ddd'
                }
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: '#ddd'
                }
            }
        },
        legend: {
            labels: {
                colors: '#ddd'
            }
        },
        tooltip: {
            theme: 'dark'
        }
    };

    const chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}