/** Class implementing the table. */
class Table {
    /**
     * Creates a Table Object
     */
    constructor(forecastData, pollData) {
        this.forecastData = forecastData;
        this.tableData = [...forecastData];
        // add useful attributes
        for (let forecast of this.tableData)
        {
            forecast.isForecast = true;
            forecast.isExpanded = false;
        }
        this.pollData = pollData;
        this.headerData = [
            {
                sorted: false,
                ascending: false,
                key: 'state'
            },
            {
                sorted: false,
                ascending: false,
                key: 'margin',
                alterFunc: d => Math.abs(+d)
            },
            {
                sorted: false,
                ascending: false,
                key: 'winstate_inc',
                alterFunc: d => +d
            },
        ]

        this.vizWidth = 300;
        this.vizHeight = 30;
        this.smallVizHeight = 20;

        this.scaleX = d3.scaleLinear()
            .domain([-100, 100])
            .range([0, this.vizWidth]);

        this.attachSortHandlers();
        this.drawLegend();
    }

    drawLegend() {

        let marginCol = d3.select("#marginAxis");
        marginCol.attr("width", 400)
                    .attr("height", 30)
                    .append("svg").attr("id", "midLine")
                    .append("line").attr("x1", this.scaleX(0) + 50).attr("x2", this.scaleX(0) + 50)
                    .attr("y1", -10).attr("y2", 2*this.vizHeight)
                    .attr("stroke-width", 1)
                    .attr("stroke", "black");
        marginCol.append("svg").attr("id", "-75")
        .append("text").text("+75").attr("x", (this.scaleX(-75) + 50)).attr("y", 20).attr("class", "biden").classed("label", true);
        marginCol.append("svg").attr("id", "-50")
        .append("text").text("+50").attr("x", (this.scaleX(-50) + 50)).attr("y", 20).attr("class", "biden").classed("label", true);
        marginCol.append("svg").attr("id", "-25")
        .append("text").text("+25").attr("x", (this.scaleX(-25) + 50)).attr("y", 20).attr("class", "biden").classed("label", true);
        marginCol.append("svg").attr("id", "+25")
        .append("text").text("+25").attr("x", (this.scaleX(25) + 50)).attr("y", 20).attr("class", "trump").classed("label", true);
        marginCol.append("svg").attr("id", "+50")
        .append("text").text("+50").attr("x", (this.scaleX(50) + 50)).attr("y", 20).attr("class", "trump").classed("label", true);
        marginCol.append("svg").attr("id", "+75")
        .append("text").text("+75").attr("x", (this.scaleX(75) + 50)).attr("y", 20).attr("class", "trump").classed("label", true);

    }

    drawTable() {
        this.updateHeaders();
        let rowSelection = d3.select('#predictionTableBody')
            .selectAll('tr')
            .data(this.tableData)
            .join('tr');

        rowSelection.on('click', (event, d) => 
            {
                if (d.isForecast)
                {
                    this.toggleRow(d, this.tableData.indexOf(d));
                }
            });

        let forecastSelection = rowSelection.selectAll('td')
            .data(this.rowToCellDataTransform)
            .join('td')
            .attr('class', d => d.class);

        let vizSelection = forecastSelection.filter(d => d.type === 'viz');

        let textSelection = forecastSelection.filter(d => d.type === "text");

        textSelection.selectAll("text")
            .data(d => [d])
            .join("text")
            .text(d => d.value);

        let svgSelect = vizSelection.selectAll('svg')
            .data(d => [d])
            .join('svg')
            .attr('width', this.vizWidth)
            .attr('height', d => d.isForecast ? this.vizHeight : this.smallVizHeight);

        let grouperSelect = svgSelect.selectAll('g')
            .data(d => [d, d, d])
            .join('g');

        this.addGridlines(grouperSelect.filter((d,i) => i === 0), [-75, -50, -25, 0, 25, 50, 75]);
        this.addRectangles(grouperSelect.filter((d,i) => i === 1));
        this.addCircles(grouperSelect.filter((d,i) => i === 2));
    }

