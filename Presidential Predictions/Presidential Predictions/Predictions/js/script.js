preGrouped = d3.json('./data/polls.json');

Promise.all([d3.csv('./data/forecasts.csv'), preGrouped]).then( data =>
    {
        console.log(data);
        let forecastData = data[0];
        let pollData = data[1];

        rolledPollData = new Map(pollData); //  convert to a Map object for consistency with d3.rollup
        let table = new Table(forecastData, rolledPollData);
        table.drawTable();
    });