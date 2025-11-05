chartTypeRanges = new Object();

charts = [];
function calculateRssiYRange(chartType, range) {

    if (isNaN(range.min) && isNaN(range.max)) {
        return { min: range.min, max: range.max };
    }

    if (chartTypeRanges[chartType] == undefined) {
        chartTypeRanges[chartType] = { min: range.min, max: range.max };
    }

    if (range.min < chartTypeRanges[chartType].min) {
        chartTypeRanges[chartType].min = range.min;
    }

    if (range.max > chartTypeRanges[chartType].max) {
        chartTypeRanges[chartType].max = range.max;
    }

    return { min: chartTypeRanges[chartType].min, max: chartTypeRanges[chartType].max };
}

function createRssiSmootheChart(chartType, elementId, minvalue, maxvalue) {

    var chart = new SmoothieChart(
        {
            millisPerPixel: 58,
            responsive: true,
            grid: { fillStyle: 'transparent', strokeStyle: 'transparent' },
            yRangeFunction: (range) => calculateRssiYRange(chartType, range),
            horizontalLines: []
        });

    var element = document.getElementById(elementId)

    chartTypeRanges[chartType] = { min: minvalue, max: maxvalue };

    chart.streamTo(element, 100);
    charts.push(chart);
    return chart;
}

function addHorizontalLine(chart, value, colour, width) {

    var line = { lineWidth: width, value: value, color: colour, originalLineWidth: width };

    chart.options.horizontalLines.push(line);

    return line;
}
function showHideHorizontalLine(line, show) {

    line.lineWidth = show ? line.originalLineWidth : 0;
}

function setLineValue(line, value) {

    line.value = value;
}

function addRssiLine(chart) {
    var line = new TimeSeries();
    chart.addTimeSeries(line, { lineWidth: 2, strokeStyle: '#0080ff', fillStyle: 'rgba(0,0,0,0.17)' });
    return line;
}

function addValueToLine(line, time, value) {
    line.append(Date.now(), value);
}