let dataTable;

document.addEventListener("DOMContentLoaded", function () {
    loadDataTable();
});

function loadDataTable() {
    const bookingTable = document.getElementById("tblBookings");

    if(!bookingTable)
        return;

    dataTable = $('#tblBookings').DataTable({
        "ajax": {
            "url": "/booking/getall",
        },
        columns: [
            { data: "id", width: "3%" },
            { data: "name", width: "10%" },
            { data: "phoneNumber", width: "5%" },
            { data: "email", width: "15%" },
            { data: "status", width: "8%" },
            { data: "checkInDate", width: "7%" },
            { data: "nights", width: "5%" },
            { data: "totalCost", render: $.fn.dataTable.render.number(',', '.', 2), width: "5%" },
            { 
                data: "id",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/booking/details?bookingId=${data}" class="btn btn-success text-white mx-2" style="cursor:pointer;">
                                <i class="bi bi-pencil-square"></i> Details
                            </a>
                        </div>`;
                },
                width: "10%"
            }
        ],
    });
}