var wrapper, marWrapper, table_rows, marRows = [], row_height = 60, patient_blocks, offsetStart, vertHighlight;
var STATUS = {
    NORMAL: "normal",
    ACKNOWLEDGED: "acknowledged",
    GIVEN: "given",
    CANCELLED: "cancelled",
    OVERDUE: "overdue"
};
var orders = {
    stat: [
        null,
        {
            status: STATUS.ACKNOWLEDGED,
            count: 5
        },
        null,
        {
            status: STATUS.NORMAL,
            count: 2
        },
        {
            status: STATUS.NORMAL
        },
        null,
        {
            status: STATUS.ACKNOWLEDGED,
            count: 3
        },
        null,
        null,
        null,
        {
            status: STATUS.ACKNOWLEDGED,
            count: 2
        },
        null
    ],
    prn: [
        null,
        null,
        {
            status: STATUS.NORMAL
        },
        null,
        null,
        null,
        {
            status: STATUS.NORMAL
        },
        {
            status: STATUS.NORMAL
        },
        null,
        null,
        null
    ],
    scheduled: [
        [
            {
                status: STATUS.OVERDUE,
                dueIn: 34
            },
            {
                status: STATUS.ACKNOWLEDGED,
                count: 2,
                dueIn: 110
            },
            {
                status: STATUS.ACKNOWLEDGED,
                dueIn: 150
            },
            {
                status: STATUS.NORMAL,
                count: 3,
                dueIn: 210
            },
            {
                status: STATUS.NORMAL,
                dueIn: 310
            },
            {
                status: STATUS.NORMAL,
                count: 2,
                dueIn: 445
            }
        ],
        [
            {
                status: STATUS.ACKNOWLEDGED,
                dueIn: 90
            },
            {
                status: STATUS.NORMAL,
                count: 2,
                dueIn: 210
            },
            {
                status: STATUS.NORMAL,
                dueIn: 300
            },
            {
                status: STATUS.NORMAL,
                count: 3,
                dueIn: 430
            }
        ],
        [
            {
                status: STATUS.ACKNOWLEDGED,
                dueIn: 120
            },
            {
                status: STATUS.NORMAL,
                count: 5,
                dueIn: 300
            },
            {
                status: STATUS.NORMAL,
                dueIn: 330
            },
            {
                status: STATUS.NORMAL,
                dueIn: 360,
                synced: true
            },
            {
                status: STATUS.NORMAL,
                dueIn: 467
            },
        ],
        [
            {
                status: STATUS.NORMAL,
                dueIn: 210
            },
            {
                status: STATUS.NORMAL,
                dueIn: 390
            }
        ],
        [
            {
                status: STATUS.NORMAL,
                dueIn: 240
            },
            {
                status: STATUS.NORMAL,
                dueIn: 400,
                count: 3
            }],
        [{
            status: STATUS.OVERDUE,
            dueIn: 10
        },
            {
                status: STATUS.NORMAL,
                dueIn: 150
            },
            {
                status: STATUS.NORMAL,
                dueIn: 240
            },
            {
                status: STATUS.NORMAL,
                dueIn: 380
            }],
        [
            {
                status: STATUS.ACKNOWLEDGED,
                dueIn: 120
            },
            {
                status: STATUS.NORMAL,
                dueIn: 270,
                count: 2
            },
            {
                status: STATUS.NORMAL,
                dueIn: 390
            }],
        null,
        null,
        [
            {
                status: STATUS.OVERDUE,
                dueIn: 20
            },
            {
                status: STATUS.ACKNOWLEDGED,
                dueIn: 120
            },
            {
                status: STATUS.NORMAL,
                dueIn: 300,
                count: 5
            },
            {
                status: STATUS.NORMAL,
                dueIn: 330
            },
            {
                status: STATUS.NORMAL,
                dueIn: 420
            }],
        null,
        null
    ],
    continous: [
        null,
        null,
        null,
        {
            status: STATUS.ACKNOWLEDGED
        },
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        {
            status: STATUS.NORMAL
        }
    ],
    timed: []
};

function createMAR(targetId) {
    setTimeout(function () {
        initMARgrid(targetId);
        setTimeout(function () {
            generateMARcolumns();
        }, 0);
    }, 1000);
}

function initMARgrid(targetId) {
    wrapper = document.getElementById(targetId);
    marWrapper = document.getElementById("marWrapper");
    table_rows = wrapper.querySelectorAll(".e-content table tbody tr");
    patient_blocks = Array.from(wrapper.querySelectorAll(".patient-block"));
    offsetStart = patient_blocks.reduce(function (max, val) {
        if (val.clientWidth > max) {
            return val.clientWidth;
        }
        return max;
    }, 0);
    marWrapper.style.left = offsetStart + 50 + "px";
    table_rows.forEach(function () {
        var marRow = document.createElement("div");
        marRow.className = "marRow";
        var idx = marRows.length;
        marRow.addEventListener("click", function (ev) {
            ev.stopPropagation();
            openPatient(idx);
        });
        marRow.addEventListener("mouseover", function (ev) {
            ev.stopPropagation();
            doHighlight(idx);
        });
        marRow.addEventListener("mouseleave", function (ev) {
            ev.stopPropagation();
            clearHighlight();
        });
        marRows.push(marRow);
        marWrapper.appendChild(marRow);
    });
    vertHighlight = document.createElement("div");
    vertHighlight.className = "vr_highlight";
    marWrapper.appendChild(vertHighlight);
    setTimeout(function () {
        marWrapper.classList.add("ready");
    }, 0);
}