    rowToCellDataTransform(d) {
        let stateInfo = {
            type: 'text',
            class: d.isForecast ? 'state-name' : 'poll-name',
            value: d.isForecast ? d.state : d.name
        };

        let marginInfo = {
            type: 'viz',
            value: {
                marginLow: +d.margin_lo,
                margin: +d.margin,
                marginHigh: +d.margin_hi,
            }
        };
        let winChance;
        if (d.isForecast)
        {
            const trumpWinChance = +d.winstate_inc;
            const bidenWinChance = +d.winstate_chal;

            const trumpWin = trumpWinChance > bidenWinChance;
            const winOddsValue = 100 * Math.max(trumpWinChance, bidenWinChance);
            let winOddsMessage = `${Math.floor(winOddsValue)} of 100`
            if (winOddsValue > 99.5 && winOddsValue !== 100)
            {
                winOddsMessage = '> ' + winOddsMessage
            }
            winChance = {
                type: 'text',
                class: trumpWin ? 'trump' : 'biden',
                value: winOddsMessage
            }
        }
        else
        {
            winChance = {type: 'text', class: '', value: ''}
        }

        let dataList = [stateInfo, marginInfo, winChance];
        for (let point of dataList)
        {
            point.isForecast = d.isForecast;
        }
        return dataList;
    }

    updateHeaders() {

    }

    addGridlines(containerSelect, ticks) {

        for (let i = 0; i < ticks.length; i++) {
            if (ticks[i] === 0) {
                containerSelect.append("line")
                .attr("x1", this.scaleX(i) - 4.5)
                .attr("x2", this.scaleX(i) - 4.5)
                .attr("y1", 0)
                .attr("y2", this.vizHeight)
                .attr("stroke", "black")
                .attr("stroke-width", 1)
            }

            else 
            {
                containerSelect.append("line")
                .attr("x1", this.scaleX(ticks[i]))
                .attr("x2", this.scaleX(ticks[i]))
                .attr("y1", 0)
                .attr("y2", this.vizHeight)
                .attr("stroke", "black")
                .attr("stroke-width", 0.5)
                .attr("class", "margin-bar")
        };
        }
    }

    addRectangles(containerSelect) {

       containerSelect.selectAll("rect").remove();
        let that = this;
        let oneRect = containerSelect.filter(function(d) {
            return (d.value.marginHigh > 0 && d.value.marginLow > 0) || (d.value.marginHigh < 0 && d.value.marginLow < 0);
        });

        let twoRect = containerSelect.filter(function(d) {
            return (d.value.marginHigh > 0 && d.value.marginLow < 0);
        });

        oneRect.append("rect")
                .attr("x", function(d) {
                    return that.scaleX(d.value.marginLow);
                })
                .attr("y", 0)
                .attr("width", function(d) {
                    return that.scaleX(d.value.marginHigh) - that.scaleX(d.value.marginLow);
                })
                .attr("height", 20)
                .attr("class", function(d) {
                    if (d.value.margin > 0) {
                        return "trump";
                    }
                    else if (d.value.margin < 0) {
                        return "biden";
                    }
                })
                .attr("opacity", 0.6);

        twoRect.append("rect")
                .attr("x", d => that.scaleX(d.value.marginLow))
                .attr("y", 0)
                .attr("width", function(d) {
                    return that.scaleX(0) - that.scaleX(d.value.marginLow);
                })
                .attr("height", 20)
                .attr("class", "biden")
                .attr("opacity", 0.6);

        twoRect.append("rect")
                .attr("x", that.scaleX(0))
                .attr("y", 0)
                .attr("width", function(d) {
                    return that.scaleX(d.value.marginHigh) - that.scaleX(0);
                })
                .attr("height", 20)
                .attr("class", "trump")
                .attr("opacity", 0.6);
    }

    addCircles(containerSelect) {

        containerSelect.selectAll("circle").remove();
        let that = this;
        containerSelect.append("circle")
        .attr("cx", function(d) {
            return that.scaleX(d.value.margin);
        })
        .attr("cy", 10)
        .attr("r", function(d) {
            if (d.isForecast === undefined) {
                return 3.5;
            }
            else return 5;
        })
        .attr("class", function(d) {
            if (d.value.margin > 0) {
                return "trump";
            }
            else if (d.value.margin < 0) {
                return "biden";
            }
        })
        .attr("stroke", "black")
        .attr("stroke-width", 0.5)
    }

