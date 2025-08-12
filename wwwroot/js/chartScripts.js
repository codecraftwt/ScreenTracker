﻿window.renderBarChart = (labels, data) => {
    console.log("Bar Chart.js function is being called!");
    const canvas = document.getElementById('barChart');
    if (!canvas) {
        console.error('Canvas element not found!');
        return;
    }

    const ctx = canvas.getContext('2d');
    if (!ctx) {
        console.error('Context could not be obtained for the canvas!');
        return;
    }

    if (window.barChartInstance) {
        window.barChartInstance.destroy();
    }

    const today = new Date().toISOString().split('T')[0];

    const roundedBar = {
        id: 'roundedBar',
        beforeDatasetsDraw(chart) {
            const { ctx, data, chartArea: { top, bottom }, scales: { x, y } } = chart;
            ctx.save();
            data.datasets.forEach((dataset, datasetIndex) => {
                chart.getDatasetMeta(datasetIndex).data.forEach((bar, index) => {
                    const radius = 4;
                    const barX = bar.x - bar.width / 2;
                    const barY = bar.y;
                    const barWidth = bar.width;
                    const barHeight = bar.base - bar.y;

                    ctx.beginPath();
                    ctx.fillStyle = dataset.backgroundColor[index];
                    ctx.moveTo(barX, barY + radius);
                    ctx.arcTo(barX, barY, barX + barWidth, barY, radius);
                    ctx.arcTo(barX + barWidth, barY, barX + barWidth, barY + radius, radius);
                    ctx.lineTo(barX + barWidth, barY + barHeight);
                    ctx.lineTo(barX, barY + barHeight);
                    ctx.closePath();
                    ctx.fill();
                });
            });
        }
    };

    window.barChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Duration',
                data: data,
                backgroundColor: labels.map(label => label === today ? '#fdbb2d' : '#999'),
                borderColor: labels.map(label => label === today ? '#fdbb2d' : '#999'),
                borderWidth: 0,
                barThickness: 20,
                categoryPercentage: 0.6,
                barPercentage: 0.8,
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        title: (tooltipItems) => {
                            return tooltipItems[0].label === today ? "Today" : tooltipItems[0].label;
                        },
                        label: (context) => {
                            const minutes = context.raw;
                            const hours = Math.floor(minutes / 60);
                            const remainingMinutes = Math.floor(minutes % 60);
                            const seconds = Math.floor((minutes - Math.floor(minutes)) * 60);
                            return `${hours}h ${remainingMinutes}m ${seconds}s`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    display: false
                },
                x: {
                    grid: { display: false },
                    ticks: {
                        callback: (val, index) => labels[index] === today ? "Today" : ""
                    }
                }
            }
        },
        plugins: [roundedBar]
    });
};