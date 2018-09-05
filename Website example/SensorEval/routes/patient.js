var express = require('express');
var router = express.Router();

var requests = require('../includes/functions_db');
var patientID;

var today = new Date();

function getPatientInfo(req, res, next) {
	patientID = req.param('PatientID');
	var sql = "SELECT * FROM Patients WHERE PatientID = " + patientID;
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.patientInfo = rows;
			next();
		}
	});
}

function graphOne(req, res, next) {

	var date = new Date();
	var m = date.getMonth();
	var months = [];
	var numbers = [0,0,0,0,0,0,0,0,0,0,0,0];
	var monthname = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
	for (var i = 0; i <= 11; i++) {
		var index = (m+1+i)%12;
		months.push(monthname[index]);
	}
	var sql = "SELECT MONTH(DateTimeUploaded) AS MonthUploaded, Year(DateTimeUploaded) AS YearUploaded, COUNT(*) AS NumberOfUploads \
	FROM SessionSummaryAnalysis \
	WHERE DateTimeUploaded > EOMONTH (GETDATE(), -11)  AND PatientID = " + patientID + "\
	GROUP BY MONTH(DateTimeUploaded), YEAR(DateTimeUploaded)\
	ORDER BY YearUploaded, MonthUploaded";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.monthInfo = rows;

			for (var i = 0; i < rows.length; i++) {
				var month = rows[i].MonthUploaded;
				var value = rows[i].NumberOfUploads;
				var index = (11-m+month-1)%12;
				console.log("month: "+month+ " current month: "+m)
				numbers[index] = value;
			}
		}

		var numberUploads = [
			{
				"name": req.patientInfo[0].Name,
				"data": numbers
			}
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
		WHERE SessionSummaryAnalysis.PatientID IS NULL OR SessionSummaryAnalysis.PatientID = "+ patientID +"\
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

		var dayUploads = [
			{
				"name": req.patientInfo[0].Name,
				"data": numbersarray
			}
		];

		console.log(dayUploads);
		req.days = JSON.stringify(datesarray);
		req.dayUploads = JSON.stringify(dayUploads);

		next();

	});

}

function graphThree(req,res,next) {
	sql="SELECT TOP 25 CONVERT(date, DateTimeUploaded) AS Date, COUNT(CONVERT(date, DateTimeUploaded)) AS NumberOfUploads, CONVERT(decimal(10,2),SUM(AnalysisTimeActive)/3600) AS AnalysisTimeActive,\
    CONVERT(decimal(10,2),SUM(AnalysisTotalTime)/60) AS AnalysisTotalTime, CONVERT(decimal(10,2),SUM(AnalysisTimeLyingDown)/60) AS AnalysisTimeLyingDown,\
    CONVERT(decimal(10,2),SUM(AnalysisTimeStanding)/60) AS AnalysisTimeStanding,  CONVERT(decimal(10,2),SUM(AnalysisTimeSitting)/60) AS AnalysisTimeSitting, CONVERT(decimal(10,2),SUM(AnalysisTimeWalking)/60) AS AnalysisTimeWalking,\
    CONVERT(decimal(10,2),SUM(AnalysisTimeLimping)/60) AS AnalysisTimeLimping, CONVERT(decimal(10,2),SUM(AnalysisTimeRunning)/60) AS AnalysisTimeRunning\
	FROM SessionSummaryAnalysis\
	WHERE SessionSummaryAnalysis.PatientID =  " + patientID +"\
	GROUP BY CONVERT(date, DateTimeUploaded)\
	ORDER BY CONVERT(date, DateTimeUploaded) ASC";

	alldates = [];
	timeActive = [];
	totalTime = [];
	timeLyingDown = [];
	timeSitting = [];
	timeStanding = [];
	timeWalking = [];
	timeLimping = [];
	timeRunning =[];
	stepsPerSecond = [];
	timeNotActive = [];

	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {

			for (var i = 0; i < rows.length; i++) {
			    alldates.push(rows[i].Date.toString().slice(0,15));
				timeActive.push(rows[i].AnalysisTimeActive);
				totalTime.push(rows[i].AnalysisTotalTime);
				timeLyingDown.push(rows[i].AnalysisTimeLyingDown);
				timeSitting.push(rows[i].AnalysisTimeSitting);
				timeStanding.push(rows[i].AnalysisTimeStanding);
				timeWalking.push(rows[i].AnalysisTimeWalking);
				timeLimping.push(rows[i].AnalysisTimeLimping);
				timeRunning.push(rows[i].AnalysisTimeRunning);
				timeNotActive.push(rows[i].AnalysisTotalTime - rows[i].AnalysisTimeActive);
			}

		}

		var activity = [ {
			"name": "Active: Running",
			"data": timeRunning,
			"stack": "active",
			"color": "lightgreen"
		},
		{
			"name": "Active: Walking",
			"data": timeWalking,
			"stack": "active",
			"color": "green"
		},
		{
			"name": "Active: Limping",
			"data": timeLimping,
			"stack": "active",
			"color": "brown"
		},
		{
			"name": "Inactive: Sitting",
			"data": timeSitting,
			"stack": "inactive",
			"color": "lightblue"
		},
		{
			"name": "Inactive: Standing",
			"data": timeStanding,
			"stack": "inactive",
			"color": "blue"
		},
		{
			"name": "Inactive: Lying Down",
			"data": timeLyingDown,
			"stack": "inactive",
			"color": "darkblue"
		}];

		req.activity = JSON.stringify(activity);
		console.log(activity);
		req.alldates = JSON.stringify(alldates);
		console.log(alldates);

		next();

	});
}

function getAllUploads(req, res, next) {
	var sql = "SELECT *\
	FROM SessionSummaryAnalysis\
	LEFT JOIN Patients ON SessionSummaryAnalysis.PatientID = Patients.PatientID\
	WHERE SessionSummaryAnalysis.PatientID =  " + patientID +"\
	ORDER BY DateTimeUploaded DESC";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.allUploads = rows;
			next();
		}
	});
}

function renderPatientPage(req, res) {
	res.render('patient', {
		title: 'Sensor Evaluation',
		patientInfo: req.patientInfo,
		numberUploads: req.numberUploads,
		months: req.months,
		dayUploads: req.dayUploads,
		days: req.days,
		activity: req.activity,
		alldates: req.alldates,
		allUploads: req.allUploads
	});
}

router.get('/', getPatientInfo, graphOne, graphTwo, graphThree, getAllUploads, renderPatientPage);

/* GET home page. */
//router.get('/', function(req, res, next) {
	// get PatientID from url

	// get relevant data from database 

	//let name = {first: 'Sophiaa', last:'botz'};
  	//res.render('patient', { title: 'Patient', name: 'Sop', patientID: patientID });
 // });

router.post('/', function(req, res) {
});

module.exports = router;
