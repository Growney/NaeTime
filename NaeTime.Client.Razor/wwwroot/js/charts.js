
function createRssiSmootheChart(elementId) {
    var chart = new SmoothieChart({ millisPerPixel: 58, maxValue: 70000, minValue: 25000 });

    var element = document.getElementById(elementId)

    chart.streamTo(element);

    return chart;
}

function addRssiLine(chart) {
    var line = new TimeSeries();
    chart.addTimeSeries(line);
    return line;
}

function addValueToLine(line, time, value) {
    line.append(Date.now(), value);
}