chartTypeRanges = new Object();

function calculateRssiYRange(chartType, range) {

    if (isNaN(range.min) && isNaN(range.max)) {
        return { min: range.min, max: range.max };
    }

    if (chartTypeRanges[chartType] == undefined) {
        console.log("new range:" + range.min + "," + range.max);
        chartTypeRanges[chartType] = { min: range.min, max: range.max };
    }

    if (range.min < chartTypeRanges[chartType].min) {
        console.log("new min:" + range.min);
        chartTypeRanges[chartType].min = range.min;
    }

    if (range.max > chartTypeRanges[chartType].max) {
        console.log("new max:" + range.max);
        chartTypeRanges[chartType].max = range.max;
    }

    return { min: chartTypeRanges[chartType].min, max: chartTypeRanges[chartType].max };
}

function createRssiSmootheChart(chartType,elementId) {
    var chart = new SmoothieChart(
        {
            millisPerPixel: 58,
            responsive: true,
            grid: { fillStyle: 'transparent', strokeStyle: 'transparent' },
            yRangeFunction: (range) => calculateRssiYRange(chartType, range)
        });

    var element = document.getElementById(elementId)

    chart.streamTo(element,100);

    return chart;
}

function addRssiLine(chart) {
    var line = new TimeSeries();
    chart.addTimeSeries(line, { lineWidth: 2, strokeStyle: '#0080ff', fillStyle: 'rgba(0,0,0,0.17)' });
    return line;
}

function addValueToLine(line, time, value) {
    line.append(Date.now(), value);
}