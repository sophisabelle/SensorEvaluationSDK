var express = require('express');
var router = express.Router();


/* GET home page. */

var requests = require('../includes/functions_db');

function getLatestUploads(req, res, next) {
	var sql = "SELECT TOP 20 *\
	FROM SessionSummaryAnalysis\
	ORDER BY DateTimeUploaded DESC";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.latestUploads = rows;
			next();
		}
	});
}

function getNewlyRegistered(req, res, next) {
	sql = "SELECT TOP 20 * FROM Patients ORDER BY DateRegistered DESC";
	requests.connectAndQuery(sql, function(rows) {
		if(rows.length !== 0) {
			req.newlyRegistered = rows;
			next();
		}
	});
}

function renderHomePage(req, res) {
	res.render('index', {
		title: 'Sensor Evaluation',
		latestUploads: req.latestUploads,
		newlyRegistered: req.newlyRegistered
	});
}

router.get('/', getLatestUploads, getNewlyRegistered, renderHomePage);

module.exports = router;