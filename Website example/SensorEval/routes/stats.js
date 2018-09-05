var express = require('express');
var router = express.Router();

var requests = require('../includes/functions_db');

var today = new Date();

function graphOne(req, res, next) {
	var months = [];
	var numbers = [0,0,0,0,0,0,0,0,0,0,0,0];
	var m = today.getMonth();
	var monthname = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
	for (var i = 0; i <= 11; i++) {
		var index = (m+1+i)%12;
		months.push(monthname[index]);
	}

	var sql = "SELECT MONTH(DateTimeUploaded) AS MonthUploaded, Year(DateTimeUploaded) AS YearUploaded, COUNT(*) AS NumberOfUploads \
	FROM SessionSummaryAnalysis \
	WHERE DateTimeUploaded > EOMONTH (GETDATE(), -11) \
	GROUP BY MONTH(DateTimeUploaded), YEAR(DateTimeUploaded)\
	ORDER BY YearUploaded, MonthUploaded";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.monthInfo = rows;

			for (var i = 0; i < rows.length; i++) {
				var month = rows[i].MonthUploaded;
				var value = rows[i].NumberOfUploads;
				var index = (11-m+month-1)%12;
				numbers[index] = value;
			}
		}

		var numberUploads = [
		{
			"name": "All patients",
			"data": numbers
		},
		//{
			//"name": "John",
			//"data": [5, 7, 3]
		//}
		];
		req.months = JSON.stringify(months);
		req.numberUploads = JSON.stringify(numberUploads);

		next();

	});

}

function graphTwo(req, res, next) {
	var sql = "WITH Dates AS (\
        SELECT\
         [Date] = CONVERT(DATE ,DATEADD(month, -1,GETDATE()))\
        UNION ALL SELECT\
         [Date] = DATEADD(DAY, 1, [Date])\
        FROM\
         Dates\
        WHERE\
         Date < GETDATE()\
		) SELECT\
		 [Date] AS Date, COUNT(CONVERT(date, DateTimeUploaded)) AS NumberOfUploads\
		FROM\
		 Dates\
		LEFT JOIN SessionSummaryAnalysis ON Dates.[Date] = CONVERT(date, SessionSummaryAnalysis.DateTimeUploaded)\
		GROUP BY [Date], CONVERT(date, DateTimeUploaded)\
		ORDER BY [Date] ASC\
		OPTION (MAXRECURSION 45)";

	var datesarray = [];
	var numbersarray = [];

	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {

			for (var i = 0; i < rows.length; i++) {
			    datesarray.push(rows[i].Date.toString().slice(0,15));
			    numbersarray.push(rows[i].NumberOfUploads);
			}

		}

		console.log(datesarray);
		console.log(numbersarray);

		var dayUploads = [
			{
				"name": "All patients",
				"data": numbersarray
			}
		];
		req.days = JSON.stringify(datesarray);
		req.dayUploads = JSON.stringify(dayUploads);

		next();

	});

}


function renderStatsPage(req, res) {

	res.render('stats', {
		title: 'Sensor Evaluation',
		numberUploads: req.numberUploads,
		months: req.months,
		dayUploads: req.dayUploads,
		days: req.days
	});
}

router.get('/', graphOne, graphTwo, renderStatsPage);

module.exports = router;