    attachSortHandlers() 
    {
        let that = this;
        let ths = d3.select("#columnHeaders").selectAll("th").data(that.headerData);
        ths.on("click", function(e, d) {
            that.collapseAll();
            let newD = d;
            that.tableData = that.tableData.sort(function(a, b) {
                if (newD.key === "state") {
                    newD.sorted = true;
                    let fa = a.state.toLowerCase();
                    let fb = b.state.toLowerCase();
                
                    if (newD.ascending) {
                        if (fa < fb) {
                            return 1;
                        }
                        if (fa > fb) {
                            return -1;
                        }
                        return 0;
                    }
                    else {
                        if (fa < fb) {
                            return -1;
                        }
                        if (fa > fb) {
                            return 1;
                        }
                        return 0;
                    }
                }
                else if (newD.key === "margin") {
                    newD.sorted = true;
                    if (newD.ascending) {
                        return newD.alterFunc(a.margin) - newD.alterFunc(b.margin);
                    }

                    else {
                        return newD.alterFunc(b.margin) - newD.alterFunc(a.margin);
                    }
                }

                else if (newD.key === "winstate_inc") {
                    newD.sorted = true;
                    if (newD.ascending) {
                        return newD.alterFunc(a.winstate_inc) - newD.alterFunc(b.winstate_inc);
                    }

                    else {
                        return newD.alterFunc(b.winstate_inc) - newD.alterFunc(a.winstate_inc);
                    }
                }
                
            });

            for (let i = 0; i < 3; i++) {
                if (that.headerData[i].key !== newD.key) {
                    that.headerData[i].sorted = false;

                }
            }

            if (newD.key === "state") {
                d3.select("#columnHeaders").select("#stateCol").classed("sorting", true);
                d3.select("#columnHeaders").select("#stateCol").select("i").classed("no-display", false);
                if (newD.ascending) {
                    d3.select("#columnHeaders").select("#stateCol").select("i").classed("fa-sort-up", false);
                    d3.select("#columnHeaders").select("#stateCol").select("i").classed("fa-sort-down", true);
                }
                else {
                    d3.select("#columnHeaders").select("#stateCol").select("i").classed("fa-sort-down", false);
                    d3.select("#columnHeaders").select("#stateCol").select("i").classed("fa-sort-up", true);

                }

                d3.select("#columnHeaders").select("#margCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#margCol").select("i").classed("no-display", true);

                d3.select("#columnHeaders").select("#winsCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#winsCol").select("i").classed("no-display", true);

            }

            else if (newD.key === "margin") {
                d3.select("#columnHeaders").select("#stateCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#stateCol").select("i").classed("no-display", true);
                if (newD.ascending) {
                    d3.select("#columnHeaders").select("#margCol").select("i").classed("fa-sort-up", false);
                    d3.select("#columnHeaders").select("#margCol").select("i").classed("fa-sort-down", true);
                }
                else {
                    d3.select("#columnHeaders").select("#margCol").select("i").classed("fa-sort-down", false);
                    d3.select("#columnHeaders").select("#margCol").select("i").classed("fa-sort-up", true);

                }

                d3.select("#columnHeaders").select("#margCol").classed("sorting", true);
                d3.select("#columnHeaders").select("#margCol").select("i").classed("no-display", false);

                d3.select("#columnHeaders").select("#winsCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#winsCol").select("i").classed("no-display", true);
            }

            else if (newD.key === "winstate_inc") {
                d3.select("#columnHeaders").select("#stateCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#stateCol").select("i").classed("no-display", true);
                if (newD.ascending) {
                    d3.select("#columnHeaders").select("#winsCol").select("i").classed("fa-sort-up", false);
                    d3.select("#columnHeaders").select("#winsCol").select("i").classed("fa-sort-down", true);
                }
                else {
                    d3.select("#columnHeaders").select("#winsCol").select("i").classed("fa-sort-down", false);
                    d3.select("#columnHeaders").select("#winsCol").select("i").classed("fa-sort-up", true);

                }

                d3.select("#columnHeaders").select("#margCol").classed("sorting", false);
                d3.select("#columnHeaders").select("#margCol").select("i").classed("no-display", true);

                d3.select("#columnHeaders").select("#winsCol").classed("sorting", true);
                d3.select("#columnHeaders").select("#winsCol").select("i").classed("no-display", false);
            }

            if (newD.ascending) {
                newD.ascending = false;
            }

            else {
                newD.ascending = true;
            }

            that.drawTable();
        });

    }


    toggleRow(rowData, index) {

        let chosenState = this.pollData.get(rowData.state);
        if (rowData.isExpanded) {
            this.tableData.splice(index + 1, chosenState.length);
            rowData.isExpanded = false;
        }

        else {
            for (let i = 0; i < chosenState.length; i++) {
                this.tableData.splice(index + i + 1, 0, chosenState[i]);
            }
            rowData.isExpanded = true;
        }
        
        this.drawTable();
    }

    collapseAll() {
        this.tableData = this.tableData.filter(d => d.isForecast)
    }

}
