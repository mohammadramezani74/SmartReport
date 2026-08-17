window.atmCharts = {
    _instances: {},

    render: function (cassetteLabels, pickup, reject, errorLabels, errorValues, cpuPercent, ramPercent, diskPercent) {
        Object.values(this._instances).forEach(c => c.destroy());
        Chart.defaults.font.family = "Vazirmatn, sans-serif";

        this.renderGauge('cpuGauge', 'cpuGaugeValue', cpuPercent, '#5b5ff0');
        this.renderGauge('ramGauge', 'ramGaugeValue', ramPercent, '#0f6e56');
        this.renderGauge('diskGauge', 'diskGaugeValue', diskPercent, diskPercent >= 85 ? '#ef4444' : '#5b5ff0');

        this._instances.cassette = new Chart(document.getElementById('cassetteChart'), {
            type: 'bar',
            data: {
                labels: cassetteLabels, datasets: [
                    { label: 'پرداخت', data: pickup, backgroundColor: '#5b5ff0', borderRadius: 6 },
                    { label: 'رد', data: reject, backgroundColor: '#ef4444', borderRadius: 6 }
                ]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });

        this._instances.error = new Chart(document.getElementById('errorChart'), {
            type: 'doughnut',
            data: { labels: errorLabels, datasets: [{ data: errorValues, backgroundColor: ['#5b5ff0', '#0f6e56', '#eda100', '#ef4444'], borderWidth: 0 }] },
            options: { responsive: true, maintainAspectRatio: false, cutout: '65%' }
        });
    },

    renderGauge: function (canvasId, valueElId, percent, color) {
        this._instances[canvasId] = new Chart(document.getElementById(canvasId), {
            type: 'doughnut',
            data: { datasets: [{ data: [percent, 100 - percent], backgroundColor: [color, '#eceef5'], borderWidth: 0 }] },
            options: {
                responsive: true, maintainAspectRatio: false,
                circumference: 180, rotation: 270, cutout: '75%',
                plugins: { legend: { display: false }, tooltip: { enabled: false } }
            }
        });
        document.getElementById(valueElId).textContent = percent + '%';
    }
};

let errorChartInstance = null;

window.renderErrorChart = (labels, data, ids, ranges) => {
    const ctx = document.getElementById('errorChart');
    if (!ctx) return;

    // اگه قبلاً چارت بود، destroy کن
    if (errorChartInstance) {
        errorChartInstance.destroy();
        errorChartInstance = null;
    }

    errorChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'تعداد خطا',
                data: data,
                backgroundColor: data.map((_, i) =>
                    i === 0 ? '#ef4444' :   // اول قرمز
                        i === 1 ? '#f97316' :   // دوم نارنجی
                            i === 2 ? '#eab308' :   // سوم زرد
                                '#6366f1'               // بقیه بنفش
                ),
                borderRadius: 6,
                borderSkipped: false,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            onClick: (_, elements) => {
                if (elements.length > 0) {
                    const index = elements[0].index;
                    // کلیک روی ستون → رفتن به صفحه دستگاه
                    window.location.href = `/atms/${ids[index]}`;
                }
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        afterLabel: (ctx) => ranges ? ranges[ctx.dataIndex] : ''
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        font: { family: 'Vazirmatn, sans-serif', size: 12 },
                        color: '#64748b'
                    },
                    grid: { display: false }
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        font: { family: 'Vazirmatn, sans-serif', size: 12 },
                        color: '#64748b',
                        stepSize: 1
                    },
                    grid: { color: '#f1f5f9' }
                }
            }
        }
    });
};
let distributionChartInstance = null;

window.renderDistributionChart = (labels, data, colors) => {
    const ctx = document.getElementById('errorDistributionChart');
    if (!ctx) return;

    if (distributionChartInstance) {
        distributionChartInstance.destroy();
        distributionChartInstance = null;
    }

    Chart.defaults.font.family = "Vazirmatn, sans-serif";

    distributionChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            plugins: {
                legend: {
                    display: true,
                    position: 'right',
                    labels: {
                        font: { family: 'Vazirmatn, sans-serif', size: 12 },
                        boxWidth: 12,
                        padding: 15
                    }
                },
                tooltip: {
                    callbacks: {
                        label: (item) => ` تعداد خطا: ${item.raw}`
                    }
                }
            }
        }
    });
};