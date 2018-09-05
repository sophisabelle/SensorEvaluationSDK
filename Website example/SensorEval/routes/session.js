var express = require('express');
var router = express.Router();

var requests = require('../includes/functions_db');
var sessionID;

var today = new Date();

function getSessionAndPatientInfo(req, res, next) {
	sessionID = req.query.SessionID;
	var sql = "SELECT * FROM Patients, SessionSummaryAnalysis\
	WHERE SessionSummaryAnalysis.SessionID = '" + sessionID + "'\
	AND SessionSummaryAnalysis.PatientID = Patients.PatientID";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.sessionPatientInfo = rows;
			next();
		}
	});
}

function graphOne(req, res, next) {
	var sessionOverview = [{
              "name": "Active: Walking",
              "y": req.sessionPatientInfo[0].AnalysisTimeWalking,
              "selected": true
          }, {
              "name": "Active: Running",
              "y": req.sessionPatientInfo[0].AnalysisTimeRunning
          }, {
              "name": "Active: Limping",
              "y": req.sessionPatientInfo[0].AnalysisTimeLimping
          }, {
              "name": "Inactive: Standing",
              "y": req.sessionPatientInfo[0].AnalysisTimeStanding
          }, {
              "name": "Inactive: Sitting",
              "y": req.sessionPatientInfo[0].AnalysisTimeSitting
          }, {
              "name": "Inactive: Lying down",
              "y": req.sessionPatientInfo[0].AnalysisTimeLyingDown
          }];
    console.log(sessionOverview);

    req.sessionOverview = JSON.stringify(sessionOverview);
	next();

}

function graphTwo (req, res, next) {
	var times = [];
	var values = [];

	var sql = "SELECT * FROM SessionAnalysis WHERE SessionID = '" + sessionID + "'";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			for (var i = 0; i < rows.length; i++) {
				times.push(rows[i].TimeInterval);
				var val;
				if (rows[i].movement == 'Lying down') {
					val = 0;
				}
				else if (rows[i].movement == 'Sitting') {
					val = 1;
				}
				else if (rows[i].movement == 'Standing') {
					val = 2;
				}
				else if (rows[i].movement == 'Limping') {
					val = 3;
				}
				else if (rows[i].movement == 'Walking') {
					val = 4;
				}
				else if (rows[i].movement == 'Running') {
					val = 5;
				}
				values.push(val);
			}

			req.values = JSON.stringify(values);
			console.log(values);
			req.startDate = JSON.stringify(req.sessionPatientInfo[0].StartTime);

			next();
		}
	});

}


function renderSessionPage(req, res) {
	res.render('session', {
		title: 'Sensor Evaluation',
		sessionPatientInfo: req.sessionPatientInfo,
		sessionOverview: req.sessionOverview,
		values: req.values,
		startDate: req.startDate

	});
}

router.get('/', getSessionAndPatientInfo, graphOne, graphTwo, renderSessionPage);

module.exports = router;