function generateMARcolumns() {
    drawTimeline();
    drawNow();
    drawSpecial("STAT", orders.stat, 0);
    drawSpecial("PRN", orders.prn, 1);
    drawOrders(orders.scheduled);
}

function drawSpecial(name, ors, pos) {
    var offset = pos * 50;
    var vr = document.createElement("span");
    vr.className = "vr special";
    var title = document.createElement("span");
    title.className = "title";
    title.innerText = name;
    vr.appendChild(title);
    ors.forEach(function (o, idx) {
        var newOrder = document.createElement("span");
        var type = o ? o.status : "dummy";
        newOrder.className = "orderSpecial " + type;
        newOrder.style.height = row_height;
        newOrder.dataset.popover = "order-popover";
        if (o && o.count) {
            var newOrderCount = document.createElement("span");
            newOrderCount.innerText = o.count;
            newOrderCount.className = "counter";
            newOrder.appendChild(newOrderCount);
        }
        newOrder.addEventListener("click", function (ev) {
            ev.stopPropagation();
            openPatient(idx);
        });
        newOrder.addEventListener("mouseover", function (ev) {
            ev.stopPropagation();
            doHighlight(idx, newOrder, offset - 20);
            if (o) {
                setPopoverContent(o, "special");
                $(newOrder).gpopover("show");
            }
        });
        newOrder.addEventListener("mouseleave", function (ev) {
            ev.stopPropagation();
            clearHighlight();
            if (o) {
                $(newOrder).gpopover("hide");
            }
        });
        vr.appendChild(newOrder);
    });
    vr.style.left = offset + "px";
    marWrapper.appendChild(vr);
}

function drawNow() {
    var offset = 220;
    var now = new Date();
    var hour = now.getHours();
    var min = now.getMinutes();
    var vr = document.createElement("span");
    vr.className = "vr now";
    var title = document.createElement("span");
    title.className = "title";
    title.innerText = (hour <= 9 ? "0" + hour : hour) + "" + (min <= 9 ? "0" + min : min);
    vr.appendChild(title);
    vr.style.left = offset + 2 * min + "px";
    marWrapper.appendChild(vr);
}

function drawTimeline() {
    var offset = 100;
    var hour = (new Date()).getHours() - 1;
    for (var i = 0; i < 23; i++) {
        var vr = document.createElement("span");
        vr.className = "vr time";
        var title = document.createElement("span");
        title.className = "title";
        title.innerText = (hour <= 9 ? "0" + hour : hour) + "00";
        hour += 1;
        if (hour > 23) {
            hour = 0;
        }
        vr.appendChild(title);
        vr.style.left = offset + 120 * i + "px";
        marWrapper.appendChild(vr);
    }
}

function drawOrders(ors) {
    marRows.forEach(function (target, idx) {
        drawOrdersRow(ors[idx], target, idx);
    });
}

function drawOrdersRow(orders, row, idx) {
    if (!orders) {
        return;
    }
    orders.forEach(function (o) {
        var newOrder = document.createElement("span");
        newOrder.className = "order " + o.status;
        newOrder.style.left = 2 * o.dueIn + "px";
        newOrder.dataset.popover = "order-popover";
        if (o && o.count) {
            var newOrderCount = document.createElement("span");
            newOrderCount.innerText = o.count;
            newOrderCount.className = "counter";
            newOrder.appendChild(newOrderCount);
        }
        newOrder.addEventListener("click", function (event) {
            event.stopPropagation();
            // openPatient(idx);
        });
        newOrder.addEventListener("mouseover", function (event) {
            event.stopPropagation();
            doHighlight(idx, newOrder, 95);
            setPopoverContent(o, "scheduled");
            $(newOrder).gpopover("show");
        });
        newOrder.addEventListener("mouseleave", function (event) {
            event.stopPropagation();
            $(newOrder).gpopover("hide");
        });
        row.appendChild(newOrder);
    });
}

function doHighlight(rowIdx, target, offset) {
    if (table_rows && table_rows[rowIdx]) {
        table_rows.forEach(function (row) {
            row && row.classList.remove("marHighlight");
        });
        table_rows[rowIdx].classList.add("marHighlight");
    }
    if (target) {
        vertHighlight.classList.add("active");
        vertHighlight.style.left = target.offsetLeft + (offset || 0) + "px";
    }
}

function clearHighlight() {
    if (table_rows) {
        table_rows.forEach(function (row) {
            row && row.classList.remove("marHighlight");
        });
    }
    vertHighlight.classList.remove("active");
}

function openPatient(id) {
    window.location.href = "/PatientDetails/GetPatient?patientId=" + (id + 1);
}

function setPopoverContent(order, type) {
    var pvr = document.getElementById("order-popover");
    var status = pvr.querySelector(".p_status");
    status.innerHTML = order.status === "normal" ? "" : order.status;
    status.className = "p_status " + order.status;

    var lists = pvr.querySelectorAll(".p_orders_list");
    lists.forEach(function (l) {
        l.classList.remove("active");
    });
    var alist = pvr.querySelector(".p_orders_list." + type);
    alist.classList.add("active");
    alist.querySelectorAll("li").forEach(function (el, idx) {
        var c = order.count || 1;
        if (idx < c) {
            el.classList.remove("hidden");
        }
        if (idx >= c) {
            el.classList.add("hidden");
        }
    })
}
